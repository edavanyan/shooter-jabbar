using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SphereCollider))]
public class Coin : MonoBehaviour, IPoolable
{
    private float _pickUpRadius = 1.5f;
    private float _rotationSpeed = 150f;

    private SphereCollider _sphereCollider;
    
    void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.isTrigger = true;
        _sphereCollider.radius = _pickUpRadius;

        _rotationSpeed += Random.Range(-25, 25);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _pickUpRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        PickupCoin(other);
    }

    private void PickupCoin(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().UpdateScore(1);
            
            GameManager.Instance.Events.Get<PickupCoinEvent>().Set(this);
            GameManager.Instance.Events.FireEvent(typeof(PickupCoinEvent));
        }
    }

    public void New()
    {
        gameObject.SetActive(true);
    }

    public void Free()
    {
        gameObject.SetActive(false);
    }
}
