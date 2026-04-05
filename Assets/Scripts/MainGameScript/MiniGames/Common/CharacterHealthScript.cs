using System;
using System.Collections;
using UnityEngine;

public class CharacterHealthScript : MonoBehaviour
{
    private readonly float _maxHealth = 100;
    private float _currentHealth;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hurtClips;
    
    [Header("Damage Flicker")]
    [SerializeField] private SpriteRenderer[] SpriteRenderers;
    [SerializeField] private float flickerDuration = 0.2f;
    [SerializeField] private float flickerInterval = 0.05f;
    [SerializeField] [Range(0f, 1f)] private float lowAlpha = 0.25f;
    
    bool _isFlickering = false;
    private int _lastPlayedIndex = -1;

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

    IEnumerator FlickerEntity()
    {
        if (SpriteRenderers == null || SpriteRenderers.Length == 0) yield break;
        
        _isFlickering = true;
        
        float elapsedTime = 0f;
        bool low = true;

        while (elapsedTime < flickerInterval)
        {
            SetAlpha(low ? lowAlpha : 1f);
            low = !low;
            
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += flickerInterval;
        }
        
        SetAlpha(1f);
        _isFlickering = false;
    }

    void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer sr in SpriteRenderers)
        {
            if (!sr) continue;
            
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        _currentHealth = Math.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth / _maxHealth * 100);
        
        PlayRandomHurtClip();

        if (!_isFlickering)
        {
            StartCoroutine(FlickerEntity());
        }
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
