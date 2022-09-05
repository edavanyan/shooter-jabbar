using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ComponentPool<T> : Pool<T> where T : MonoBehaviour, IPoolable
{
    private Transform _defaultParent;
    public ComponentPool(T prototype) : base(prototype)
    {
    }
    
    public ComponentPool(T prototype, Transform transform) : base(prototype)
    {
        _defaultParent = transform;
    }
    
    protected override T CreateItem(T prototype)
    {
        return GameObject.Instantiate(prototype, _defaultParent);
    }

}
