using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    IHudManager HudManager { get; }
    string UserId { get; }
    string BotId { get; }
    CameraController Camera { get; }

    void OnMoveCommand(Vector2 direction);
}
