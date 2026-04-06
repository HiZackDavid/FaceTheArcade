using System;
using UnityEngine;

public class LoAMonsterDamageZone : MonoBehaviour
{
    [SerializeField] private float contactDamage = 20f;

    private int _playerLayer;

    private void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("LoAPlayerHurtbox");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.layer != _playerLayer) return;
        
        CharacterHealthScript health = other.GetComponent<CharacterHealthScript>();
        
        if (!health)
        {
            health = other.GetComponentInParent<CharacterHealthScript>();
        }
        
        health.TakeDamage(contactDamage);
    }
}
