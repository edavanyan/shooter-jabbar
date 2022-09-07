using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using WebSocketSharp;

public class Network : MonoBehaviour
{
    public WebSocket WebSocket
    {
        get;
        private set;
    }
    
    void Start()
    {
        WebSocket = new WebSocket("ws://shooter-jabbar.herokuapp.com");
        // WebSocket = new WebSocket("ws://localhost:8080");
        WebSocket.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data);
            var data = JsonUtility.FromJson<Json>(e.Data);
            GameManager.Instance.Events.Get<WebMessageReceivedEvent>().Set((WebSocket)sender, data);
            GameManager.Instance.Events.FireEvent(typeof(WebMessageReceivedEvent));
        };
        WebSocket.Connect();
    }

    private DateTime time = DateTime.Now;
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Right))
        {
            time = DateTime.Now;
            _json.message = "join";
            WebSocket.Send(JsonUtility.ToJson(_json));
        }
    }

    private Json _json;
    public void SendMove(Vector2 movement)
    {
        _json.message = "move";
        _json.data = movement;
        var jsonData = JsonUtility.ToJson(_json);
        Debug.Log("sendng: " + jsonData);
        WebSocket.Send(jsonData);
    }

    public struct Json
    {
        public string message;
        public Vector2 data;
    }
}
