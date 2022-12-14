using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class OldGameManager : MonoBehaviour, EventListener
{
    public static OldGameManager Instance { get; private set; }
    [SerializeField] private PlayerInput _playerInputPrefab;
    [SerializeField] private PlayerController _playerPrefab;
    public GameObject[] SpawnPoints { get; private set; }
    public Dictionary<string, PlayerController> Players { get; private set; }

    public PlayerController Player
    {
        get { return Players[Network.Id]; }
    }

    public bool IsInGameRoom => Players.ContainsKey(Network.Id);
    public EventService Events { get; private set; }
    public BulletController BulletController { get; private set; }
    public Network Network { get; private set; }
    public CoinSpawner CoinSpawner { get; private set; }

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
        CoinSpawner = GetComponent<CoinSpawner>();

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

    IEnumerator UnregisterPlayer(string id)
    {
        yield return new WaitForUpdate();
        var player = Players[id];
        Players.Remove(id);
        Events.Get<PlayerLeftEvent>().Set(player);
        Events.FireEvent(typeof(PlayerLeftEvent));

        Destroy(player.gameObject);
    }

    IEnumerator PositionPlayer(string id, Vector2 position, bool respawn = false)
    {
        yield return new WaitForUpdate();
        
        Players[id].SetPosition(new Vector3(position.x, 0.5f, position.y));
        if (respawn)
        {
            // Players[id].Reset();
        }
    }

    IEnumerator FireBullet(string id, Vector2 position)
    {
        yield return new WaitForUpdate();
        
        var playerPosition = Players[id].transform.position;
        playerPosition.y = 1.5f;
        BulletController.Fire(playerPosition, new Vector3(position.x, 0, position.y), OldGameManager.Instance.Network.Id == id);
    }

    IEnumerator JoinPlayer(string id, Vector2 position)
    {
        yield return new WaitForUpdate();
        
        Vector3 spawnPoint = new Vector3(position.x, 0.5f, position.y);
        PlayerController playerController;
        var isMyMessage = IsMyMessage(id);
        if (isMyMessage)
        {
            var playerInputHandler = Instantiate(_playerInputPrefab).GetComponent<PlayerInputHandler>();
            // playerInputHandler.Init(spawnPoint);

            playerController = playerInputHandler.PlayerController;
            playerController.SetPosition(spawnPoint);
        }
        else
        {
            playerController = Instantiate(_playerPrefab, spawnPoint, Quaternion.identity).GetComponent<PlayerController>();
            playerController.SetPosition(spawnPoint);
            playerController.Init(id);
        }

        Players.Add(id, playerController);
        
        Events.Get<PlayerJoinedEvent>().Set(playerController, isMyMessage);
        Events.FireEvent(typeof(PlayerJoinedEvent));
    }

    private IEnumerator SpawnCoin(string id, Vector2 position)
    {
        yield return new WaitForUpdate();

        CoinSpawner.SpawnCoin(id, position);
    }

    private IEnumerator CoinPickup(string id)
    {
        yield return new WaitForUpdate();

        CoinSpawner.CoinPickup(id);
    }

    [EventHandler]
    void WebMessageEvent(WebMessageReceivedEvent messageReceivedEvent)
    {
        if (messageReceivedEvent.Message == "join")
        {
            StartCoroutine(JoinPlayer(messageReceivedEvent.Id, (Vector2)messageReceivedEvent.Data));
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
            var data = (Dictionary<string, Dictionary<string, Vector2>>)messageReceivedEvent.Data;
            var dict = data["players"];
            foreach (var (id, position) in dict)
            {
                if (!Players.ContainsKey(id))
                {
                    StartCoroutine(JoinPlayer(id, position));
                }
                else
                {
                    StartCoroutine(PositionPlayer(id, position));
                }
            }
            dict = data["coins"];
            foreach (var (id, position) in dict)
            {
                if (!CoinSpawner.ContainsCoin(id))
                {
                    StartCoroutine(SpawnCoin(id, position));
                }
            }
        }
        
        if (messageReceivedEvent.Message == "fire")
        {
            StartCoroutine(FireBullet(messageReceivedEvent.Id, (Vector2)messageReceivedEvent.Data));
        }
        
        if (messageReceivedEvent.Message == "bullet_hit")
        {
            Players[messageReceivedEvent.Data.ToString()].Damage(messageReceivedEvent.Id);
        }
        
        if (messageReceivedEvent.Message == "sync_position")
        {
            StartCoroutine(PositionPlayer(messageReceivedEvent.Id, (Vector2)messageReceivedEvent.Data));
        }
        
        if (messageReceivedEvent.Message == "respawn")
        {
            StartCoroutine(PositionPlayer(messageReceivedEvent.Id, (Vector2)messageReceivedEvent.Data, true));
        }
        
        if (messageReceivedEvent.Message == "fire")
        {
            StartCoroutine(FireBullet(messageReceivedEvent.Id, (Vector2)messageReceivedEvent.Data));
        }
        
        if (messageReceivedEvent.Message == "coin_pick")
        {
            StartCoroutine(CoinPickup(messageReceivedEvent.Data.ToString()));
        }
        
        if (messageReceivedEvent.Message == "spawn_coin")
        {
            var coinData = (Dictionary<string, Vector2>)messageReceivedEvent.Data;
            foreach (var (id, position) in coinData)
            {
                StartCoroutine(SpawnCoin(id, position));
            }
        }

        if (messageReceivedEvent.Message == "disconnect")
        {
            StartCoroutine(UnregisterPlayer(messageReceivedEvent.Id));
        }
    }
}
