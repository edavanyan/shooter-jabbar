using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    private const int MaxHealth = 10;
    public  int Health 
    {
        get; set;
    }
    
    private void Awake()
    {
        Health = MaxHealth;
    }

    public void Damage(int amount)
    {
        Health -= amount;
        if (Health >= MaxHealth)
        {
            Health = MaxHealth;
        }
    }

    public void Reset()
    {
        Health = MaxHealth;
    }
}
