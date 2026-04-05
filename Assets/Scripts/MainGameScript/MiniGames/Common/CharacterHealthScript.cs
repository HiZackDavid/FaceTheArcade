using System;
using UnityEngine;

public class CharacterHealthScript : MonoBehaviour
{
    private readonly float _maxHealth = 100;
    private float _currentHealth;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hurtClips;

    public event Action<float> OnHealthChanged;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    void PlayRandomHurtClip()
    {
        if (!audioSource || hurtClips == null || hurtClips.Length == 0) return;
        
        int randomIndex = UnityEngine.Random.Range(0, hurtClips.Length);
        AudioClip clip = hurtClips[randomIndex];
        
        audioSource.PlayOneShot(clip);
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        _currentHealth = Math.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth / _maxHealth * 100);
        
        PlayRandomHurtClip();
    }

    public bool IsDead()
    {
        return _currentHealth <= 0;
    }
    
    void OnDisable()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged = null;
    }
}
