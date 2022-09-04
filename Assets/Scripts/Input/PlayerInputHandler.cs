using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public GameObject playerPrefab;
    private PlayerController _playerController;
    private Vector3 _startPos = new Vector3(0, 0, 0);

    private void Awake()
    {
        _playerController = Instantiate(playerPrefab, _startPos, transform.rotation).GetComponent<PlayerController>();
        _playerController.transform.SetParent(transform);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _playerController.OnMove(context);
    }
}
