using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour, EventListener
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerInput _playerInputPrefab;
    [SerializeField] private PlayerController _playerPrefab;
    public GameObject[] SpawnPoints { get; private set; }
    public Dictionary<string, PlayerController> Players { get; private set; }

    private MainThreadRunnableArgs<Vector2> mainThreadArgs;
    private readonly Dictionary<MainThreadRunnableArgs<Vector2>, Action<string, Vector2>> runOnMainThread =
        new Dictionary<MainThreadRunnableArgs<Vector2>, Action<string, Vector2>>();

    public PlayerController Player
    {
        get { return Players[Network.Id]; }
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

        Players = new Dictionary<string, PlayerController>();
        
        Events.RegisterObserver(this);
    }

    private bool IsMyMessage(string id)
    {
        return Network.Id.Equals(id);
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log("Bye");
    }

    private void UnregisterPlayer(string id, Vector2 position)
    {
        var player = Players[id];
        Players.Remove(id);
        Events.Get<PlayerLeftEvent>().Set(player);
        Events.FireEvent(typeof(PlayerLeftEvent));

        Destroy(player.gameObject);
    }

    void JoinPlayer(string id, Vector2 position)
    {
        Vector3 spawnPoint = new Vector3(position.x, 0.5f, position.y);
        PlayerController playerController;
        var isMyMessage = IsMyMessage(id);
        if (isMyMessage)
        {
            var playerInputHandler = Instantiate(_playerInputPrefab).GetComponent<PlayerInputHandler>();
            playerInputHandler.Init(spawnPoint);

            playerController = playerInputHandler.PlayerController;
        }
        else
        {
            playerController = Instantiate(_playerPrefab, spawnPoint, Quaternion.identity).GetComponent<PlayerController>();
        }

        Players.Add(id, playerController);
        
        Events.Get<PlayerJoinedEvent>().Set(playerController, isMyMessage);
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
            action.Value(action.Key.id, action.Key.data);
        }
        runOnMainThread.Clear();
    }

    [EventHandler]
    void WebMessageEvent(WebMessageReceivedEvent messageReceivedEvent)
    {
        if (messageReceivedEvent.Message == "join")
        {
            mainThreadArgs.id = messageReceivedEvent.Id;
            mainThreadArgs.data = (Vector2)messageReceivedEvent.Data;
            runOnMainThread.Add(mainThreadArgs, JoinPlayer);
        }

        if (messageReceivedEvent.Message == "move")
        {
            if (Players.ContainsKey(messageReceivedEvent.Id))
            {
                var data = (Vector2)messageReceivedEvent.Data;
                Players[messageReceivedEvent.Id].OnMove(data);
            }
        }
        
        if (messageReceivedEvent.Message == "map")
        {
            var data = (Dictionary<string, Vector2>)messageReceivedEvent.Data;
            foreach (var d in data)
            {
                if (!Players.ContainsKey(d.Key))
                {
                    mainThreadArgs.id = d.Key;
                    mainThreadArgs.data = d.Value;
                    runOnMainThread.Add(mainThreadArgs, JoinPlayer);
                }
            }
        }

        if (messageReceivedEvent.Message == "disconnect")
        {
            mainThreadArgs.id = messageReceivedEvent.Id;
            mainThreadArgs.data = Vector2.zero;
            runOnMainThread.Add(mainThreadArgs, UnregisterPlayer);
        }
    }

    internal struct MainThreadRunnableArgs<T>
    {
        internal string id;
        internal T data;
    }
}
