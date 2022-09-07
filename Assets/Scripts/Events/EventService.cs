using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class EventService : MonoBehaviour
{

    private Dictionary<Type, Event> _events = new Dictionary<Type, Event>();
    private List<EventListener> _eventListeners = new List<EventListener>();
    
    private object[] _tempParameters = new object[1];

    public void Awake()
    {
        _events.Add(typeof(PlayerJoinedEvent), new PlayerJoinedEvent());
        _events.Add(typeof(PlayerLeftEvent), new PlayerLeftEvent());
        _events.Add(typeof(PickupCoinEvent), new PickupCoinEvent());
        _events.Add(typeof(ScoreUpdatedEvent), new ScoreUpdatedEvent());
        _events.Add(typeof(WebMessageReceivedEvent), new WebMessageReceivedEvent());
    }

    public T Get<T>() where T : Event
    {
        return (T)_events[typeof(T)];
    }

    public void RegisterObserver(EventListener observer)
    {
        _eventListeners.Add(observer);
    }

    public void FireEvent(Event eventToFire)
    {
        FireEvent(eventToFire.GetType());
    }

    public void FireEvent(Type eventType)
    {
        foreach (var eventListener in _eventListeners)
        {
            var methodInfo = eventListener.GetType().GetDeclaredMethods();
            foreach (var info in methodInfo)
            {
                if (info.GetCustomAttribute<EventHandler>() != null)
                {
                    if (info.GetParameters()[0].ParameterType == eventType)
                    {
                        _tempParameters[0] = _events[eventType];
                        info.Invoke(eventListener, _tempParameters);
                        break;
                    }
                }
            }
        }
    }
}
