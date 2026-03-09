using System;
using UnityEngine;

public class LoAMonsterDamageZone : MonoBehaviour
{
    [SerializeField] private float contactDamage = 15f;

    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("LoAPlayer");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != playerLayer) return;

        CharacterHealthScript playerHealth = other.GetComponent<CharacterHealthScript>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
