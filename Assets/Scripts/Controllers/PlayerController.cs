using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    // [SerializeField]private GameObject playerPrefab;
    // [SerializeField]private PlayerController _playerController;
    // public void Init(Vector3 position)
    // {
    //     _playerController = Instantiate(playerPrefab, position, transform.rotation).GetComponent<PlayerController>();
    //     _playerController.Init(GetComponent<PlayerInput>(), GameManager.Instance.Network.Id);
    //     _playerController.SetColor(Color.gray);
    // }

    public bool IsMyPlayer;
    public void Damage(string x)
    {
        
    }

    public void OnMove(Vector2 v)
    {
        
    }
    private const float SyncTime = 0.2f;
    private float syncTimer = 0;
    private event Action<string, Vector2> OnMoveSync = delegate {  };
    public string Id
    {
        get;
        private set;
    }
    public int Score { get; private set; }
    public void Init(string id)
    {
        Id = id;
    }

    public void UpdateScore(int score)
    {
        Score += score;
        OldGameManager.Instance.Events.Get<ScoreUpdatedEvent>().Set(this, Score);
        OldGameManager.Instance.Events.FireEvent(typeof(ScoreUpdatedEvent));
    }

    public void BulletHit(Bullet bullet)
    {
        // if (bullet.IsMyBullet)
        // {
            OldGameManager.Instance.Network.SendBulletHit(Id);
        // }
    }
    
    public void Respawn()
    {
        OldGameManager.Instance.Network.SendPlayerRespawn(Id);
    }
    
    public void SetColor(Color color)
    {
        // meshRenderer.material.color = color;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        // SyncedPosition = position;
    }
    
    void SyncPosition()
    {
        var position = new Vector2(transform.position.x, transform.position.z);
        OldGameManager.Instance.Network.SendPosition(Id, position);
    }

    public void Update()
    {
        syncTimer += Time.deltaTime;
        if (syncTimer >= SyncTime)
        {
            syncTimer = 0;
            SyncPosition();
        }
    }
}