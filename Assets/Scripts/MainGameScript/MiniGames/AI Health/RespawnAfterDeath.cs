using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterHealthScript))]
public class RespawnAfterDeath : MonoBehaviour
{
    [Header("Respawn")]
    public MazeGridProvider2D grid;
    public float respawnDelay = 5f;

    [Header("Respawn Health")]
    [Range(1f, 100f)] public float respawnHealthPercent = 20f; // ennemi revient à 20%

    [Header("Optional (recommended)")]
    public Transform visualRoot; // mets ici ton "PlayerGraphics"/"EnemyGraphics" si tu as un GO visuel

    CharacterHealthScript hp;
    Rigidbody2D rb;

    bool dead;
    bool respawning;

    // cached
    MonoBehaviour[] scriptsToToggle;
    Collider2D[] collidersToToggle;
    Renderer[] renderersToToggle;
    LineRenderer[] linesToToggle;

    void Awake()
    {
        hp = GetComponent<CharacterHealthScript>();
        rb = GetComponent<Rigidbody2D>();

        CacheAll();
    }

    void OnEnable()
    {
        if (hp == null) hp = GetComponent<CharacterHealthScript>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (scriptsToToggle == null || collidersToToggle == null || renderersToToggle == null || linesToToggle == null)
            CacheAll();

        hp.OnHealthChanged -= OnHealthChanged;
        hp.OnHealthChanged += OnHealthChanged;

        StopAllCoroutines();

        dead = false;
        respawning = false;

        SetAlive(true);

        // remet la vie dans un état valide au redémarrage
        hp.TakeDamage(-9999f);

        float toDamage = Mathf.Clamp(100f - respawnHealthPercent, 0f, 100f);
        if (toDamage > 0f) hp.TakeDamage(toDamage);
    }

    void OnDisable()
    {
        if (hp != null) hp.OnHealthChanged -= OnHealthChanged;
        dead = false;
        respawning = false;
    }

    void CacheAll()
    {
        // Tous les scripts sur root + enfants (même inactifs)
        var allScripts = GetComponentsInChildren<MonoBehaviour>(true);
        var list = new List<MonoBehaviour>();
        foreach (var s in allScripts)
        {
            if (s == null) continue;
            if (s == this) continue;
            if (s is CharacterHealthScript) continue; // ne jamais désactiver
            list.Add(s);
        }
        scriptsToToggle = list.ToArray();

        // colliders / renderers / lines (root + enfants)
        collidersToToggle = GetComponentsInChildren<Collider2D>(true);
        renderersToToggle = GetComponentsInChildren<Renderer>(true);
        linesToToggle = GetComponentsInChildren<LineRenderer>(true);
    }

    void OnHealthChanged(float hpPercent)
    {
        if (respawning) return;
        if (!dead && hpPercent <= 0f)
        {
            dead = true;
            StartCoroutine(CoRespawn());
        }
    }

    IEnumerator CoRespawn()
    {
        respawning = true;

        SetAlive(false);

        yield return new WaitForSeconds(respawnDelay);

        // Teleport random walkable
        if (grid != null && grid.IsReady)
        {
            var size = grid.GridSize;
            for (int tries = 0; tries < 500; tries++)
            {
                int x = Random.Range(1, size.x - 1);
                int y = Random.Range(1, size.y - 1);
                var cell = new Vector3Int(x, y, 0);
                if (!grid.IsWalkable(cell)) continue;

                Vector3 pos = grid.CellCenterWorld(cell);
                if (rb != null) rb.position = pos; // mieux que transform.position
                else transform.position = pos;

                break;
            }
        }

        // Heal full (sans toucher CharacterHealthScript)
        hp.TakeDamage(-9999f);

        // Met la vie à respawnHealthPercent (en %), sans modifier CharacterHealthScript
        // On part du principe que le script est basé sur 0-100 (vu que OnHealthChanged donne un %).
        float toDamage = Mathf.Clamp(100f - respawnHealthPercent, 0f, 100f);
        if (toDamage > 0f) hp.TakeDamage(toDamage);

        dead = false;
        respawning = false;

        SetAlive(true);
    }

    void SetAlive(bool alive)
    {
        // stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = alive;
        }

        // coupe les lasers laissés à l’écran
        if (linesToToggle != null)
        {
            foreach (var l in linesToToggle)
                if (l) l.enabled = false;
        }

        // scripts
        if (scriptsToToggle != null)
        {
            foreach (var s in scriptsToToggle)
                if (s) s.enabled = alive;
        }

        // colliders
        if (collidersToToggle != null)
        {
            foreach (var c in collidersToToggle)
                if (c) c.enabled = alive;
        }

        // renderers
        if (renderersToToggle != null)
        {
            foreach (var r in renderersToToggle)
                if (r) r.enabled = alive;
        }

        // si ton visuel est carrément un GameObject qui se désactive
        if (visualRoot != null)
            visualRoot.gameObject.SetActive(true); // on force ON au respawn
    }
}