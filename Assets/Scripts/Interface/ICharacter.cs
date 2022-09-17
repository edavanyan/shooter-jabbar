using System;
using UnityEngine;

public interface ICharacter
{
    event Action<string, string> OnDie;
    event Action<Vector2> OnPositionSync;
    bool IsMarkedDead { get; }
    Vector3 SyncedPosition { set; }
    void Init(string uid);
    string Id { get; }
    Vector3 Position { get; }
    int Health { get; }
    CharacterData CharacterData { get; }
    void SetInputMotion(Vector3 motion);
    void Move();
    void DamageBy(int amount, string uid);
    void Heal(int amount);
    void Respawn(Vector3 position);
    void SetPosition(Vector3 position);
    void SetHealth(int health);
    void Hide();
    void Destroy();
}
