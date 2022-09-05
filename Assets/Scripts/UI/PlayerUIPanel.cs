using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIPanel : MonoBehaviour, IPoolable
{
    [SerializeField] private TextMeshProUGUI _playerScore;
    [SerializeField] private TextMeshProUGUI _playerName;
    public string PlayerName
    {
        get { return _playerName.text; }
    }

    public void Set(PlayerController player)
    {
        _playerName.text = player.name;
        _playerScore.text = player.Score.ToString();
    }

    public void SetScore(int score)
    {
        _playerScore.text = score.ToString();
    }

    public void New()
    {
    }

    public void Free()
    {
    }
}
