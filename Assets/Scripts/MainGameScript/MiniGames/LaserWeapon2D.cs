using System.Collections;
using UnityEngine;

public class LaserWeapon2D : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public LineRenderer line;

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
    public float muzzleOffsetWorld = 0.03f; // distance devant le tank (monde)
    public Transform muzzleOrigin;          // le centre du tank (souvent transform du player)

    float cd;

    void Awake()
    {
        if (firePoint == null) firePoint = transform;

        if (line != null)
        {
            line.positionCount = 2;
            line.useWorldSpace = true;

            // force sorting devant
            line.sortingLayerName = "UI";
            line.sortingOrder = 200;

            line.enabled = false;
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

        Debug.LogError(
            $"RAY start={start} dir={dir} dist={maxDist} mask={combined} " +
            $"-> hit={(hit.collider ? hit.collider.name : "NONE")}"
        );

        Vector2 end = start + dir * maxDist;

        if (hit.collider != null)
        {
            end = hit.point;

            // si pas un mur => dégâts
            if (((1 << hit.collider.gameObject.layer) & wallsMask) == 0)
            {
                var hp = hit.collider.GetComponentInParent<CharacterHealthScript>();
                if (hp != null)
                {
                    var proxy = hit.collider.GetComponentInParent<HealthDeathProxy>();
                    if (proxy != null) proxy.RegisterDamager(owner);

                    hp.TakeDamage(damage);
                }
            }
        }

        if (line != null) StartCoroutine(ShowBeam(start, end));        

    }

    IEnumerator ShowBeam(Vector2 start, Vector2 end)
    {
        float z = firePoint.position.z;

        line.enabled = true;
        line.SetPosition(0, new Vector3(start.x, start.y, z));
        line.SetPosition(1, new Vector3(end.x, end.y, z));

        yield return new WaitForSeconds(beamTime);
        line.enabled = false;
    }
}