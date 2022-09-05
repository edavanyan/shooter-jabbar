using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinSpawner : MonoBehaviour, EventListener
{
    [SerializeField]
    Coin _coinPrefab;
    private ComponentPool<Coin> _coinPool;

    private int _minX = -11, _maxX = 11;
    private int _minY = -11, _maxY = 11;

    private void Awake()
    {
        _coinPool = new ComponentPool<Coin>(_coinPrefab);
    }

    private void Start()
    {
        GameManager.Instance.Events.RegisterObserver(this);
        
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnCoin();
            yield return new WaitForSeconds(2);
        }
    }

    private Vector3 RandomPosition()
    {
        int x = Random.Range(_minX, _maxX);
        int z = Random.Range(_minY, _maxY);
        return new Vector3(x, 0.5f, z);
    }

    private void SpawnCoin()
    {
        Coin coin = _coinPool.NewItem();
        coin.transform.position = RandomPosition();
    }

    [EventHandler]
    void OnCoinPickupEvent(PickupCoinEvent coinEvent)
    {
        _coinPool.DestoryItem(coinEvent.Coin);
    }
}
