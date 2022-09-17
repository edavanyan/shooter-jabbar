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
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    private enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected
    }
    public string Id { get; private set; }

    [SerializeField] private NetwrokManager.URL url;


    async void Awake()
    {
        ws = WebSocketFactory.CreateInstance(url.GetStringValue());

        Id = Guid.NewGuid().ToString();
        ws.OnMessage += (message) =>
        {
            var json = Encoding.UTF8.GetString(message);
            var tmp = JsonUtility.FromJson<Jsonold<object>>(json);
            if (tmp.message == "get_map")
            {
                SendMapData(tmp.id);
            }
            else
            {
                IJson data;
                var webMessageReceivedEvent = OldGameManager.Instance.Events.Get<WebMessageReceivedEvent>();
                if (tmp.message == "join" && tmp.id == Id)
                {
                    _connectionStatus = ConnectionStatus.Connected;
                }

                if (tmp.message == "move" ||
                    tmp.message == "join" ||
                    tmp.message == "fire" ||
                    tmp.message == "sync_position" ||
                    tmp.message == "respawn")
                {
                    data = JsonUtility.FromJson<Jsonold<Vector2>>(json);
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
                }
                else if (tmp.message == "map")
                {
                    var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var dictionary =
                        JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Vector2>>>(dataDict["data"]
                            .ToString());
                    webMessageReceivedEvent.Set(tmp.id, tmp.message, dictionary);
                }
                else if (tmp.message == "spawn_coin")
                {
                    var coinData = JsonConvert.DeserializeObject<Jsonold<Dictionary<string, Vector2>>>(json);
                    webMessageReceivedEvent.Set(tmp.id, tmp.message, coinData.data);
                }
                else if (tmp.message == "bullet_hit" ||
                         tmp.message == "coin_pick")
                {
                    data = JsonUtility.FromJson<Jsonold<string>>(json);
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
                }
                else
                {
                    data = tmp;
                    webMessageReceivedEvent.Set(data.GetId(), data.GetMessage(), data.GetData());
                }

                OldGameManager.Instance.Events.FireEvent(typeof(WebMessageReceivedEvent));
            }
        };

        ws.OnOpen += () => { Debug.Log("Connecting with: " + Id); };

        await ws.Connect();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus &&
            _connectionStatus == ConnectionStatus.Connected)
        {
            GetMapData();
        }
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_LINUX
        if (ws.State == WebSocketState.Open)
        {
            ws.DispatchMessageQueue();
        }
#endif
        if (Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            if (!OldGameManager.Instance.IsInGameRoom)
            {
                if (_connectionStatus == ConnectionStatus.Disconnected)
                {
                    Join();
                }
            }
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JoinBot();
        }
#endif
    }

    async void SendText(string data)
    {
        if (ws.State == WebSocketState.Open)
        {
            await ws.SendText(data);
        }
    }

    void GetMapData()
    {
        Jsonold<string> data;
        data.id = Id;
        data.message = "get_map";
        data.data = "";
        SendText(JsonUtility.ToJson(data));
    }

    void SendMapData(string id)
    {
        Jsonold<string> map;
        map.id = id;
        map.message = "map";

        var positions = new Dictionary<string, string>();
        foreach (var (playerId, player) in OldGameManager.Instance.Players)
        {
            var transform = player.transform;
            var playerPosition = transform.position;
            var position = new Vector2(playerPosition.x, playerPosition.z);

            positions.Add(playerId, JsonUtility.ToJson(position));
        }

        var dataString = JsonConvert.SerializeObject(positions);
        map.data = dataString;
        var mapString = JsonConvert.SerializeObject(map);
        SendText(mapString);
    }

    private void JoinBot()
    {
        Jsonold<object> jsonold;
        jsonold.id = Guid.NewGuid().ToString();
        jsonold.message = "join_bot";
        jsonold.data = "";
        var data = JsonUtility.ToJson(jsonold);
        SendText(data);
    }

    private void Join()
    {
        _connectionStatus = ConnectionStatus.Connecting;
        Jsonold<object> jsonold;
        jsonold.id = Id;
        jsonold.message = "join";
        jsonold.data = "";
        var data = JsonUtility.ToJson(jsonold);
        SendText(data);
    }

    public void SendMove(Vector2 movement)
    {
        Jsonold<Vector2> jsonold;
        jsonold.id = Id;
        jsonold.message = "move";
        jsonold.data = movement;
        var jsonData = JsonUtility.ToJson(jsonold);
        SendText(jsonData);
    }

    public void SendFire(Vector2 direction)
    {
        Jsonold<Vector2> data;
        data.id = Id;
        data.message = "fire";
        data.data = direction;
        SendText(JsonUtility.ToJson(data));
    }

    public void SendCoinPickUp(string coinId)
    {
        Jsonold<string> data;
        data.id = Id;
        data.message = "coin_pick";
        data.data = coinId;
        SendText(JsonUtility.ToJson(data));
    }

    public void SendBulletHit(string victimId)
    {
        Jsonold<string> data;
        data.id = Id;
        data.message = "bullet_hit";
        data.data = victimId;
        SendText(JsonUtility.ToJson(data));
    }

    public void SendPlayerRespawn(string id)
    {
        Jsonold<string> data;
        data.id = id;
        data.message = "respawn";
        data.data = "";
        SendText(JsonUtility.ToJson(data));
    }

    public void SendPosition(string id, Vector2 position)
    {
        Jsonold<Vector2> data;
        data.id = id;
        data.message = "sync_position";
        data.data = position;
        SendText(JsonUtility.ToJson(data));

    }

    private async void OnApplicationQuit()
    {
        if (ws.State == WebSocketState.Open)
        {
            await ws.Close();
        }
    }

    public interface IJson
    {
        string GetId();
        string GetMessage();
        object GetData();
    }

    public struct Jsonold<T> : IJson
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