using WebSocketSharp;

public class WebMessageReceivedEvent : Event
{
    public WebSocket Socket { get; private set; }
    public Network.Json Data { get; private set; }

    public void Set(WebSocket socket, Network.Json data)
    {
        Socket = socket;
        Data = data;
    }
}
