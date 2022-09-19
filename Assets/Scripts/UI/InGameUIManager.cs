using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIManager : MonoBehaviour, IInGameUIManager
{
    [SerializeField] private HealthBar healthPrefab;
    private ComponentPool<HealthBar> healthPool;
    private Dictionary<string, HealthBar> healthBars;
    private Dictionary<string, Vector2> healthBarPositions;

    private void Awake()
    {
        healthPool = new ComponentPool<HealthBar>(healthPrefab, transform);

        healthBars = new Dictionary<string, HealthBar>();
        healthBarPositions = new Dictionary<string, Vector2>();
    }

    public void CreateHealth(string uid)
    {
        var healthBar = healthPool.NewItem();
        healthBars.Add(uid, healthBar);
    }

    public void DestroyHealth(string uid)
    {
        healthPool.DestoryItem(healthBars[uid]);
        healthBars.Remove(uid);
    }

    public void ChangeHealth(int healthAmount, string uid)
    {
        healthBars[uid].SetHealth(healthAmount);
    }

    public void SetHealthBarPosition(Vector3 position, string uid)
    {
        var screenPosition = GameManager.Instance.Camera.WorldToScreenPosition(position);
        var canvasPosition = GameManager.Instance.HudManager.PointToCanvas(screenPosition);
        ((RectTransform)healthBars[uid].transform).anchoredPosition = canvasPosition;
    }

    public void HideCharacterHealth(string uid)
    {
        healthBars[uid].gameObject.SetActive(false);
    }

    public void ShowCharacterHealth(string uid)
    {
        healthBars[uid].gameObject.SetActive(true);
    }
}
