using System;
using UnityEngine;

public class LoACharacterTrapDamage : MonoBehaviour
{
    [SerializeField] private CharacterHealthScript characterHealthScript;
    [SerializeField] private float trapDamage = 20f;

    private void Reset()
    {
        characterHealthScript = GetComponent<CharacterHealthScript>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("LoATrap"))
        {
            characterHealthScript.TakeDamage(trapDamage);
        }
    }
}
