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

    public void Fire(Vector3 position, Vector3 direction)
    {
        var bullet = _bulletPool.NewItem();
        bullet.transform.position = position;
        bullet.OnBulletTriggerEnter += col =>
        {
            if (col.gameObject.CompareTag("Wall"))
            {
                _bulletPool.DestoryItem(bullet);
                _bullets.Remove(bullet);
            }
        };
        _bullets.Add(bullet, direction);
    }
    
    void FixedUpdate()
    {
        foreach (var bullet in _bullets)
        {
            bullet.Key.Move(bullet.Value);
        }
    }
}
