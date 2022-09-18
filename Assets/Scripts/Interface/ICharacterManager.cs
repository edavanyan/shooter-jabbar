using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterManager
{
    event Action<string, Vector2> OnPositionSync;
    event Action<string, string> OnCharacterDie;
    event Action<string> OnCharacterRespawn;
    void BulletCollision(string uid, string killerId);
    void AidCollision(string uid);
    void OnMoveInput(string uid, Vector3 motion);
    void SpawnCharacter(string uid, Vector3 spawnPosition);
    void SyncronizePositionFromNetwrok(string uid, Vector3 position);
    void RemoveCharacter(string uid);
    ICharacter GetCharacterById(string uid);
    bool CharacterExists(string uid);
    void ReSpawnCharacter(string uid, Vector3 position);
    void DieCharacter(string uid);
    Dictionary<string, CharacterData> GetCharactersData();
    void SynchronizeWithMap(string uid, Dictionary<string,object> character);
}
