using System;
using UnityEngine;

public class LoACharacterTrapDamage : MonoBehaviour
{
    [SerializeField] private CharacterHealthScript characterHealthScript;
    [SerializeField] private float trapDamage = 20f;

    private int trapLayer;

    void Awake()
    {
        trapLayer = LayerMask.NameToLayer("LoATrap");
    }

    void Reset()
    {
        characterHealthScript = GetComponent<CharacterHealthScript>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != trapLayer) return;
        characterHealthScript.TakeDamage(trapDamage);
    }
}
