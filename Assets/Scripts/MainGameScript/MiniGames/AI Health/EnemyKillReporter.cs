using UnityEngine;

[RequireComponent(typeof(HealthDeathProxy))]
public class EnemyKillReporter : MonoBehaviour
{
    public bool respawnAfterDeath = true;
    public float respawnDelay = 5f;

    public MazeGridProvider2D grid; // assign
    public MonoBehaviour[] disableOnDeath;
    public Collider2D[] collidersToDisable;
    public Renderer[] renderersToDisable;

    HealthDeathProxy proxy;

    void Awake()
    {
        proxy = GetComponent<HealthDeathProxy>();
        proxy.OnDied += OnDied;
    }

    void OnDestroy()
    {
        if (proxy != null) proxy.OnDied -= OnDied;
    }

    void OnDied(GameObject killer)
    {
        // +1 kill si killer a un KillTracker
        if (killer != null)
        {
            var kt = killer.GetComponentInParent<KillTracker>();
            if (kt != null) kt.RegisterKill();
        }

        if (respawnAfterDeath) StartCoroutine(Respawn());
        else SetEnabled(false);
    }

    System.Collections.IEnumerator Respawn()
    {
        SetEnabled(false);
        yield return new WaitForSeconds(respawnDelay);

        // reposition random walkable
        if (grid != null && grid.IsReady)
        {
            var size = grid.GridSize;
            for (int tries = 0; tries < 500; tries++)
            {
                int x = Random.Range(1, size.x - 1);
                int y = Random.Range(1, size.y - 1);
                var c = new Vector3Int(x, y, 0);
                if (!grid.IsWalkable(c)) continue;

                transform.position = grid.CellCenterWorld(c);
                break;
            }
        }

        // Reset vie : CharacterHealthScript reset à OnDisable(), donc on force disable/enable du composant HP
        var hp = GetComponent<CharacterHealthScript>();
        hp.enabled = false;
        hp.enabled = true;

        SetEnabled(true);
    }

    void SetEnabled(bool on)
    {
        if (disableOnDeath != null) foreach (var s in disableOnDeath) if (s) s.enabled = on;
        if (collidersToDisable != null) foreach (var c in collidersToDisable) if (c) c.enabled = on;
        if (renderersToDisable != null) foreach (var r in renderersToDisable) if (r) r.enabled = on;
    }
}