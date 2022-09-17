using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private Vector2 _movement = Vector2.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
        OldGameManager.Instance.Network.SendMove(_movement);
    }

    public PlayerController PlayerController;
}
