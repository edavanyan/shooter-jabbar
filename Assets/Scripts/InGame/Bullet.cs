using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour, IPoolable
{
    private Rigidbody _rigidbody;
    private readonly float _bulletSpeed = 20f;

    public event Action<Collider> OnBulletTriggerEnter; 
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 velocity)
    {
        _rigidbody.MovePosition(_rigidbody.position + velocity * (_bulletSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        OnBulletTriggerEnter?.Invoke(other);
    }

    public void New()
    {
        gameObject.SetActive(true);
    }

    public void Free()
    {
        OnBulletTriggerEnter = null;
        gameObject.SetActive(false);
    }
}
