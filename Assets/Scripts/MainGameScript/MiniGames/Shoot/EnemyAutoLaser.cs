using UnityEngine;

public class EnemyAutoLaser : MonoBehaviour
{
    public LaserWeapon2D weapon;
    public EnemyVision2D vision;

    [Header("Range (world)")]
    public float fireRangeWorld = 1.0f;

    void Update()
    {
        if (weapon == null || vision == null || vision.player == null) return;

        float d = Vector2.Distance(transform.position, vision.player.position);
        if (d > fireRangeWorld) return;

        if (!vision.CanSeePlayerLOS()) return;

        Vector2 dir = ((Vector2)vision.player.position - (Vector2)weapon.firePoint.position).normalized;
        weapon.Fire(dir, gameObject);
    }
}