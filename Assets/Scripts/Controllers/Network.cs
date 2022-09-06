using System;
using UnityEngine;
using WebSocketSharp;

public class Network : MonoBehaviour
{
    private WebSocket _webSocket;
    // Start is called before the first frame update
    void Start()
    {
        _webSocket = new WebSocket("ws://shooter-jabbar.herokuapp.com");
        // _webSocket = new WebSocket("ws://localhost:8080");
        _webSocket.OnMessage += (sender, e) =>
        {
            Debug.Log("received: " + e.Data + " :" + DateTime.Now.Subtract(time).Milliseconds);
        };
        _webSocket.Connect();
    }

    private DateTime time = DateTime.Now;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            time = DateTime.Now;
            _webSocket.Send("{\"message\" : \"Hello\", \"sender\" : \"edddddaa\"}");
        }
    }
}
