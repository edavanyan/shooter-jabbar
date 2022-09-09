using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPrefab;
    private ComponentPool<Bullet> _bulletPool;

    private readonly Dictionary<Bullet, Vector3> _bullets = new Dictionary<Bullet, Vector3>();
    void Awake()
    {
        _bulletPool = new ComponentPool<Bullet>(_bulletPrefab);
    }

    public void Fire(Vector3 position, Vector3 direction, bool isMyBullet)
    {
        var bullet = _bulletPool.NewItem();
        bullet.transform.position = position;
        bullet.IsMyBullet = isMyBullet;
        bullet.OnBulletTriggerEnter += col =>
        {
            if (col.gameObject.CompareTag("Player"))
            {
                var playerController = col.GetComponent<PlayerController>();
                if (playerController.IsMyPlayer != bullet.IsMyBullet)
                {
                    playerController.BulletHit(bullet);
                    DestroyBullet(bullet);
                }
            }
            if (col.gameObject.CompareTag("Wall"))
            {
                DestroyBullet(bullet);
            }
        };
        _bullets.Add(bullet, direction);
    }

    private void DestroyBullet(Bullet bullet)
    {
        _bulletPool.DestoryItem(bullet);
        _bullets.Remove(bullet);
    }
    
    void FixedUpdate()
    {
        foreach (var bullet in _bullets)
        {
            bullet.Key.Move(bullet.Value);
        }
    }
}
