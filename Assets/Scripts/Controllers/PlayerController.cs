using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private const int HP = 10;
    
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]private float playerSpeed = 7.0f;
    private float gravityValue = -9.81f;
    private Vector3 move = Vector3.zero;

    private int currentHP = HP;

    [SerializeField]private MeshRenderer meshRenderer;

    public bool IsMyPlayer
    {
        get;
        private set;
    }

    public string Id
    {
        get;
        private set;
    }
    
    public PlayerInput PlayerInput
    {
        get;
        private set;
    }
    public int Score { get; private set; }

    private const float SyncTime = 0.2f;
    private float syncTimer = 0;
    private float _smoothSpeed = 0.02f;
    public Vector3 SyncedPosition
    {
        set;
        private get;
    }

    public void Init(string id)
    {
        Id = id;
        IsMyPlayer = false;
    }

    public void Init(PlayerInput playerInput, string id)
    {
        IsMyPlayer = true;
        PlayerInput = playerInput;
        transform.SetParent(PlayerInput.transform);
        Id = id;
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

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
            controller.Move(move * (Time.deltaTime * playerSpeed));
            SyncPosition();
        }
        else if (Vector2.Distance(transform.position, SyncedPosition) > 0.02f)
        {
            var smoothPosition = Vector3.Lerp(transform.position, SyncedPosition, _smoothSpeed);
            transform.position = smoothPosition;
        }
        else
        {
            SyncPosition();
        }
        syncTimer += Time.deltaTime;
    }

    void SyncPosition()
    {
        if (syncTimer >= SyncTime)
        {
            syncTimer = 0;
            var position = new Vector2(transform.position.x, transform.position.z);
            GameManager.Instance.Network.SendPosition(Id, position);
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

    public void BulletHit(Bullet bullet)
    {
        if (bullet.IsMyBullet)
        {
            GameManager.Instance.Network.SendBulletHit(Id);
        }
    }

    public void Damage(string damageId)
    {
        currentHP--;
        if (currentHP <= 0)
        {
            if (damageId == GameManager.Instance.Network.Id)
            {
                Respawn();
            }
        }
    }

    public void Respawn()
    {
        GameManager.Instance.Network.SendPlayerRespawn(Id);
    }

    public void Reset()
    {
        currentHP = HP;
        SyncedPosition = transform.position;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }
}