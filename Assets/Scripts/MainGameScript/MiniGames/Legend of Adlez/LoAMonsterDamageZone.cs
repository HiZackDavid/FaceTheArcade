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
        Debug.Log("Monster zone touched: " + other.name + " | layer = " + LayerMask.LayerToName(other.gameObject.layer));
        
        if (other.gameObject.layer != _playerLayer) return;
        
        Debug.Log("The monster touched a player");

        CharacterHealthScript health = other.GetComponent<CharacterHealthScript>();
        
        if (health == null)
        {
            health = other.GetComponentInParent<CharacterHealthScript>();
        }

        if (health == null)
        {
            Debug.LogWarning("No CharacterHealthScript found on player.");
            return;
        }
        
        Debug.Log("Monster dealt damage.");
        health.TakeDamage(contactDamage);
    }
}
