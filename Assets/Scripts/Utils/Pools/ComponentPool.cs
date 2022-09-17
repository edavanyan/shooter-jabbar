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
    
    public ComponentPool(T prototype, Transform transform, bool forceInstantiate = true) : base(prototype)
    {
        _defaultParent = transform;
        if (forceInstantiate)
        {
            var proto = CreateItem(prototype);
            DestoryItem(proto);
        }
    }
    
    protected override T CreateItem(T prototype)
    {
        return GameObject.Instantiate(prototype, _defaultParent);
    }

}
