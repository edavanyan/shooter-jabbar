using UnityEngine;

public class Loading : MonoBehaviour
{
    [SerializeField] private GameObject blinkingDot;

    private const float BlinkInterval = 0.5f;
    private float blinkTimer;
    void Update()
    {
        blinkTimer += Time.deltaTime;
        if (blinkTimer >= BlinkInterval)
        {
            blinkTimer = 0;
            blinkingDot.SetActive(!blinkingDot.activeSelf);
        }

    }
}
