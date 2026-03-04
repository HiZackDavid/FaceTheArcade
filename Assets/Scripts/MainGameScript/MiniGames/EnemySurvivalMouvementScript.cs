using UnityEngine;

public class EnemySurvivalMouvementScript : MonoBehaviour
{
    public Transform player;
    public float speed;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        RotateEnemy();
    }
    
    private void RotateEnemy()
    {
        // Direction enemy will face towards (Pt arrival - Pt departure)
        Vector2 rotateDirection = (player.position - transform.position).normalized;

        // Calculate angle in degrees to where the player should face
        float angle = Mathf.Atan2(rotateDirection.y, rotateDirection.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees, Y axis will look at player
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
