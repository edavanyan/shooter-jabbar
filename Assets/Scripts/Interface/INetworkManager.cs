using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkManager
{
    string Id { get; }
    event Action<Dictionary<string, object>> OnWebMessage; 
    event Action OnSocketConnect;
    void SendMessageGetMap();
    void SendMessageJoin();
    void SendMessageFire(Vector2 direction);
    void SendMessageMove(Vector2 direction);
    void SendMessageAidClaim(string aidId);
    void SendMessageCharacterDie(string uid);
    void SendMessageCharacterRespawn();
    void SendMessageBulletHit(string bulletId);
    void SendMessageSyncPosition(Vector2 position);
    void SendMessageMapData(string receiverId, Dictionary<string,CharacterData> charactersData);
}
