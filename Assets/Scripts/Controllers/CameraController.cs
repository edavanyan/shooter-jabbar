using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CameraController : MonoBehaviour, EventListener
{
    private PlayerController _playerController;
    private bool _isInGame = false;//to avoid expensive null check in update 

    [SerializeField] private float _smoothSpeed = 002f;
    [SerializeField] private Vector3 _offset;

    void Start()
    {
        GameManager.Instance.Events.RegisterObserver(this);
    }

    void LateUpdate()
    {
        if (_isInGame)
        {
            var destination = _playerController.transform.position + _offset;
            var smoothPosition = Vector3.Lerp(transform.position, destination, _smoothSpeed);
            if (smoothPosition.z < -14)
            {
                smoothPosition.z = -14;
            }
            transform.position = smoothPosition;
        }
    }

    [EventHandler]
    void OnPlayerJoined(PlayerJoinedEvent playerJoinedEvent)
    {
        _playerController = playerJoinedEvent.Player;
        _isInGame = true;
    }

    [EventHandler]
    void OnPlayerLeft(PlayerLeftEvent playerLeftEvent)
    {
        _isInGame = false;
    }
}
