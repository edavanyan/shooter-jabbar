using WebSocketSharp;

public class WebMessageReceivedEvent : Event
{
    public string Id { get; private set; }
    public string Message { get; private set; }
    public object Data { get; private set; }

    public void Set(string id, string message, object data)
    {
        Id = id;
        Message = message;
        Data = data;
    }
}
