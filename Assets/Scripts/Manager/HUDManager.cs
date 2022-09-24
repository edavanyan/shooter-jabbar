using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class HUDManager : MonoBehaviour, IHudManager
{
    public event Action JoinButtonPressed;
    private Canvas canvas;
    [SerializeField] private Canvas menu;

    [SerializeField] private GameObject loadingGO;

    [SerializeField] private InGameUIManager inGameUIManager;

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

    public void ShowMenu()
    {
        loadingGO.SetActive(false);
        menu.gameObject.SetActive(true);
    }

    public Vector2 PointToCanvas(Vector2 point)
    {
        var viewportPoint = GameManager.Instance.Camera.camera.ScreenToViewportPoint(point);
        RectTransform rect = (RectTransform)transform;
        var canvasPoint = new Vector2(
            viewportPoint.x * rect.sizeDelta.x - rect.sizeDelta.x / 2,
            viewportPoint.y * rect.sizeDelta.y - rect.sizeDelta.y / 2);
        return canvasPoint;
    }

    private void HideMenu()
    {
        inGameUIManager.gameObject.SetActive(true);
        menu.gameObject.SetActive(false);
    }
}
