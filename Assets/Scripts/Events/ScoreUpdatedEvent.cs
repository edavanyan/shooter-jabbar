using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScoreUpdatedEvent : PlayerEvent
{
    public int Score { get; private set; }

    public void Set(PlayerController player, int score)
    {
        base.Set(player);
        Score = score;
    }
}
