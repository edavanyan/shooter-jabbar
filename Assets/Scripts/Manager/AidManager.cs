using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AidManager : MonoBehaviour, IAidManager
{
    [SerializeField]
    private Aid aidPrefab;
    private ComponentPool<Aid> aidPool;
    public event Action<string, string> OnAidCollision;

    private Dictionary<string, Aid> aids = new Dictionary<string, Aid>();
    
    private void Awake()
    {
        aidPool = new ComponentPool<Aid>(aidPrefab);
    }
    
    public void SpawnAid(string id, Vector3 position)
    {
        if (!aids.ContainsKey(id))
        {
            var aid = aidPool.NewItem();
            aid.Id = id;
            aid.OnAidCollision += OnAidCollision;
            aid.transform.position = position;

            aids.Add(aid.Id, aid);
        }
    }
    
    public void RemoveAid(string aidId)
    {
        if (aids.ContainsKey(aidId))
        {
            var aid = aids[aidId];
            aidPool.DestoryItem(aid);
            aids.Remove(aidId);
        }
    }
}
