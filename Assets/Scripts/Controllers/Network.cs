using System;
using System.Collections.Generic;
using System.Text;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Network : MonoBehaviour
{
    private WebSocket ws;

    internal bool IsConnected
    {
        get;
        private set;
    }

    public string Id
    {
        get;
        private set;
    }

#if !UNITY_EDITOR
    private const string url = "ws://localhost:8080";
#else
    private const string url = "wss://shooter-jabbar.herokuapp.com";
#endif
    
    async void Start()
    {
        ws = WebSocketFactory.CreateInstance(url);
        
        Id = Guid.NewGuid().ToString();
        ws.OnMessage += (message) =>
        {
            var json = Encoding.UTF8.GetString(message);
            var tmp = JsonUtility.FromJson<Json<object>>(json);
            if (tmp.message == "get_map")
            {
                SendMapData(tmp.id);
            }
            else
            {
                IJson data;
                var webMessageReceivedEvent = GameManager.Instance.Events.Get<WebMessageReceivedEvent>();
                if (tmp.message == "move" || tmp.message == "join" || tmp.message == "fire")
                {
                    data = JsonUtility.FromJson<Json<Vector2>>(json);
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
                }
                else if (tmp.message == "map")
                {
                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var positions =
                        JsonConvert.DeserializeObject<Dictionary<string, Vector2>>(dictionary["data"].ToString());
                    webMessageReceivedEvent.Set(tmp.id, tmp.message, positions);
                }
                else
                {
                    data = tmp;
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
                }

                GameManager.Instance.Events.FireEvent(typeof(WebMessageReceivedEvent));
            }
        };

        ws.OnOpen += () =>
        {
            IsConnected = true;
            Debug.Log("Connecting with: " + Id);
        };
        
        await ws.Connect();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (IsConnected && hasFocus)
        {
            GetMapData();
        }
    }

    async void GetMapData()
    {
        Json<string> data;
        data.id = Id;
        data.message = "get_map";
        data.data = "";
        await ws.SendText(JsonUtility.ToJson(data));
    }

    async void SendMapData(string id)
    {
        Json<string> map;
        map.id = id;
        map.message = "map";

        var positions = new Dictionary<string, string>();
        int index = 0;
        foreach (var (playerId, player) in GameManager.Instance.Players)
        {
            var transform = player.transform;
            var playerPosition = transform.position;
            var position = new Vector2(playerPosition.x, playerPosition.z);

            positions.Add(playerId, JsonUtility.ToJson(position));
        }

        var dataString = JsonConvert.SerializeObject(positions);
        map.data = dataString;
        var mapString = JsonConvert.SerializeObject(map);
        await ws.SendText(mapString);
    }

    void Update()
    {
    #if UNITY_EDITOR || UNITY_STANDALONE_LINUX
        ws.DispatchMessageQueue();
    #endif
        if (Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            Join();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JoinBot();
        }
    }
    
    private async void JoinBot()
    {
        Json<object> json;
        json.id = Guid.NewGuid().ToString();
        json.message = "join_bot";
        json.data = "";
        var data = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(data));
    }

    private async void Join()
    {
        Json<object> json;
        json.id = Id;
        json.message = "join";
        json.data = "";
        var data = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(data));
    }

    private async void OnApplicationQuit()
    {
        await ws.Close();
    }

    public async void SendMove(Vector2 movement)
    {
        Json<Vector2> json;
        json.id = Id;
        json.message = "move";
        json.data = movement;
        var jsonData = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(jsonData));
    }

    public async void SendFire(Vector2 direction)
    {
        Json<Vector2> data;
        data.id = Id;
        data.message = "fire";
        data.data = direction;
        await ws.SendText(JsonUtility.ToJson(data));
    }
    
    public interface IJson
    {
        string GetId();
        string GetMessage();
        object GetData();
    }

    public struct Json<T> : IJson
    {
        public string message;
        public string id;
        public T data;
        public string GetMessage()
        {
            return message;
        }

        public string GetId()
        {
            return id;
        }

        public object GetData()
        {
            return data;
        }
    }
}
