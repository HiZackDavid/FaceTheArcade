using Unity.VisualScripting;
using UnityEngine;

public class PlayerSurvivalCollisionDetectionScript : CharacterHealthScript
{
    public EnemySurvivalMovementScript enemyPrefab;
    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.name.Equals(enemyPrefab.gameObject.name))
        {
            other.transform.GetComponent<EnemySurvivalCollisionDetectionScript>().PlayerTouched();
            Destroy(other.gameObject);
            TakeDamage(Random.Range(2, 6));
        }
    }
}
