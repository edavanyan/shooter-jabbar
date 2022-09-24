using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HUDManager : MonoBehaviour, IHudManager
{
    public event Action JoinButtonPressed;
    private Canvas canvas;
    [SerializeField]
    private Canvas menu;

    [SerializeField] private GameObject loadingGO;
    
    [SerializeField]
    private InGameUIManager inGameUIManager;

    public IInGameUIManager InGameUIManager => inGameUIManager;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        loadingGO = GameObject.Find("Loading");
    }

    public void OnJoinButtonPressed()
    {
        HideMenu();
        JoinButtonPressed();
    }

    private IEnumerator HideLoading()
    {
        yield return new WaitForUpdate();
        loadingGO.SetActive(false);
    }
    
    public void ShowMenu()
    {
        StartCoroutine(HideLoading());
        menu.gameObject.SetActive(true);
    }

    public Vector2 PointToCanvas(Vector2 point)
    {
        return new Vector2(point.x - Screen.width / 2f, point.y - Screen.height / 2f);
    }

    private void HideMenu()
    {
        menu.gameObject.SetActive(false);
    } 
}
