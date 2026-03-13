using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA de l'Anomaly.
/// Comportement:
/// - Wander : errance aléatoire exactement comme EnemyMazeAI
/// - FleeCommit : si le joueur est vu, l'Anomaly choisit UNE destination loin du joueur,
///   désactive temporairement sa vision, fuit jusqu'à cette destination,
///   puis réactive sa vision et reprend le Wander.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class AnomalyMazeAI : MonoBehaviour
{
    [Header("Refs")]
    public MazeGridProvider2D grid;
    public EnemyVision2D vision;
    public Transform playerOverride; // optionnel si vision.player non assigné

    [Header("Move")]
    public float moveSpeed = 4f;
    public float reachedCellDist = 0.08f;

    [Header("Pathfinding")]
    public float repathInterval = 0.35f;        // pour la fuite
    public float wanderRepathInterval = 0.6f;   // pour l'errance

    [Header("Wander")]
    public float minWanderDistance = 6f;
    public int wanderPickTries = 120;

    [Header("Flee")]
    public int fleeSampleTries = 150;           // nb de points testés pour trouver une bonne fuite
    public float fleeMinTilesAway = 10f;        // distance mini au joueur
    public bool disableVisionWhileFlee = true;  // demandé
    public float visionReenableDelay = 0.5f;    // évite un retrigger instant

    [Header("Debug")]
    public bool debugLogs = false;

    float ScaleSafe() => Mathf.Max(0.0001f, grid.WorldScale);
    float SpeedWorld() => moveSpeed / ScaleSafe();
    float ReachedWorld() => reachedCellDist * ScaleSafe();

    enum State { Wander, FleeCommit }
    State state = State.Wander;

    Rigidbody2D rb;
    System.Random rng;

    float repathTimer;
    float visionCooldownTimer;

    List<Vector3Int> path;
    int pathIndex;

    Vector3Int wanderGoal;
    bool hasWanderGoal;

    Vector3Int fleeGoal;
    bool hasFleeGoal;

    static readonly Vector3Int[] Neigh4 =
    {
        new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0), new Vector3Int(0,-1,0)
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rng = new System.Random(Environment.TickCount);

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        Transform target = (vision != null && vision.player != null) ? vision.player : playerOverride;
        if (grid == null || !grid.IsReady || target == null)
            return;

        if (visionCooldownTimer > 0f)
            visionCooldownTimer -= Time.fixedDeltaTime;

        bool see = false;
        if (vision != null && vision.enabled)
            see = vision.CanSeePlayerLOS();

        // Transition Wander -> FleeCommit
        if (state == State.Wander && visionCooldownTimer <= 0f && see)
        {
            state = State.FleeCommit;
            path = null;
            pathIndex = 0;
            repathTimer = 0f;
            hasFleeGoal = false;

            if (disableVisionWhileFlee && vision != null)
                vision.enabled = false;
        }

        // Update état
        if (state == State.FleeCommit) UpdateFleeCommit(target);
        else UpdateWander();

        if (debugLogs)
            Debug.Log($"[Anomaly] state={state} see={see} pathLen={(path == null ? 0 : path.Count)} idx={pathIndex}");
    }

    /// <summary>
    /// Wander identique à la logique de base de EnemyMazeAI
    /// </summary>
    void UpdateWander()
    {
        repathTimer -= Time.fixedDeltaTime;

        if (!hasWanderGoal || path == null || path.Count == 0 || pathIndex >= path.Count)
        {
            PickNewWanderGoal();
            repathTimer = 0f;
        }

        if (!hasWanderGoal)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (repathTimer <= 0f)
        {
            repathTimer = wanderRepathInterval;
            ComputePathToCell(wanderGoal);

            if (path == null || path.Count < 2)
                hasWanderGoal = false;
        }

        FollowPathOrStop();
    }

    /// <summary>
    /// L'Anomaly choisit un point de fuite loin du joueur,
    /// s'y dirige, puis réactive sa vision une fois arrivée.
    /// </summary>
    void UpdateFleeCommit(Transform target)
    {
        // On choisit UNE destination de fuite une seule fois
        if (!hasFleeGoal)
        {
            fleeGoal = PickFleeGoal(target);
            hasFleeGoal = true;

            ComputePathToCell(fleeGoal);

            if (path == null || path.Count < 2)
            {
                hasFleeGoal = false;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        FollowPathOrStop();

        // Arrivé au point de fuite
        if (path != null && path.Count > 0 && pathIndex >= path.Count - 1)
        {
            Vector2 endPos = grid.CellCenterWorld(path[path.Count - 1]);
            if (Vector2.Distance(rb.position, endPos) <= ReachedWorld())
            {
                state = State.Wander;
                path = null;
                pathIndex = 0;
                hasFleeGoal = false;
                hasWanderGoal = false;

                if (vision != null)
                    vision.enabled = true;

                visionCooldownTimer = visionReenableDelay;
            }
        }
    }

    void FollowPathOrStop()
    {
        if (path == null || path.Count < 2 || pathIndex >= path.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3Int targetCell = path[pathIndex];
        Vector2 targetPos = grid.CellCenterWorld(targetCell);

        if (Vector2.Distance(rb.position, targetPos) <= ReachedWorld() && pathIndex < path.Count - 1)
            pathIndex++;

        Vector2 nextPos = Vector2.MoveTowards(rb.position, targetPos, SpeedWorld() * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }

    void ComputePathToCell(Vector3Int goal)
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        goal = SnapToNearestWalkable(goal);

        path = AStar(start, goal);
        pathIndex = (path != null && path.Count > 1) ? 1 : 0;
    }

    void PickNewWanderGoal()
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        Vector2Int size = grid.GridSize;

        for (int i = 0; i < wanderPickTries; i++)
        {
            int x = rng.Next(1, size.x - 1);
            int y = rng.Next(1, size.y - 1);
            var c = new Vector3Int(x, y, 0);
            if (!grid.IsWalkable(c)) continue;

            float d = Mathf.Abs(c.x - start.x) + Mathf.Abs(c.y - start.y);
            if (d < minWanderDistance) continue;

            wanderGoal = c;
            hasWanderGoal = true;
            return;
        }

        foreach (var d in Neigh4)
        {
            var n = start + d;
            if (grid.IsWalkable(n))
            {
                wanderGoal = n;
                hasWanderGoal = true;
                return;
            }
        }

        hasWanderGoal = false;
    }

    Vector3Int PickFleeGoal(Transform target)
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        Vector3Int playerCell = SnapToNearestWalkable(grid.WorldToCell(target.position));
        Vector2Int size = grid.GridSize;

        Vector3Int best = start;
        float bestScore = -999999f;

        for (int i = 0; i < fleeSampleTries; i++)
        {
            int x = rng.Next(1, size.x - 1);
            int y = rng.Next(1, size.y - 1);
            var c = new Vector3Int(x, y, 0);
            if (!grid.IsWalkable(c)) continue;

            float distToPlayer = Mathf.Abs(c.x - playerCell.x) + Mathf.Abs(c.y - playerCell.y);
            float distFromStart = Mathf.Abs(c.x - start.x) + Mathf.Abs(c.y - start.y);

            // on ignore les points trop proches du joueur OU trop proches de l'anomaly
            if (distToPlayer < fleeMinTilesAway) continue;
            if (distFromStart < minWanderDistance * 1.5f) continue;

            // on privilégie surtout l'éloignement du joueur
            float score = distToPlayer * 2f + distFromStart * 0.5f;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        // fallback : le plus loin possible du joueur
        if (best == start)
        {
            for (int i = 0; i < fleeSampleTries; i++)
            {
                int x = rng.Next(1, size.x - 1);
                int y = rng.Next(1, size.y - 1);
                var c = new Vector3Int(x, y, 0);
                if (!grid.IsWalkable(c)) continue;

                float distToPlayer = Mathf.Abs(c.x - playerCell.x) + Mathf.Abs(c.y - playerCell.y);
                if (distToPlayer > bestScore)
                {
                    bestScore = distToPlayer;
                    best = c;
                }
            }
        }

        return best;
    }

    Vector3Int SnapToNearestWalkable(Vector3Int start)
    {
        if (grid.IsWalkable(start)) return start;

        Vector2Int size = grid.GridSize;
        bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;

        var q = new Queue<Vector3Int>();
        var seen = new HashSet<Vector3Int>();

        q.Enqueue(start);
        seen.Add(start);

        int maxSteps = size.x * size.y;
        int steps = 0;

        while (q.Count > 0 && steps++ < maxSteps)
        {
            var cur = q.Dequeue();
            foreach (var d in Neigh4)
            {
                var n = cur + d;
                if (!InBounds(n) || seen.Contains(n)) continue;
                if (grid.IsWalkable(n)) return n;
                seen.Add(n);
                q.Enqueue(n);
            }
        }

        return start;
    }

    // ========================= A* =========================

    List<Vector3Int> AStar(Vector3Int start, Vector3Int goal)
    {
        Vector2Int size = grid.GridSize;

        bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;
        int Heur(Vector3Int a, Vector3Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        if (!InBounds(start) || !InBounds(goal)) return null;
        if (!grid.IsWalkable(start) || !grid.IsWalkable(goal)) return null;

        var open = new List<Vector3Int> { start };
        var came = new Dictionary<Vector3Int, Vector3Int>();
        var g = new Dictionary<Vector3Int, int> { [start] = 0 };
        var f = new Dictionary<Vector3Int, int> { [start] = Heur(start, goal) };

        while (open.Count > 0)
        {
            int best = 0;
            int bestF = f[open[0]];
            for (int i = 1; i < open.Count; i++)
            {
                int fi = f[open[i]];
                if (fi < bestF) { bestF = fi; best = i; }
            }

            var current = open[best];
            open.RemoveAt(best);

            if (current == goal) return Reconstruct(came, current);

            foreach (var d in Neigh4)
            {
                var n = current + d;
                if (!InBounds(n) || !grid.IsWalkable(n)) continue;

                int tentative = g[current] + 1;
                if (!g.TryGetValue(n, out int old) || tentative < old)
                {
                    came[n] = current;
                    g[n] = tentative;
                    f[n] = tentative + Heur(n, goal);
                    if (!open.Contains(n)) open.Add(n);
                }
            }
        }

        return null;
    }

    List<Vector3Int> Reconstruct(Dictionary<Vector3Int, Vector3Int> came, Vector3Int cur)
    {
        var p = new List<Vector3Int> { cur };
        while (came.TryGetValue(cur, out var prev))
        {
            cur = prev;
            p.Add(cur);
        }
        p.Reverse();
        return p;
    }
}