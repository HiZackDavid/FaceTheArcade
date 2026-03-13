using System;
using UnityEngine;

public class CharacterHealthScript : MonoBehaviour
{
    private readonly float maxHealth = 100;
    private float currentHealth;

    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Math.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth * 100);
    }

    public bool IsDead()
    {
        return currentHealth == 0;
    }
    
    void OnDisable()
    {
        currentHealth = maxHealth;
        OnHealthChanged = null;
    }
}
