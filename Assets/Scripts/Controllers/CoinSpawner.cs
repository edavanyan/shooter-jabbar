using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour, EventListener
{
    [SerializeField]
    Coin _coinPrefab;
    private ComponentPool<Coin> _coinPool;

    private int _minX = -11, _maxX = 11;
    private int _minY = -11, _maxY = 11;

    private int _coinCount;
    [SerializeField]private int _maxCoins;

    private Dictionary<string, Coin> coins = new Dictionary<string, Coin>();

    private void Awake()
    {
        _coinPool = new ComponentPool<Coin>(_coinPrefab);
    }

    private void Start()
    {
        GameManager.Instance.Events.RegisterObserver(this);
    }

    public void SpawnCoin(string id, Vector2 position)
    {
        if (_coinCount < _maxCoins)
        {
            _coinCount++;
            var coin = _coinPool.NewItem();
            coin.Id = id;
            coin.transform.position = new Vector3(position.x, 0.5f, position.y);
            
            coins.Add(coin.Id, coin);
        }
    }

    public void CoinPickup(string id)
    {
        _coinCount--;
        var coin = coins[id];
        _coinPool.DestoryItem(coin);
        coins.Remove(id);
    }
}
