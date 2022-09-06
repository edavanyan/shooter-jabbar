using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject[] SpawnPoints { get; private set; }
    public List<PlayerController> Players { get; private set; }

    public EventService Events { get; private set; }
    public BulletController BulletController { get; private set; }
    public Network Network { get; private set; }

    [SerializeField] InputAction joinAction;
    [SerializeField] InputAction leaveAction;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Camera _camera;
        
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        Events = GetComponent<EventService>();
        BulletController = GetComponent<BulletController>();
        Network = GetComponent<Network>();
        
        Players = new List<PlayerController>();

        joinAction.Enable();
        joinAction.performed += JoinAction;
        leaveAction.Enable();
        leaveAction.performed += LeaveAction;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        var playerInputHandler = playerInput.GetComponent<PlayerInputHandler>();
        playerInputHandler.Init();
        
        var playerController = playerInputHandler.PlayerController;
        Players.Add(playerController);
        
        Events.Get<PlayerJoinedEvent>().Set(playerController);
        Events.FireEvent(typeof(PlayerJoinedEvent));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            var touchPosition = Input.mousePosition;
            var ray = _camera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out var hit, _layerMask))
            {
                var playerPosition = Players[0].transform.position;
                playerPosition.y = 1.5f;
                var hitPoint = hit.point;
                hitPoint.y = 1.5f;
                var velocity = hitPoint - playerPosition;
                BulletController.Fire(playerPosition, velocity.normalized);
            }
        }
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log("Bye");
    }

    void JoinAction(InputAction.CallbackContext context)
    {
        PlayerInputManager.instance.JoinPlayerFromActionIfNotAlreadyJoined(context);
    }
    
    void LeaveAction(InputAction.CallbackContext context)
    {
        foreach (var player in Players)
        {
            InputDevice device = null;
            foreach (var playerDevice in player.PlayerInput.devices)
            {
                if (context.control.device == playerDevice)
                {
                    device = playerDevice;
                    break;
                }
            }

            if (device != null)
            {
                UnregisterPlayer(player);
                break;
            }
        }
    }

    private void UnregisterPlayer(PlayerController player)
    {
        Players.Remove(player);
        
        Events.Get<PlayerLeftEvent>().Set(player);
        Events.FireEvent(typeof(PlayerLeftEvent));
        
        Destroy(player.PlayerInput.gameObject);
    }
}
