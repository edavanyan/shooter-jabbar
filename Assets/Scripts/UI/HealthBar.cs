using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour, IPoolable
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }

    public void New()
    {
        slider.value = slider.maxValue;
        gameObject.SetActive(true);
    }

    public void Free()
    {
        gameObject.SetActive(false);
    }
}
