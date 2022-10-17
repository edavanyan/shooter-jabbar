using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IPoolable
{
    [SerializeField] private Shader healthBarShader;
    [SerializeField] private Image healthBarImage;
    private Material healthbar;

    private void Awake()
    {
        healthbar = new Material(healthBarShader);
        healthBarImage.material = healthbar;
    }

    public void SetHealth(int health)
    {
        healthbar.SetFloat("health", health / 10f);
    }

    public void New()
    {
        healthbar.SetFloat("health", 1f);
        gameObject.SetActive(true);
    }

    public void Free()
    {
        gameObject.SetActive(false);
    }
}
