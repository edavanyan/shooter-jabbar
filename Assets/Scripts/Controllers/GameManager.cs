using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject[] SpawnPoints { get; private set; }
    public List<PlayerInput> Players { get; private set; }

    public EventService Events { get; private set; }

    [SerializeField] InputAction joinAction;
    [SerializeField] InputAction leaveAction;
        
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
        Players = new List<PlayerInput>();

        joinAction.Enable();
        joinAction.performed += JoinAction;
        leaveAction.Enable();
        leaveAction.performed += LeaveAction;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        Players.Add(playerInput);
        
        Events.Get<PlayerJoinedEvent>().Set(playerInput);
        Events.FireEvent(typeof(PlayerJoinedEvent));
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
            foreach (var playerDevice in player.devices)
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

    private void UnregisterPlayer(PlayerInput player)
    {
        Players.Remove(player);
        
        Events.Get<PlayerLeftEvent>().Set(player);
        Events.FireEvent(typeof(PlayerLeftEvent));
        
        Destroy(player.gameObject);
    }
}
