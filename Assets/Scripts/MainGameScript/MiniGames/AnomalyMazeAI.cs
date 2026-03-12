using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnomalyMazeAI : MonoBehaviour
{
    [Header("Refs")]
    public MazeGridProvider2D grid;
    public EnemyVision2D vision;          // utilise CanSeePlayerLOS() + InAggroRange()
    public Transform player;              // optionnel: si vision.player est null

    [Header("Move")]
    public float moveSpeed = 0.08f;
    public float reachedCellDist = 0.10f;

    [Header("Pathfinding")]
    public float repathInterval = 0.35f;        // recalcul A* en flee
    public float wanderRepathInterval = 0.6f;   // recalcul A* en wander

    [Header("Wander")]
    public float minWanderDistance = 2f;
    public int wanderPickTries = 200;

    [Header("Flee")]
    public int fleeSampleTries = 120;       // nb de cases random testées
    public float fleeMinTilesAway = 10f;    // au moins X tiles loin du joueur (Manhattan)
    public bool fleeWhenSeeOnly = true;     // true: fuit seulement si LOS, false: fuit si aggro aussi

    enum State { Wander, Flee }
    State state = State.Wander;

    Rigidbody2D rb;
    System.Random rng;

    float repathTimer;
    List<Vector3Int> path;
    int pathIndex;

    Vector3Int wanderGoal;
    bool hasWanderGoal;

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
        if (grid == null || !grid.IsReady) return;

        Transform target = (vision != null && vision.player != null) ? vision.player : player;
        if (target == null) return;

        bool see = vision != null ? vision.CanSeePlayerLOS() : true;
        bool aggro = vision != null ? vision.InAggroRange() : true;

        bool shouldFlee = fleeWhenSeeOnly ? see : (see || aggro);

        // transitions
        if (state == State.Wander && shouldFlee)
        {
            state = State.Flee;
            path = null;
            pathIndex = 0;
            repathTimer = 0f;
        }
        else if (state == State.Flee && !aggro && !see)
        {
            state = State.Wander;
            path = null;
            pathIndex = 0;
            hasWanderGoal = false;
            repathTimer = 0f;
        }

        if (state == State.Flee) UpdateFlee(target);
        else UpdateWander();

        // Debug.Log($"Anomaly State={state} see={see} aggro={aggro}");
    }

    void UpdateFlee(Transform target)
    {
        repathTimer -= Time.fixedDeltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            ComputeFleePath(target);
        }

        FollowPath();
    }

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

            if (path == null || path.Count == 0)
                hasWanderGoal = false;
        }

        FollowPath();
    }

    void ComputeFleePath(Transform target)
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        Vector3Int pCell = SnapToNearestWalkable(grid.WorldToCell(target.position));

        Vector2Int size = grid.GridSize;

        // Choisir une case walkable "loin du joueur"
        Vector3Int best = start;
        float bestScore = -999999f;

        for (int i = 0; i < fleeSampleTries; i++)
        {
            int x = rng.Next(1, size.x - 1);
            int y = rng.Next(1, size.y - 1);
            var c = new Vector3Int(x, y, 0);

            if (!grid.IsWalkable(c)) continue;

            float dist = Mathf.Abs(c.x - pCell.x) + Mathf.Abs(c.y - pCell.y); // Manhattan
            if (dist < fleeMinTilesAway) continue;

            // bonus: préfère aussi s’éloigner du start pour éviter micro-mouvements
            float fromStart = Mathf.Abs(c.x - start.x) + Mathf.Abs(c.y - start.y);
            float score = dist + 0.15f * fromStart;

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        // fallback si pas trouvé assez loin: prendre la plus loin possible
        if (best == start)
        {
            for (int i = 0; i < fleeSampleTries; i++)
            {
                int x = rng.Next(1, size.x - 1);
                int y = rng.Next(1, size.y - 1);
                var c = new Vector3Int(x, y, 0);
                if (!grid.IsWalkable(c)) continue;

                float dist = Mathf.Abs(c.x - pCell.x) + Mathf.Abs(c.y - pCell.y);
                if (dist > bestScore)
                {
                    bestScore = dist;
                    best = c;
                }
            }
        }

        path = AStar(start, best);
        pathIndex = 0;

        // si path impossible => essaye juste un pas "opposé"
        if (path == null || path.Count == 0)
        {
            Vector3Int dir = new Vector3Int(
                Math.Sign(start.x - pCell.x),
                Math.Sign(start.y - pCell.y),
                0
            );

            var n = start + dir;
            if (grid.IsWalkable(n))
            {
                path = new List<Vector3Int> { start, n };
                pathIndex = 0;
            }
        }
    }

    void ComputePathToCell(Vector3Int goal)
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        goal = SnapToNearestWalkable(goal);

        path = AStar(start, goal);
        pathIndex = 0;
    }

    void FollowPath()
    {
        if (path == null || path.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3Int targetCell = path[Mathf.Min(pathIndex, path.Count - 1)];
        Vector2 targetPos = grid.CellCenterWorld(targetCell);

        if (Vector2.Distance(rb.position, targetPos) <= reachedCellDist && pathIndex < path.Count - 1)
            pathIndex++;

        Vector2 next = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);
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

    Vector3Int SnapToNearestWalkable(Vector3Int c)
    {
        if (grid.IsWalkable(c)) return c;

        var q = new Queue<Vector3Int>();
        var seen = new HashSet<Vector3Int>();
        q.Enqueue(c);
        seen.Add(c);

        int steps = 0;
        while (q.Count > 0 && steps++ < 30)
        {
            var cur = q.Dequeue();
            foreach (var d in Neigh4)
            {
                var n = cur + d;
                if (seen.Contains(n)) continue;
                seen.Add(n);
                if (grid.IsWalkable(n)) return n;
                q.Enqueue(n);
            }
        }

        return c;
    }

    // ---------------- A* ----------------
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