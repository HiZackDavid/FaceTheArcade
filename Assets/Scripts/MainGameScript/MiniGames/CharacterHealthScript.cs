using System;
using UnityEngine;

public class CharacterHealthScript : MonoBehaviour
{
    private readonly int maxHealth = 100;
    private int currentHealth;

    public event Action<int> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;

    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Math.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
}
