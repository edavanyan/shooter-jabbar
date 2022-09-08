using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinedEvent : PlayerEvent
{
    public bool IsMe
    {
        get;
        private set;
    }

    public void Set(PlayerController player, bool isMe)
    {
        base.Set(player);
        IsMe = isMe;
    }
}
