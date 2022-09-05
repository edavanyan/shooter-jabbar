using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject[] SpawnPoints { get; private set; }

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

        joinAction.Enable();
        joinAction.performed += context => JoinAction(context);
        leaveAction.Enable();
        leaveAction.performed += context => LeaveAction(context);
        
    }

    private void Start()
    {
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player joined: " + playerInput.user);
    }
    
    void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log("Player left");
    }

    void JoinAction(InputAction.CallbackContext context)
    {
        PlayerInputManager.instance.JoinPlayerFromActionIfNotAlreadyJoined(context);
    }
    
    void LeaveAction(InputAction.CallbackContext context)
    {
        
    }
}
