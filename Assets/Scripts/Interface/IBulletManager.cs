using System;
using UnityEngine;

public interface IBulletManager
{
    event Action<string> OnBulletHit;
    void Fire(string uid, Vector3 position, Vector3 direction);
}
