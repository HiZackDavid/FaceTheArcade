using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA de l'ennemi dans le labyrinthe.
/// L'ennemi possède deux comportements (états) : Wander (errance aléatoire) et Chase (poursuite du joueur).
/// Le pathfinding utilise un algorithme A* sur la grille 2D du labyrinthe.
/// La machine à états gère les transitions entre errance et poursuite selon la vision de l'ennemi.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMazeAI : MonoBehaviour
{
    // === Références aux composants externes ===
    [Header("Refs")]
    public MazeGridProvider2D grid;      // Fournit les données de la grille du labyrinthe (cases walkable, taille, etc.)
    public EnemyVision2D vision;         // Gère la détection du joueur (line of sight, range d'aggro)

    // === Paramètres de déplacement ===
    [Header("Move")]
    public float moveSpeed = 4f;         // Vitesse de déplacement de l'ennemi (unités/s)
    public float reachedCellDist = 0.08f;// Distance seuil pour considérer qu'on a atteint le centre d'une cellule

    // === Paramètres du pathfinding ===
    [Header("Pathfinding")]
    public float repathInterval = 0.35f;       // Intervalle (en secondes) entre deux recalculs de chemin en mode Chase
    public float wanderRepathInterval = 0.6f;  // Intervalle entre deux recalculs de chemin en mode Wander

    // === Paramètres d'errance (Wander) ===
    [Header("Wander")]
    public float minWanderDistance = 6f; // Distance Manhattan minimale pour choisir un objectif d'errance (évite les points trop proches)
    public int wanderPickTries = 120;    // Nombre max de tentatives aléatoires pour trouver un point d'errance valide

    // === Paramètres de poursuite (Chase) ===
    [Header("Chase")]
    [SerializeField] float stopDistance = 0.20f; // Distance à laquelle l'ennemi s'arrête devant le joueur (mets 0.05 si tu veux qu'il colle plus)
    private bool chaseDirect = false;            // Passe à true quand on poursuit en ligne droite (sans A*)

    // === Debug ===
    [Header("Debug")]
    public bool debugLogs = false; // Active les logs dans la console pour visualiser l'état de l'IA en temps réel

    float ScaleSafe() => Mathf.Max(0.0001f, grid.WorldScale);

    // vitesse monde corrigée
    float SpeedWorld() => moveSpeed / ScaleSafe();

    // seuils en monde (plus petit quand le monde est réduit)
    float ReachedWorld() => reachedCellDist * ScaleSafe();
    float StopWorld() => stopDistance * ScaleSafe();

    // Machine à états simple : l'ennemi est soit en train d'errer, soit en train de poursuivre le joueur
    enum State { Wander, Chase }
    State state = State.Wander; // On commence toujours en mode errance

    // Composants internes
    Rigidbody2D rb;          // Rigidbody pour le déplacement physique 2D
    System.Random rng;       // Générateur aléatoire pour choisir les destinations d'errance

    // Variables de pathfinding
    float repathTimer;       // Timer qui décompte avant le prochain recalcul de chemin
    List<Vector3Int> path;   // Liste ordonnée des cellules du chemin calculé par A*
    int pathIndex;           // Index de la prochaine cellule à atteindre dans le path

    // Variables d'errance
    Vector3Int wanderGoal;   // Cellule cible actuelle pour l'errance
    bool hasWanderGoal;      // Indique si on a un objectif d'errance valide

    // Les 4 directions cardinales (droite, gauche, haut, bas) utilisées pour le parcours de la grille
    // On les stocke en static readonly pour éviter de recréer les vecteurs à chaque frame
    static readonly Vector3Int[] Neigh4 =
    {
        new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0), new Vector3Int(0,-1,0)
    };

    /// <summary>
    /// Initialisation des composants au démarrage.
    /// On récupère le Rigidbody2D, on crée le RNG et on configure la physique.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rng = new System.Random(Environment.TickCount); // Seed basée sur le temps pour avoir des résultats différents à chaque lancement

        rb.gravityScale = 0f;     // Pas de gravité car on est en vue top-down 2D
        rb.freezeRotation = true; // On empêche la rotation du sprite par la physique
    }

    /// <summary>
    /// Boucle principale appelée à chaque pas physique (FixedUpdate).
    /// Gère les transitions entre les états Wander et Chase,
    /// puis délègue la mise à jour au bon état.
    /// </summary>
    void FixedUpdate()
    {
        if (isPaused)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            return;
        }


        // Sécurité : on ne fait rien si les références ne sont pas prêtes
        if (grid == null || vision == null || !grid.IsReady || vision.player == null)
            return;

        // On vérifie la vision à chaque frame pour savoir si on détecte le joueur
        bool see = vision.CanSeePlayerLOS();  // Est-ce qu'on a une ligne de vue directe sur le joueur ?
        bool aggro = vision.InAggroRange();    // Est-ce que le joueur est dans la zone d'aggro ?

        // --- Transitions de la machine à états ---
        // Wander -> Chase : dès qu'on voit le joueur, on passe en poursuite
        if (state == State.Wander && see)
        {
            state = State.Chase;
            path = null;        // On reset le chemin pour en calculer un nouveau vers le joueur
            pathIndex = 0;
            repathTimer = 0f;   // Forcer un recalcul immédiat
        }
        // Chase -> Wander : si le joueur sort de la zone d'aggro, on retourne errer
        else if (state == State.Chase && !aggro)
        {
            state = State.Wander;
            path = null;
            pathIndex = 0;
            hasWanderGoal = false; // On n'a plus d'objectif, il faudra en choisir un nouveau
            repathTimer = 0f;
            chaseDirect = false;
        }

        // On exécute la logique de l'état courant
        if (state == State.Chase) UpdateChase(see);
        else UpdateWander();

        // Affichage de debug dans la console Unity (utile pour tester/debugger)
        if (debugLogs)
            Debug.Log($"State={state} see={see} aggro={aggro} pathLen={(path == null ? 0 : path.Count)} idx={pathIndex} direct={chaseDirect}");
    }

    /// <summary>
    /// Mise à jour de l'état Chase (poursuite).
    /// On recalcule périodiquement le chemin A* vers le joueur.
    /// Si A* échoue mais qu'on voit le joueur, on le poursuit en ligne droite.
    /// </summary>
    void UpdateChase(bool see)
    {
        // Décompte du timer de recalcul
        repathTimer -= Time.fixedDeltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval; // On remet le timer pour le prochain recalcul
            ComputePathToPlayer();        // Recalcul du chemin A* vers la position actuelle du joueur

            // Fallback : si A* n'a pas trouvé de chemin ou que le chemin est trop court,
            // et qu'on a une ligne de vue sur le joueur, on passe en poursuite directe
            if (see && (path == null || path.Count < 2))
                chaseDirect = true;
        }

        FollowPathOrStop(); // On se déplace le long du chemin (ou en direct)
    }

    /// <summary>
    /// Mise à jour de l'état Wander (errance).
    /// L'ennemi choisit un point aléatoire dans le labyrinthe et s'y déplace via A*.
    /// Quand il arrive à destination (ou si le chemin est invalide), il choisit un nouveau point.
    /// </summary>
    void UpdateWander()
    {
        repathTimer -= Time.fixedDeltaTime;

        // Si on n'a pas d'objectif ou qu'on a fini de parcourir le chemin, on en choisit un nouveau
        if (!hasWanderGoal || path == null || path.Count == 0 || pathIndex >= path.Count)
        {
            PickNewWanderGoal();  // Tire un nouveau point aléatoire dans la grille
            repathTimer = 0f;    // Force le recalcul immédiat du chemin
        }

        // Si on n'a toujours pas trouvé de destination valide, on ne bouge pas
        if (!hasWanderGoal)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Recalcul périodique du chemin vers le point d'errance
        if (repathTimer <= 0f)
        {
            repathTimer = wanderRepathInterval;
            ComputePathToCell(wanderGoal);

            // Si A* n'a pas trouvé de chemin, on invalidera le goal et on repioche au prochain frame
            if (path == null || path.Count < 2)
                hasWanderGoal = false;
        }

        FollowPathOrStop();
    }

    /// <summary>
    /// Calcule un chemin A* depuis la position de l'ennemi jusqu'à la position du joueur.
    /// Si l'ennemi et le joueur sont sur la même cellule, on passe en poursuite directe.
    /// </summary>
    void ComputePathToPlayer()
    {
        // Conversion des positions world en coordonnées de cellules de la grille
        Vector3Int start = grid.WorldToCell(transform.position);
        Vector3Int goal = grid.WorldToCell(vision.player.position);

        // Si la cellule n'est pas walkable (ex: on est sur un mur), on snap vers la plus proche walkable
        start = SnapToNearestWalkable(start);
        goal = SnapToNearestWalkable(goal);

        // Cas trivial : ennemi et joueur sur la même cellule -> pas besoin d'A*, on fonce directement
        if (start == goal)
        {
            chaseDirect = true;
            path = null;
            pathIndex = 0;
            return;
        }

        chaseDirect = false;

        // Lancement de l'algorithme A*
        path = AStar(start, goal);
        // On commence à l'index 1 car path[0] est la cellule de départ (on y est déjà)
        pathIndex = (path != null && path.Count > 1) ? 1 : 0;
    }

    /// <summary>
    /// Calcule un chemin A* depuis la position actuelle de l'ennemi vers une cellule arbitraire.
    /// Utilisée principalement pour le mode Wander.
    /// </summary>
    void ComputePathToCell(Vector3Int goal)
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        goal = SnapToNearestWalkable(goal);

        path = AStar(start, goal);
        pathIndex = (path != null && path.Count > 1) ? 1 : 0; // Même logique : on skip la case de départ
    }

    /// <summary>
    /// Gère le déplacement de l'ennemi le long du chemin calculé.
    /// Deux modes possibles :
    ///   1) Poursuite directe (chaseDirect) : on fonce tout droit vers le joueur sans suivre le path A*
    ///   2) Suivi du path A* : on avance de cellule en cellule
    /// Si pas de chemin valide, l'ennemi s'arrête.
    /// </summary>
    void FollowPathOrStop()
    {
        // --- Mode poursuite directe (sans A*) ---
        // Activé quand on est trop proche ou que A* n'a pas trouvé de chemin mais qu'on voit le joueur
        if (state == State.Chase && chaseDirect)
        {
            Vector2 target = vision.player.position;

            // Si on est assez proche du joueur, on s'arrête (pour ne pas le traverser)
            if (Vector2.Distance(rb.position, target) <= StopWorld())
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Déplacement en ligne droite vers le joueur avec MoveTowards
            Vector2 next = Vector2.MoveTowards(rb.position, target, SpeedWorld() * Time.fixedDeltaTime);
            rb.MovePosition(next);
            return;
        }

        // --- Mode suivi du chemin A* ---
        // Pas de chemin valide -> on s'arrête
        if (path == null || path.Count < 2 || pathIndex >= path.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // On récupère la position world du centre de la prochaine cellule à atteindre
        Vector3Int targetCell = path[pathIndex];
        Vector2 targetPos = grid.CellCenterWorld(targetCell);

        // Si on est assez proche du centre de la cellule courante, on passe à la suivante
        if (Vector2.Distance(rb.position, targetPos) <= ReachedWorld() && pathIndex < path.Count - 1)
            pathIndex++;

        // Déplacement progressif vers le centre de la cellule cible
        Vector2 nextPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }

    /// <summary>
    /// Choisit un nouveau point d'errance aléatoire dans le labyrinthe.
    /// On tire des coordonnées au hasard et on vérifie qu'elles sont walkable
    /// et suffisamment éloignées de la position actuelle (distance Manhattan).
    /// Si on ne trouve rien après N essais, on prend un voisin direct comme fallback.
    /// </summary>
    void PickNewWanderGoal()
    {
        Vector3Int start = SnapToNearestWalkable(grid.WorldToCell(transform.position));
        Vector2Int size = grid.GridSize;

        // On essaie wanderPickTries fois de trouver un point aléatoire convenable
        for (int i = 0; i < wanderPickTries; i++)
        {
            // Tirage aléatoire d'une cellule (en excluant les bords de la grille)
            int x = rng.Next(1, size.x - 1);
            int y = rng.Next(1, size.y - 1);
            var c = new Vector3Int(x, y, 0);
            if (!grid.IsWalkable(c)) continue; // On ignore les murs

            // On vérifie que le point est assez loin (distance de Manhattan)
            float d = Mathf.Abs(c.x - start.x) + Mathf.Abs(c.y - start.y);
            if (d < minWanderDistance) continue; // Trop proche, on retente

            wanderGoal = c;
            hasWanderGoal = true;
            return;
        }

        // Fallback : si on n'a rien trouvé, on prend simplement une cellule voisine walkable
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

        // Aucun point trouvé (cas très rare, grille très petite ou ennemi complètement bloqué)
        hasWanderGoal = false;
    }

    private bool isPaused = false;

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Si la cellule donnée n'est pas walkable (ex: un mur), on cherche la cellule walkable
    /// la plus proche en faisant un BFS (parcours en largeur) dans les 4 directions.
    /// Ça garantit qu'on trouve toujours un point de départ/arrivée valide pour A*.
    /// </summary>
    Vector3Int SnapToNearestWalkable(Vector3Int start)
    {
        // Si la cellule est déjà walkable, pas besoin de chercher
        if (grid.IsWalkable(start)) return start;

        Vector2Int size = grid.GridSize;
        // Fonction locale pour vérifier qu'on reste dans les limites de la grille
        bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;

        // BFS classique avec une queue et un set de cellules déjà visitées
        var q = new Queue<Vector3Int>();
        var seen = new HashSet<Vector3Int>();

        q.Enqueue(start);
        seen.Add(start);

        int maxSteps = size.x * size.y; // Limite de sécurité pour éviter une boucle infinie
        int steps = 0;

        while (q.Count > 0 && steps++ < maxSteps)
        {
            var cur = q.Dequeue();
            foreach (var d in Neigh4)
            {
                var n = cur + d;
                if (!InBounds(n) || seen.Contains(n)) continue;
                if (grid.IsWalkable(n)) return n; // Trouvé ! On retourne la première cellule walkable
                seen.Add(n);
                q.Enqueue(n);
            }
        }

        // Si vraiment rien trouvé (ne devrait pas arriver), on retourne la cellule de départ
        return start;
    }

    // =====================================================================
    //                        ALGORITHME A*
    // =====================================================================
    // Implémentation de l'algorithme A* pour trouver le chemin le plus court
    // entre deux cellules de la grille du labyrinthe.
    // Heuristique utilisée : distance de Manhattan (adaptée aux grilles 4-connexes).
    // Complexité : O(n log n) en théorie avec un heap, ici O(n²) car on utilise
    // une simple List comme open set (suffisant pour nos tailles de labyrinthe).
    // =====================================================================

    /// <summary>
    /// Algorithme A* : trouve le chemin le plus court entre start et goal sur la grille.
    /// Retourne la liste ordonnée des cellules du chemin, ou null si aucun chemin n'existe.
    /// </summary>
    List<Vector3Int> AStar(Vector3Int start, Vector3Int goal)
    {
        Vector2Int size = grid.GridSize;

        // Fonctions locales pour la lisibilité
        bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;
        int Heur(Vector3Int a, Vector3Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Heuristique Manhattan

        // Vérifications de base : les deux extrémités doivent être valides et walkable
        if (!InBounds(start) || !InBounds(goal)) return null;
        if (!grid.IsWalkable(start) || !grid.IsWalkable(goal)) return null;

        // Open set : les noeuds à explorer (on commence par start)
        var open = new List<Vector3Int> { start };
        // Came from : pour reconstruire le chemin à la fin (noeud -> prédécesseur)
        var came = new Dictionary<Vector3Int, Vector3Int>();
        // g[n] : coût réel du chemin depuis start jusqu'à n
        var g = new Dictionary<Vector3Int, int> { [start] = 0 };
        // f[n] = g[n] + h(n) : coût estimé total (réel + heuristique)
        var f = new Dictionary<Vector3Int, int> { [start] = Heur(start, goal) };

        while (open.Count > 0)
        {
            // On cherche le noeud avec le plus petit f dans l'open set (recherche linéaire)
            int best = 0;
            int bestF = f[open[0]];
            for (int i = 1; i < open.Count; i++)
            {
                int fi = f[open[i]];
                if (fi < bestF) { bestF = fi; best = i; }
            }

            // On retire le meilleur noeud de l'open set pour l'explorer
            var current = open[best];
            open.RemoveAt(best);

            // Si on a atteint le goal, on reconstruit et retourne le chemin
            if (current == goal) return Reconstruct(came, current);

            // On explore les 4 voisins (haut, bas, gauche, droite)
            foreach (var d in Neigh4)
            {
                var n = current + d;
                // On ignore les voisins hors limites ou non walkable (murs)
                if (!InBounds(n) || !grid.IsWalkable(n)) continue;

                // Coût pour atteindre le voisin en passant par current (chaque pas coûte 1)
                int tentative = g[current] + 1;
                // Si on a trouvé un chemin plus court vers ce voisin, on met à jour
                if (!g.TryGetValue(n, out int old) || tentative < old)
                {
                    came[n] = current;                    // On enregistre d'où on vient
                    g[n] = tentative;                     // Mise à jour du coût réel
                    f[n] = tentative + Heur(n, goal);     // Mise à jour du coût estimé
                    if (!open.Contains(n)) open.Add(n);   // On ajoute à l'open set si pas déjà dedans
                }
            }
        }

        // Aucun chemin trouvé (le goal est inaccessible)
        return null;
    }

    /// <summary>
    /// Reconstruit le chemin en remontant le dictionnaire "came from" depuis le goal jusqu'au start.
    /// Le chemin est construit à l'envers (goal -> start) puis inversé pour obtenir l'ordre correct.
    /// </summary>
    List<Vector3Int> Reconstruct(Dictionary<Vector3Int, Vector3Int> came, Vector3Int cur)
    {
        var p = new List<Vector3Int> { cur }; // On part du goal
        // On remonte de prédécesseur en prédécesseur jusqu'au start (qui n'a pas de prédécesseur)
        while (came.TryGetValue(cur, out var prev))
        {
            cur = prev;
            p.Add(cur);
        }
        p.Reverse(); // On inverse pour avoir start -> goal
        return p;
    }
}