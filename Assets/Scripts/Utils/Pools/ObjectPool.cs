using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool<T> : Pool<T> where T : IPoolable, new()
{
    public ObjectPool(T prototype) : base(prototype)
    {
    }

    protected override T CreateItem(T prototype)
    {
        return new T();
    }
}
