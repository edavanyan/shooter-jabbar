using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public GameObject playerPrefab;
    private PlayerController _playerController;

    private void Awake()
    {
        var startPoint = GameManager.Instance.SpawnPoints[0].transform.position;
        _playerController = Instantiate(playerPrefab, startPoint, transform.rotation).GetComponent<PlayerController>();
        _playerController.transform.SetParent(transform);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _playerController.OnMove(context);
    }
}
