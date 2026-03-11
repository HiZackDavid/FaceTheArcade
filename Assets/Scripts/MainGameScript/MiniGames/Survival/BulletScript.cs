using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public PlayerSurvivalShootScript playerPrefab;
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy bullet if hits anything (except player)
        if (!collision.gameObject.name.Equals(playerPrefab.playerGraphics.name ) && !collision.gameObject.name.Equals(transform.name))
        {
            Destroy(gameObject);
        }
    }
}
