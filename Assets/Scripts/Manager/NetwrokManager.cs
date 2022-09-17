using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;

public class NetwrokManager : MonoBehaviour, INetworkManager
{
    private WebSocket ws;
    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;
    private string id;
    public string Id => id;
    private MessageParser parser = new MessageParser();
    
    public event Action<Dictionary<string, object>> OnWebMessage; 
    public event Action OnSocketConnect; 

    [SerializeField] private URL url;
    private async void Awake()
    {
        ws = WebSocketFactory.CreateInstance(url.GetStringValue());

        id = Guid.NewGuid().ToString();
        ws.OnMessage += OnMessage;
        ws.OnOpen += () => OnSocketConnect();

        await ws.Connect();
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_LINUX
        if (ws.State == WebSocketState.Open)
        {
            ws.DispatchMessageQueue();
        }
#endif
    }

    private void OnMessage(byte[] messageData)
    {
        var messageString = Encoding.UTF8.GetString(messageData);
        var dataDictionary = parser.ParseMessage(messageString);

        OnWebMessage(dataDictionary);
    }

    private enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected
    }

    public enum URL
    {
        [StringValue("ws://localhost:8080")] Local,

        [StringValue("wss://shooter-jabbar.herokuapp.com")] Server
    }

    public void RegisterWebMessageListener(Action<Dictionary<string, object>> messageHandler)
    {
        OnWebMessage += messageHandler;
    }

    public void SendMessageGetMap()
    {
        Debug.Log("get map");
        MessageData<object> map = new MessageData<object>();
        map.message = "get_map";
        SendJson(map);
    }

    public void SendMessageJoin()
    {
        MessageData<object> join = new MessageData<object>();
        join.message = "join";
        SendJson(join);
    }

    public void SendMessageFire(Vector2 direction)
    {
        MessageData<PositionData> fire = new MessageData<PositionData>();
        fire.message = "fire";
        fire.data = new PositionData();
        fire.data.x = direction.x;
        fire.data.y = direction.y;
        SendJson(fire);
    }

    public void SendMessageMove(Vector2 direction)
    {
        MessageData<PositionData> move = new MessageData<PositionData>();
        move.message = "move";
        move.data = new PositionData();
        move.data.x = direction.x;
        move.data.y = direction.y;
        SendJson(move);
    }

    public void SendMessageAidClaim(string aidId)
    {
        MessageData<string> claim = new MessageData<string>();
        claim.message = "aid_pick";
        claim.data = aidId;
        SendJson(claim);
    }

    public void SendMessageCharacterDie(string uid)
    {
        MessageData<string> die = new MessageData<string>();
        die.message = "die";
        die.data = uid;
        SendJson(die);
    }

    public void SendMessageCharacterRespawn()
    {
        Debug.Log("send respawn");
        MessageData<string> respawn = new MessageData<string>();
        respawn.message = "respawn";
        respawn.data = "";
        SendJson(respawn);
    }

    public void SendMessageBulletHit(string victimId)
    {
        MessageData<string> bullet = new MessageData<string>();
        bullet.message = "bullet_hit";
        bullet.data = victimId;
        SendJson(bullet);
    }

    public void SendMessageSyncPosition(Vector2 position)
    {
        MessageData<PositionData> syncPos = new MessageData<PositionData>();
        syncPos.message = "sync_position";
        syncPos.data = new PositionData();
        syncPos.data.x = position.x;
        syncPos.data.y = position.y;
        SendJson(syncPos);
    }

    Dictionary<string, string> positions = new Dictionary<string, string>();
    public void SendMessageMapData(string receiverId, Dictionary<string, CharacterData> charactersData)
    {
        MessageData<string> map = new MessageData<string>();
        map.message = "map";
        positions.Clear();
        foreach (var (playerId, player) in charactersData)
        {
            positions.Add(playerId, JsonConvert.SerializeObject(player));
        }

        var dataString = JsonConvert.SerializeObject(positions);
        var mapData = new MessageData<string>();
        mapData.id = receiverId;
        mapData.data = dataString;
        
        map.data = JsonConvert.SerializeObject(mapData);
        
        SendJson(map);
    }

    async void SendJson<T>(MessageData<T> data)
    {
        if (ws.State == WebSocketState.Open)
        {
            if (!String.IsNullOrEmpty(data.message))
            {
                data.id = Id;
                var serializeObject = JsonConvert.SerializeObject(data);
                await ws.SendText(serializeObject);
            }
            else
            {
                throw new Exception("message is empty");
            }
        }
    }
    
    private async void OnApplicationQuit()
    {
        if (ws.State == WebSocketState.Open)
        {
            await ws.Close();
        }
    }
}
