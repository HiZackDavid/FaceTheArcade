using System.Collections;
using UnityEngine;

public class LaserWeapon2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public LineRenderer[] lines;

    [Header("Masks")]
    public LayerMask wallsMask;
    public LayerMask hitMask;

    [Header("Laser")]
    public float rangeWorld = 1.2f;
    public float damage = 10f;
    public float cooldown = 0.25f;
    public float beamTime = 0.1f;

    [Header("FirePoint dynamic")]
    public bool useDynamicStart = true;
    public float muzzleOffsetWorld = 0.03f;
    public Transform muzzleOrigin;

    float cd;

    void Awake()
    {
        if (firePoint == null) firePoint = transform;

        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == null) continue;

                lines[i].positionCount = 2;
                lines[i].useWorldSpace = true;
                lines[i].sortingLayerName = "UI";
                lines[i].sortingOrder = 200 + i; // léger décalage
                lines[i].enabled = false;
            }
        }
    }

    void Update()
    {
        if (cd > 0f) cd -= Time.deltaTime;
    }

    public bool CanFire() => cd <= 0f;

    public void Fire(Vector2 dir, GameObject owner)
    {
        if (!CanFire()) return;
        cd = cooldown;

        MiniGameSfx.I?.PlayLaserShoot();

        dir = (dir.sqrMagnitude < 0.001f) ? Vector2.right : dir.normalized;

        Vector2 start;
        if (useDynamicStart)
        {
            var origin = (muzzleOrigin != null) ? muzzleOrigin.position : transform.position;
            start = (Vector2)origin + dir * muzzleOffsetWorld;
        }
        else
        {
            start = firePoint.position;
        }

        float maxDist = Mathf.Max(0.01f, rangeWorld);

        int combined = wallsMask.value | hitMask.value;
        Debug.DrawRay(start, dir * maxDist, Color.magenta, 1f);
        RaycastHit2D hit = Physics2D.Raycast(start, dir, maxDist, combined);

        Vector2 end = start + dir * maxDist;

        if (hit.collider != null)
        {
            end = hit.point;

            if (((1 << hit.collider.gameObject.layer) & wallsMask) == 0)
            {
                MiniGameSfx.I?.PlayLaserHit();

                var hp = hit.collider.GetComponentInParent<CharacterHealthScript>();
                if (hp != null)
                {
                    var proxy = hit.collider.GetComponentInParent<HealthDeathProxy>();
                    if (proxy != null) proxy.RegisterDamager(owner);

                    hp.TakeDamage(damage);
                }
            }
        }

        if (lines != null && lines.Length > 0)
            StartCoroutine(ShowBeam(start, end));
    }

    IEnumerator ShowBeam(Vector2 start, Vector2 end)
    {
        float z = firePoint.position.z;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null) continue;

            lines[i].enabled = true;
            lines[i].SetPosition(0, new Vector3(start.x, start.y, z));
            lines[i].SetPosition(1, new Vector3(end.x, end.y, z));
        }

        yield return new WaitForSeconds(beamTime);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null) continue;
            lines[i].enabled = false;
        }
    }
}