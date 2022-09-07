using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]private float playerSpeed = 7.0f;
    private float gravityValue = -9.81f;
    private Vector3 move = Vector3.zero;
    public PlayerInput PlayerInput
    {
        get;
        private set;
    }
    public int Score { get; private set; }
    
    public void Init(PlayerInput playerInput)
    {
        PlayerInput = playerInput;
        transform.SetParent(PlayerInput.transform);
    }
    
    private void Awake()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        controller.Move(move * (Time.deltaTime * playerSpeed));

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
    }

    public void OnMove(Vector3 movement)
    {
        move = new Vector3(movement.x, 0, movement.y);
    }

    public void UpdateScore(int score)
    {
        Score += score;
        GameManager.Instance.Events.Get<ScoreUpdatedEvent>().Set(this, Score);
        GameManager.Instance.Events.FireEvent(typeof(ScoreUpdatedEvent));
    }
}