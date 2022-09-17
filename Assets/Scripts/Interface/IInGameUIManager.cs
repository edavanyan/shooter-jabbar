
using UnityEngine;

public interface IInGameUIManager
{
    void CreateHealth(string uid);
    void DestroyHealth(string uid);
    void ChangeHealth(int healthAmount, string uid);
    void SetHealthBarPosition(Vector2 position, string uid);
    void HideCharacterHealth(string uid);
    void ShowCharacterHealth(string uid);
}
