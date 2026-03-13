using UnityEngine;

[RequireComponent(typeof(CharacterHealthScript))]
public class EnemyHealthInit : MonoBehaviour
{
    [Range(1, 100)] public float startHealth = 40f;

    void Start()
    {
        var hp = GetComponent<CharacterHealthScript>();
        // suppose maxHealth=100 (cas le plus courant)
        hp.TakeDamage(100f - startHealth);
    }
}