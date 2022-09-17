using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SphereCollider))]
public class Aid : MonoBehaviour, IPoolable
{
    private float pickUpRadius = 1.5f;
    private float rotationSpeed = 150f;

    private SphereCollider _sphereCollider;
    public string Id { get; set; }
    public event Action<string, string> OnAidCollision;

    void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        _sphereCollider.isTrigger = true;
        _sphereCollider.radius = pickUpRadius;

        rotationSpeed += Random.Range(-25, 25);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        PickupCoin(other);
    }

    private void PickupCoin(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            OnAidCollision?.Invoke(other.GetComponent<Character>().Id, Id);
        }
    }

    public void New()
    {
        gameObject.SetActive(true);
    }

    public void Free()
    {
        OnAidCollision = null;
        Id = "";
        gameObject.SetActive(false);
    }
}
