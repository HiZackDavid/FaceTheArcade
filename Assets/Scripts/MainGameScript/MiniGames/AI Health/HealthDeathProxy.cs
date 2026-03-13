using System;
using UnityEngine;

[RequireComponent(typeof(CharacterHealthScript))]
public class HealthDeathProxy : MonoBehaviour
{
    public event Action<GameObject> OnDied; // killer GameObject (peut être null)

    public bool IsDead { get; private set; }
    public GameObject LastDamager { get; private set; }

    CharacterHealthScript hp;

    void Awake()
    {
        hp = GetComponent<CharacterHealthScript>();
        hp.OnHealthChanged += OnHealthChanged;
    }

    void OnDestroy()
    {
        if (hp != null) hp.OnHealthChanged -= OnHealthChanged;
    }

    void OnDisable()
    {
        // CharacterHealthScript reset sa vie à OnDisable, donc on reset notre état
        IsDead = false;
        LastDamager = null;
    }

    public void RegisterDamager(GameObject damager)
    {
        LastDamager = damager;
    }

    void OnHealthChanged(float percent0To100)
    {
        if (IsDead) return;
        if (percent0To100 <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke(LastDamager);
        }
    }
}