using System;
using System.Collections.Generic;
using System.Reflection;
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

    [SerializeField] private URL url;


    async void Start()
    {
        ws = WebSocketFactory.CreateInstance(url.GetStringValue());
        
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
                if (tmp.message == "move" || 
                    tmp.message == "join" || 
                    tmp.message == "fire" ||
                    tmp.message == "sync_position" ||
                    tmp.message == "respawn")
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
                else if (tmp.message == "spawn_coin")
                {
                    var coinData = JsonConvert.DeserializeObject<Json<Dictionary<string, Vector2>>>(json);
                    webMessageReceivedEvent.Set(tmp.id, tmp.message, coinData.data);
                }
                else if (tmp.message == "bullet_hit" ||
                         tmp.message == "coin_pick")
                {
                    data = JsonUtility.FromJson<Json<string>>(json);
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
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
        if (!IsConnected) return;
        Debug.Log("get_map");
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
            if (!GameManager.Instance.IsInGameRoom)
            {
                Join();
            }
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JoinBot();
        }
#endif
    }
    
    private async void JoinBot()
    {
        if (!IsConnected) return;
        Json<object> json;
        json.id = Guid.NewGuid().ToString();
        json.message = "join_bot";
        json.data = "";
        var data = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(data));
    }

    private async void Join()
    {
        if (!IsConnected) return;
        Json<object> json;
        json.id = Id;
        json.message = "join";
        json.data = "";
        var data = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(data));
    }

    public async void SendMove(Vector2 movement)
    {
        if (!IsConnected) return;
        Json<Vector2> json;
        json.id = Id;
        json.message = "move";
        json.data = movement;
        var jsonData = JsonUtility.ToJson(json);
        await ws.Send(Encoding.UTF8.GetBytes(jsonData));
    }

    public async void SendFire(Vector2 direction)
    {
        if (!IsConnected) return;
        Json<Vector2> data;
        data.id = Id;
        data.message = "fire";
        data.data = direction;
        await ws.SendText(JsonUtility.ToJson(data));
    }

    public async void SendCoinPickUp(string coinId)
    {
        if (!IsConnected) return;
        Json<string> data;
        data.id = Id;
        data.message = "coin_pick";
        data.data = coinId;
        await ws.SendText(JsonUtility.ToJson(data));
    }
    
    public async void SendBulletHit(string victimId)
    {
        if (!IsConnected) return;
        Json<string> data;
        data.id = Id;
        data.message = "bullet_hit";
        data.data = victimId;
        await ws.SendText(JsonUtility.ToJson(data));
    }

    public async void SendPlayerRespawn(string id)
    {
        if (!IsConnected) return;
        Json<string> data;
        data.id = id;
        data.message = "respawn";
        data.data = "";
        await ws.SendText(JsonUtility.ToJson(data));
    }

    public async void SendPosition(string id, Vector2 position)
    {
        if (!IsConnected) return;
        Json<Vector2> data;
        data.id = id;
        data.message = "sync_position";
        data.data = position;
        await ws.SendText(JsonUtility.ToJson(data));
        
    }

    private async void OnApplicationQuit()
    {
        if (!IsConnected) return;
        await ws.Close();
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
    public enum URL : int
    {
        [StringValue("ws://localhost:8080")]
        Local,
        [StringValue("wss://shooter-jabbar.herokuapp.com")]
        Server
    }
}