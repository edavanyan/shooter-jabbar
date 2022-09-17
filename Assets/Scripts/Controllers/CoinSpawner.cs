using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour, EventListener
{
    [SerializeField]
    Aid _coinPrefab;
    private ComponentPool<Aid> _coinPool;

    private int _minX = -11, _maxX = 11;
    private int _minY = -11, _maxY = 11;

    private int _coinCount;
    [SerializeField]private int _maxCoins;

    private Dictionary<string, Aid> coins = new Dictionary<string, Aid>();

    private void Awake()
    {
        _coinPool = new ComponentPool<Aid>(_coinPrefab);
    }

    private void Start()
    {
        OldGameManager.Instance.Events.RegisterObserver(this);
    }

    public bool ContainsCoin(string id)
    {
        return coins.ContainsKey(id);
    }

    public void SpawnCoin(string id, Vector2 position)
    {
        if (_coinCount < _maxCoins)
        {
            if (!coins.ContainsKey(id))
            {
                _coinCount++;
                var coin = _coinPool.NewItem();
                coin.Id = id;
                coin.transform.position = new Vector3(position.x, 0.5f, position.y);

                coins.Add(coin.Id, coin);
            }
        }
    }

    public void CoinPickup(string id)
    {
        if (coins.ContainsKey(id))
        {
            _coinCount--;
            var coin = coins[id];
            _coinPool.DestoryItem(coin);
            coins.Remove(id);
        }
    }
}
