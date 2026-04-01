using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [SerializeField] private float damageToPlayer = 10f;
    [SerializeField] private float damageToSelf = 9999f;

    private CharacterHealthScript enemyHealth;
    private bool hasHit = false;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<CharacterHealthScript>();
    }

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (!other.CompareTag("PlayerHitbox")) return;

        CharacterHealthScript playerHealth = other.GetComponentInParent<CharacterHealthScript>();
        if (playerHealth == null)
        {
            Debug.LogWarning("CharacterHealthScript introuvable sur le joueur.");
            return;
        }

        hasHit = true;
        
        MiniGameSfx.I?.PlayEnemyTouch();

        // dégâts au joueur
        playerHealth.TakeDamage(damageToPlayer);

        // mort de l'ennemi via les dégâts pour laisser RespawnAfterDeath gérer le respawn
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damageToSelf);
    }
}