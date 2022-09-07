using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using WebSocketSharp;

public class GameManager : MonoBehaviour, EventListener
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerInput _playerInputPrefab;
    [SerializeField] private PlayerController _playerPrefab;
    public GameObject[] SpawnPoints { get; private set; }
    public Dictionary<WebSocket, PlayerController> Players { get; private set; }

    private readonly Dictionary<WebSocket, Action<WebSocket>> runOnMainThread =
        new Dictionary<WebSocket, Action<WebSocket>>();

    PlayerController Player
    {
        get { return Players[Network.WebSocket]; }
    }
    public EventService Events { get; private set; }
    public BulletController BulletController { get; private set; }
    public Network Network { get; private set; }

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

        Players = new Dictionary<WebSocket, PlayerController>();
        
        Events.RegisterObserver(this);
    }

    void JoinPlayer(WebSocket socket)
    {
        PlayerController playerController;
        if (IsMyMessage(socket))
        {
            var playerInputHandler = Instantiate(_playerInputPrefab).GetComponent<PlayerInputHandler>();
            playerInputHandler.Init();

            playerController = playerInputHandler.PlayerController;
        }
        else
        {
            playerController = Instantiate(_playerInputPrefab).GetComponent<PlayerController>();
        }

        Players.Add(socket, playerController);
        
        Events.Get<PlayerJoinedEvent>().Set(playerController);
        Events.FireEvent(typeof(PlayerJoinedEvent));
    }

    private bool IsMyMessage(WebSocket socket)
    {
        return Network.WebSocket.Equals(socket);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            var touchPosition = Input.mousePosition;
            var ray = _camera.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out var hit, _layerMask))
            {
                var playerPosition = Player.transform.position;
                playerPosition.y = 1.5f;
                var hitPoint = hit.point;
                hitPoint.y = 1.5f;
                var velocity = hitPoint - playerPosition;
                BulletController.Fire(playerPosition, velocity.normalized);
            }
        }

        foreach (var action in runOnMainThread)
        {
            action.Value(action.Key);
        }
        runOnMainThread.Clear();
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
        foreach (var player in Players.Values)
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
        WebSocket keySocket = null;
        foreach (var socket in Players)
        {
            if (player.Equals(socket.Value))
            {
                keySocket = socket.Key;
                break;
            }
        }

        if (keySocket != null)
        {
            Players.Remove(keySocket);

            Events.Get<PlayerLeftEvent>().Set(player);
            Events.FireEvent(typeof(PlayerLeftEvent));

            Destroy(player.PlayerInput.gameObject);
        }
    }

    [EventHandler]
    void WebMessageEvent(WebMessageReceivedEvent messageReceivedEvent)
    {
        if (messageReceivedEvent.Data.message == "join")
        {
            runOnMainThread.Add(messageReceivedEvent.Socket, JoinPlayer);
        }

        if (messageReceivedEvent.Data.message == "move")
        {
            Players[messageReceivedEvent.Socket].OnMove(messageReceivedEvent.Data.data);
        }
    }
}
