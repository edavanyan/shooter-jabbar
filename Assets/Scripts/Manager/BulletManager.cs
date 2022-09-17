using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour, IBulletManager
{
    [SerializeField] private Bullet bulletPrefab;
    private ComponentPool<Bullet> bulletPool;
    private readonly Dictionary<Bullet, Vector3> bullets = new Dictionary<Bullet, Vector3>();
    
    void Awake()
    {
        bulletPool = new ComponentPool<Bullet>(bulletPrefab);
    }

    public event Action<string> OnBulletHit;

    public void Fire(string uid, Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool.NewItem();
        bullet.transform.position = position;
        bullet.Id = uid;
        
        bullet.OnBulletTriggerEnter += col =>
        {
            if (col.gameObject.CompareTag("Player"))
            {
                if (bullet.Id == GameManager.Instance.UserId || bullet.Id == GameManager.Instance.BotId)
                {
                    var character = col.GetComponent<Character>();
                    if (character.Id != bullet.Id)
                    {
                        DestroyBullet(bullet);
                        if (character.Health > 0)
                        {
                            OnBulletHit(character.Id);
                        }
                    }
                }
            }
            if (col.gameObject.CompareTag("Wall"))
            {
                DestroyBullet(bullet);
            }
        };
        
        bullets.Add(bullet, direction);
    }
    
    private void DestroyBullet(Bullet bullet)
    {
        bulletPool.DestoryItem(bullet);
        bullets.Remove(bullet);
    }
    
    void FixedUpdate()
    {
        foreach (var bullet in bullets)
        {
            bullet.Key.Move(bullet.Value);
        }
    }
}
