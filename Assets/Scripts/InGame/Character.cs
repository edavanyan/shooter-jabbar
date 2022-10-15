using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterEngine))]
[RequireComponent(typeof(CharacterHealth))]
public class Character : MonoBehaviour, ICharacter
{
    private CharacterEngine characterEngine;
    private CharacterHealth characterHealth;
    
    private Vector3 inputMotion = Vector3.zero;
    private Vector3 cachedMotion = Vector3.zero;
    private const float Speed = 9f;

    private string id;
    [SerializeField] private GameObject healEffect; 
    [SerializeField] private GameObject damageEffect; 
    [SerializeField] private Material playerMaterial; 
        
    [SerializeField]
    private float smoothSpeed = 0.001f;
    public string Id => id;
    public Vector3 SyncedPosition { private get; set; }

    public event Action<string, string> OnDie;
    public event Action<Vector2> OnPositionSync;

    private const float SyncInterval = 0.2f;
    private float syncronizeTimer = SyncInterval;
    public int Health => characterHealth.Health;

    private bool isAlive = true;
    public bool IsMarkedDead => !isAlive;

    public CharacterData CharacterData {
        get
        {
            var characterData = new CharacterData();
            characterData.health = characterHealth.Health;
            characterData.position = new PositionData();
            characterData.position.x = transform.position.x;
            characterData.position.y = transform.position.z;
            return characterData;
        }
    }

    private void Awake()
    {
        characterEngine = GetComponent<CharacterEngine>();
        characterHealth = GetComponent<CharacterHealth>();
    }

    public void Respawn(Vector3 position)
    {
        isAlive = true;
        gameObject.SetActive(isAlive);
        SetPosition(position);
        
        characterHealth.Reset();
        GameManager.Instance.HudManager.InGameUIManager.ShowCharacterHealth(Id);
        GameManager.Instance.HudManager.InGameUIManager.ChangeHealth(10, Id);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        SyncedPosition = position;
        var characterPosition = new Vector3(Position.x, Position.y, Position.z + 1.5f);
        GameManager.Instance.HudManager.InGameUIManager.SetHealthBarPosition(characterPosition, Id);
        
        OnSynchronizePosition();
    }

    public void SetHealth(int health)
    {
        characterHealth.Health = health;
        GameManager.Instance.HudManager.InGameUIManager.ChangeHealth(characterHealth.Health, Id);
    }

    public void Hide()
    {
        isAlive = false;
        gameObject.SetActive(isAlive);
        healEffect.SetActive(false);
        damageEffect.SetActive(false);
        GameManager.Instance.HudManager.InGameUIManager.HideCharacterHealth(id);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void Init(string uid)
    {
        id = uid;
    }

    public void Move()
    {
        if (isAlive)
        {
            syncronizeTimer += Time.deltaTime;
            if (inputMotion != Vector3.zero)
            {
                characterEngine.Move(inputMotion * (Time.deltaTime * Speed));
                // SyncedPosition = transform.position;
                if (syncronizeTimer >= SyncInterval)
                {
                    OnSynchronizePosition();
                }
            }
            else if (cachedMotion != Vector3.zero)
            {
                OnSynchronizePosition();
            }
            else
            {
                SynchronizeWithNetwork();
            }

            cachedMotion = inputMotion;
        }
    }

    private void SynchronizeWithNetwork()
    {
        var smoothPosition = Vector3.Lerp(transform.position, SyncedPosition, smoothSpeed);
        transform.position = smoothPosition;
    }

    private void OnSynchronizePosition()
    {
        syncronizeTimer = 0;
        OnPositionSync(new Vector2(transform.position.x, transform.position.z));
    }

    public Vector3 Position => transform.position;

    public void SetInputMotion(Vector3 motion)
    {
        inputMotion = motion;
        if (inputMotion != Vector3.zero)
        {
            playerMaterial.SetInt("moving", 1);
        }
        else
        {
            playerMaterial.SetInt("moving", 0);
        }
    }

    public void Heal(int amount)
    {
        healEffect.SetActive(true);
        StartCoroutine(HideHealEffect(healEffect));
        characterHealth.Damage(-amount);
        GameManager.Instance.HudManager.InGameUIManager.ChangeHealth(characterHealth.Health, Id);
    }

    private IEnumerator HideHealEffect(GameObject effect)
    {
        yield return new WaitForSeconds(1);
        effect.SetActive(false);
    }

    public void DamageBy(int amount, string uid)
    {
        characterHealth.Damage(amount);
        GameManager.Instance.HudManager.InGameUIManager.ChangeHealth(characterHealth.Health, Id);
        damageEffect.SetActive(true);
        StartCoroutine(HideHealEffect(damageEffect));
        if (characterHealth.Health <= 0)
        {
            OnDie(id, uid);
        }
    }
}
