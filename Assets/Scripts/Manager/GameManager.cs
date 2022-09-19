using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterManager))]
[RequireComponent(typeof(AidManager))]
[RequireComponent(typeof(BulletManager))]
[RequireComponent(typeof(NetwrokManager))]
public class GameManager : MonoBehaviour, IGameManager
{
    public static IGameManager Instance
    {
        get;
        private set;
    }
    
    private ICharacterManager characterManager;
    private IAidManager aidManager;
    private IBulletManager bulletManager;
    private INetworkManager networkManager;

    [SerializeField] private InputManager inputManager;
    [SerializeField] private CameraController cameraController;
    public CameraController Camera => cameraController;

    [SerializeField]
    private HUDManager hudManager;
    public IHudManager HudManager => hudManager;
    public string UserId => networkManager.Id;
    private string botId = "nan";
    public string BotId => botId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);

            characterManager = GetComponent<CharacterManager>();
            aidManager = GetComponent<AidManager>();
            bulletManager = GetComponent<BulletManager>();
            networkManager = GetComponent<NetwrokManager>();
            hudManager = GetComponent<HUDManager>();

            characterManager.OnCharacterDie += HandleCharacterDie;
            characterManager.OnPositionSync += HandleCharacterPositionSync;
            characterManager.OnCharacterRespawn += HandleCharacterRespawn;
            hudManager.JoinButtonPressed += HandleJoinButtonPressed;
            networkManager.OnWebMessage += HandleWebMessage;
            networkManager.OnSocketConnect += HandleSocketConnect;
            inputManager.OnMouseClick += HandleFireInput;
            aidManager.OnAidCollision += HandleAidPickup;
            bulletManager.OnBulletHit += HandleBulletHit;
        }
        else
        {
            Destroy(this);
        }
    }

    private void HandleCharacterRespawn(string uid)
    {
        networkManager.SendMessageCharacterRespawn(uid);
    }

    private void HandleCharacterPositionSync(string uid, Vector2 position)
    {
        if (uid == networkManager.Id)
        {
            networkManager.SendMessageSyncPosition(position);
        }
    }

    private void HandleBulletHit(string victimId)
    {
        networkManager.SendMessageBulletHit(victimId);
    }

    private void HandleSocketConnect()
    {
        Debug.Log("connected: " + UserId);
        hudManager.ShowMenu();
    }

    private void HandleJoinButtonPressed()
    {
        networkManager.SendMessageJoin();
    }

    private void HandleAidPickup(string uid, string aidId)
    {
        if (uid == UserId || uid == BotId)
        {
            networkManager.SendMessageAidClaim(aidId, uid);
        }
    }

    private void HandleCharacterDie(string victimId, string killerId)
    {
        if (killerId == UserId || killerId == botId)
        {
            networkManager.SendMessageCharacterDie(victimId);
        }
    }

    //called by unity events
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>().normalized;
        networkManager.SendMessageMove(new Vector2(direction.x, direction.y));
    }

    private void HandleFireInput(Vector3 position)
    {
        if (characterManager.CharacterExists(networkManager.Id))
        {
            var playerPosition = characterManager.GetCharacterById(networkManager.Id).Position;
            var direction = (position - playerPosition).normalized;
            
            networkManager.SendMessageFire(new Vector2(direction.x, direction.z));
        }
    }

    private void HandleWebMessage(Dictionary<string, object> messageData)
    {
        var message = messageData["message"].ToString();

        if (message == "join")
        {
            StartCoroutine(SpawnCharacterOnMainThread(messageData));
        }
        else if (message == "join_bot")
        {
            botId = messageData["id"].ToString();
            StartCoroutine(SpawnCharacterOnMainThread(messageData));
        }
        else if (message == "move")
        {
            StartCoroutine(MoveCharacterOnMainThread(messageData));
        }
        else if (message == "fire")
        {
            StartCoroutine(FireBulletOnMainThread(messageData));
        }
        else if (message == "spawn_aid")
        {
            StartCoroutine(SpawnAidOnMainThread(messageData));
        }
        else if (message == "aid_pick")
        {
            StartCoroutine(ClaimAidOnMainThread(messageData));
        }
        else if (message == "die")
        {
            StartCoroutine(DieCharacterOnMainThread(messageData));
        }
        else if (message == "respawn")
        {
            StartCoroutine(ReSpawnCharacterOnMainThread(messageData));
        }
        else if (message == "bullet_hit")
        {
            StartCoroutine(DamagePlayerOnMainThread(messageData));
        }
        else if (message == "sync_position")
        {
            StartCoroutine(SyncPositionOnMainThread(messageData));
        }
        else if (message == "get_map")
        {
            SendMapData(messageData);
        }
        else if (message == "map")
        {
            StartCoroutine(SyncMapOnMainThread(messageData));
        }
        else if (message == "disconnect")
        {
            RemoveCharacter(messageData);
        }
    }

    private void RemoveCharacter(Dictionary<string,object> messageData)
    {
        var senderId = messageData["id"].ToString();
        characterManager.RemoveCharacter(senderId);
    }

    private void SendMapData(Dictionary<string,object> messageData)
    {
        var charactersData = characterManager.GetCharactersData();
        networkManager.SendMessageMapData(messageData["id"].ToString(), charactersData);
    }

    private void SpawnAid(string id, Vector3 position)
    {
        aidManager.SpawnAid(id, position);
    }

    private void SpawnCharacter(string id, Dictionary<string, object> data)
    {
        var position = new Vector3(Convert.ToSingle(data["x"]), 0, Convert.ToSingle(data["y"]));
        characterManager.SpawnCharacter(id, position);
    }

    private IEnumerator SpawnAidOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var data = messageData["data"] as Dictionary<string, object>;
        foreach (var (id, aidData) in data)
        {
            var xy = aidData as Dictionary<string, object>;
            var position = new Vector3(Convert.ToSingle(xy["x"]), 0, Convert.ToSingle(xy["y"]));
            SpawnAid(id, position);
        }
    }

    private IEnumerator SpawnCharacterOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        var data = messageData["data"] as Dictionary<string, object>;
        SpawnCharacter(senderId, data);
    }

    private IEnumerator SyncMapOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var data = messageData["data"] as Dictionary<string, object>;
        var characters = data["characters"] as Dictionary<string, object>;
        foreach (var (uid, character) in characters)
        {
            characterManager.SynchronizeWithMap(uid, character as Dictionary<string, object>);
        }
        
        var aids = data["aids"] as Dictionary<string, object>;
        foreach (var (aidid, aid) in aids)
        {
            var xy = aid as Dictionary<string, object>;
            var position = new Vector3(Convert.ToSingle(xy["x"]), 0, Convert.ToSingle(xy["y"]));
            SpawnAid(aidid, position);
        }
    }

    private IEnumerator SyncPositionOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        var data = messageData["data"] as Dictionary<string, object>;
        
        var position = new Vector3(Convert.ToSingle(data["x"]), 0, Convert.ToSingle(data["y"]));
        
        characterManager.SyncronizePositionFromNetwrok(senderId, position);
        
    }

    private IEnumerator DamagePlayerOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        var victimId = messageData["data"].ToString();
        characterManager.BulletCollision(victimId, senderId);
    }

    private IEnumerator DieCharacterOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var victimId = messageData["data"].ToString();
        characterManager.DieCharacter(victimId);
    }

    private IEnumerator ReSpawnCharacterOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var data = messageData["data"] as Dictionary<string, object>;
        foreach (var (id, positionData) in data)
        {
            var xy = positionData as Dictionary<string, object>;
            var position = new Vector3(Convert.ToSingle(xy["x"]), 0, Convert.ToSingle(xy["y"]));
            characterManager.ReSpawnCharacter(id, position);
        }
    }

    private IEnumerator ClaimAidOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        var aidId = messageData["data"].ToString();
        aidManager.RemoveAid(aidId);
        characterManager.AidCollision(senderId);
    }

    private IEnumerator MoveCharacterOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        var data = messageData["data"] as Dictionary<string, object>;
        
        var motion = new Vector3(Convert.ToSingle(data["x"]), 0, Convert.ToSingle(data["y"]));
        
        characterManager.OnMoveInput(senderId, motion);
    }

    private IEnumerator FireBulletOnMainThread(Dictionary<string, object> messageData)
    {
        yield return new WaitForUpdate();
        var senderId = messageData["id"].ToString();
        if (characterManager.CharacterExists(senderId))
        {
            var characterById = characterManager.GetCharacterById(senderId);
            if (!characterById.IsMarkedDead)
            {
                var data = messageData["data"] as Dictionary<string, object>;

                var playerPosition = characterById.Position;
                bulletManager.Fire(senderId, playerPosition,
                    new Vector3(Convert.ToSingle(data["x"]), 0, Convert.ToSingle(data["y"])));
            }
        }
    }

    private void LateUpdate()
    {
        if (characterManager.CharacterExists(UserId))
        {
            cameraController.Move(characterManager.GetCharacterById(UserId).Position);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            networkManager.SendMessageGetMap();
        }
    }
}
