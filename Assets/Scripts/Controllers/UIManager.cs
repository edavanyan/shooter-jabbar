using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour, EventListener
{
    [SerializeField] private PlayerUIPanel _playerUIPanelPrefab;
    private ComponentPool<PlayerUIPanel> _playerPanelPool;

    private Dictionary<PlayerController, PlayerUIPanel>
        playerPanels = new Dictionary<PlayerController, PlayerUIPanel>();
    private void Start()
    {
        GameManager.Instance.Events.RegisterObserver(this);

        _playerPanelPool = new ComponentPool<PlayerUIPanel>(_playerUIPanelPrefab, transform);
    }

    [EventHandler]
    void OnPlayerJoined(PlayerJoinedEvent playerJoinedEvent)
    {
        var playerUIPanel = _playerPanelPool.NewItem();
        playerUIPanel.Set(playerJoinedEvent.Player);
        playerPanels.Add(playerJoinedEvent.Player, playerUIPanel);
    }

    [EventHandler]
    void OnPlayerLeft(PlayerLeftEvent playerLeftEvent)
    {
        _playerPanelPool.DestoryItem(playerPanels[playerLeftEvent.Player]);
        playerPanels.Remove(playerLeftEvent.Player);
    }
    
    [EventHandler]
    void OnScoreUpdate(ScoreUpdatedEvent scoreUpdatedEvent)
    {
        playerPanels[scoreUpdatedEvent.Player].SetScore(scoreUpdatedEvent.Score);
    }
}
