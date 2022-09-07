using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]private GameObject playerPrefab;
    private PlayerController _playerController;
    private Vector2 _movement = Vector2.zero;
    public PlayerController PlayerController
    {
        get { return _playerController; }
    }

    public void Init()
    {
        var startPoint = GameManager.Instance.SpawnPoints[0].transform.position;
        _playerController = Instantiate(playerPrefab, startPoint, transform.rotation).GetComponent<PlayerController>();
        _playerController.Init(GetComponent<PlayerInput>());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
        GameManager.Instance.Network.SendMove(_movement);
    }
}