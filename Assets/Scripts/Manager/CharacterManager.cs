using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterManager : MonoBehaviour, ICharacterManager
{
    [SerializeField]
    private Character characterPrefab;
    private readonly Dictionary<string, ICharacter> charactersById = new Dictionary<string, ICharacter>();

    public event Action<string, string> OnCharacterDie;
    public event Action<string> OnCharacterRespawn;
    public event Action<string, Vector2> OnPositionSync;

    public void SpawnCharacter(string uid, Vector3 spawnPosition)
    {
        var character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity, transform);
        character.SyncedPosition = spawnPosition;
        charactersById.Add(uid, character);
        character.Init(uid);

        character.OnDie += OnCharacterDieEvent;
        character.OnPositionSync += position => OnPositionSync(character.Id, position);
        
        GameManager.Instance.HudManager.InGameUIManager.CreateHealth(uid);
    }

    public void SyncronizePositionFromNetwrok(string uid, Vector3 position)
    {
        charactersById[uid].SyncedPosition = position;
    }

    private void OnCharacterDieEvent(string victimId, string killerId)
    {
        OnCharacterDie(victimId, killerId);
    }
    
    public void ReSpawnCharacter(string uid, Vector3 position)
    {
        charactersById[uid].Respawn(position);
    }

    public void DieCharacter(string uid)
    {
        var character = charactersById[uid];
        if (!character.IsMarkedDead)
        {
            character.Hide();
            if (uid == GameManager.Instance.UserId || uid == GameManager.Instance.BotId)
            {
                StartCoroutine(SendRespawnMessage(uid));
            }
        }
    }

    private IEnumerator SendRespawnMessage(string uid)
    {
        yield return new WaitForSeconds(1);

        OnCharacterRespawn(uid);
    }

    Dictionary<string, CharacterData> tmpCharacterPositions = new Dictionary<string, CharacterData>();
    public Dictionary<string, CharacterData> GetCharactersData()
    {
        tmpCharacterPositions.Clear();
        foreach (var (uid, character) in charactersById)
        {
            tmpCharacterPositions.Add(uid, character.CharacterData);
        }

        return tmpCharacterPositions;
    }

    public void SynchronizeWithMap(string uid, Dictionary<string, object> character)
    {
        var xy = character["position"] as Dictionary<string, object>;
        var position = new Vector3(Convert.ToSingle(xy["x"]), 0, Convert.ToSingle(xy["y"]));
        var health = Convert.ToInt32(character["health"]);
        if (CharacterExists(uid))
        {
            charactersById[uid].SetPosition(position);
        }
        else
        {
            SpawnCharacter(uid, position);
        }

        charactersById[uid].SetHealth(health);
    }

    public void BulletCollision(string uid, string killerId)
    {
        charactersById[uid].DamageBy(1, killerId);
    }

    public void AidCollision(string uid)
    {
        charactersById[uid].Heal(1);
    }

    public void OnMoveInput(string uid, Vector3 motion)
    {
        charactersById[uid].SetInputMotion(motion);
    }

    public void RemoveCharacter(string uid)
    {
        charactersById[uid].Hide();
        StartCoroutine(SafeDestroyAfterDelay(uid));
    }

    private IEnumerator SafeDestroyAfterDelay(string uid)
    {
        yield return new WaitForSeconds(1);

        charactersById[uid].Destroy();
        charactersById.Remove(uid);
        GameManager.Instance.HudManager.InGameUIManager.DestroyHealth(uid);
    }

    public ICharacter GetCharacterById(string uid)
    {
        return charactersById[uid];
    }

    public bool CharacterExists(string uid)
    {
        return charactersById.ContainsKey(uid);
    }

    private void Update()
    {
        foreach (var (uid, character) in charactersById)
        {
            character.Move();
            var characterPosition = new Vector3(character.Position.x, character.Position.y, character.Position.z + 1.5f);
            var position = GameManager.Instance.Camera.WorldToScreenPosition(characterPosition);
            var canvasPosition = GameManager.Instance.HudManager.PointToCanvas(position);
            GameManager.Instance.HudManager.InGameUIManager.SetHealthBarPosition(canvasPosition, uid);
        }
    }
}
