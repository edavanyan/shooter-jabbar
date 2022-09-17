using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAidManager
{
    void SpawnAid(string id, Vector3 position);
    event Action<string, string> OnAidCollision;
    void RemoveAid(string aidId);
}
