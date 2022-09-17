using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHudManager
{
    event Action JoinButtonPressed;
    IInGameUIManager InGameUIManager { get; }
    void ShowMenu();
    Vector2 PointToCanvas(Vector2 point);
}
