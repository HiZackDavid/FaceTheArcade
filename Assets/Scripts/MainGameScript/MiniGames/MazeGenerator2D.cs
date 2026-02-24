using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Générateur procédural de labyrinthe 2D basé sur un algorithme DFS (Depth-First Search).
/// Le labyrinthe est généré sous forme de grille de tiles (murs et sols) puis affiché
/// via une Tilemap Unity. Le script gère aussi le placement aléatoire du joueur et de l'ennemi.
/// On peut aussi ajouter des boucles (chemins alternatifs) pour éviter les culs-de-sac trop longs.
/// </summary>
public class MazeGenerator2D : MonoBehaviour
{
    // === Taille du labyrinthe ===
    [Header("Maze size (in cells)")]
    [Min(2)] public int cellsX = 20;  // Nombre de cellules en largeur (minimum 2)
    [Min(2)] public int cellsY = 12;  // Nombre de cellules en hauteur (minimum 2)

    // === Graine aléatoire ===
    // Permet de reproduire le même labyrinthe en fixant la seed (utile pour le debug)
    [Header("Seed")]
    public bool useRandomSeed = true; // Si true, génère un labyrinthe différent à chaque fois
    public int seed = 12345;          // Seed fixe utilisée si useRandomSeed est false

    // === Boucles (chemins alternatifs) ===
    // Un labyrinthe DFS pur est un arbre (un seul chemin entre deux points)
    // En ouvrant un pourcentage de murs, on crée des boucles = plusieurs chemins possibles
    [Header("Loops (multiple paths)")]
    [Range(0f, 0.30f)] public float loopPercent = 0.10f; // Pourcentage de murs à ouvrir (0.05-0.15 conseillé)

    // === Rendu Tilemap ===
    [Header("Tilemap Rendering")]
    public Tilemap tilemap;       // Tilemap principale pour afficher le labyrinthe
    public TileBase floorTile;    // Tile utilisée pour les sols (passages)
    public TileBase wallTile;     // Tile utilisée pour les murs

    // === Placement du joueur ===
    [Header("Player Placement")]
    public Transform player;                    // Référence au transform du joueur pour le placer dans le labyrinthe
    public Vector3 playerOffset = Vector3.zero; // Offset optionnel pour ajuster la position de spawn du joueur

    public Tilemap floorTilemap;  // Tilemap du sol (Tilemap_Floor), utilisée pour la conversion cell -> world
    public Transform enemy;       // Référence au transform de l'ennemi pour le placer dans le labyrinthe

    // === Variables internes ===
    // La grille de tiles est plus grande que la grille de cellules :
    // chaque cellule occupe 1 tile, et entre chaque cellule il y a un mur potentiel.
    // Donc pour cellsX cellules, on a gridW = cellsX * 2 + 1 tiles en largeur (idem en hauteur).
    private int gridW, gridH;       // Dimensions de la grille de tiles (en nombre de tiles)
    private bool[,] isWall;         // true = mur, false = sol (utilisé pendant la génération)
    private bool[,] walkable;       // true = traversable (copie inversée de isWall, utilisée par le pathfinding)

    // Les 4 directions cardinales pour explorer les voisins d'une cellule
    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int( 1, 0),  // droite
        new Vector2Int(-1, 0),  // gauche
        new Vector2Int( 0, 1),  // haut
        new Vector2Int( 0,-1),  // bas
    };

    /// <summary>
    /// OnEnable est appelé quand le GameObject est activé.
    /// On l'utilise à la place de Start() car c'est plus fiable quand on instancie
    /// ou réactive un prefab dynamiquement (Start n'est appelé qu'une seule fois).
    /// </summary>
    private void OnEnable()
    {
        GenerateAndRender();
    }

    /// <summary>
    /// Méthode principale : génère le labyrinthe et l'affiche sur la tilemap.
    /// Les étapes sont :
    ///   1. Construire la grille initiale (tout en murs)
    ///   2. Creuser les passages avec DFS (Depth-First Search)
    ///   3. Ajouter des boucles pour avoir plusieurs chemins
    ///   4. Construire la grille walkable (pour le pathfinding A*)
    ///   5. Afficher le résultat sur la Tilemap Unity
    ///   6. Placer le joueur et l'ennemi aléatoirement sur des cases walkable
    /// </summary>
    [ContextMenu("Generate And Render")]
    public void GenerateAndRender()
    {
        // Vérification des références : on ne peut pas générer sans tilemap et tiles
        if (tilemap == null || floorTile == null || wallTile == null)
        {
            Debug.LogError("MazeGenerator2D: Assigne tilemap + floorTile + wallTile dans l'inspecteur.");
            return;
        }

        // Initialisation du générateur aléatoire (seed fixe ou aléatoire selon le paramètre)
        var rng = useRandomSeed ? new System.Random(Environment.TickCount) : new System.Random(seed);

        // Pipeline de génération du labyrinthe
        BuildInitialWallGrid();    // Étape 1 : grille de murs
        CarveMazeDFS(rng);         // Étape 2 : creuser avec DFS
        AddLoops(rng);             // Étape 3 : ouvrir des murs pour créer des boucles
        BuildWalkableGrid();       // Étape 4 : grille walkable pour A*
        RenderToTilemap();         // Étape 5 : affichage sur la tilemap
        PlacePlayerRandom(rng);    // Étape 6a : spawn joueur
        PlaceEnemyRandom(rng);     // Étape 6b : spawn ennemi
    }

    /// <summary>
    /// Construit la grille initiale où tout est un mur, sauf les centres des cellules.
    /// Structure de la grille de tiles :
    ///   - Les coordonnées impaires (1, 3, 5...) correspondent aux centres des cellules (= sols)
    ///   - Les coordonnées paires (0, 2, 4...) correspondent aux murs entre les cellules
    ///   - Exemple pour 3x3 cellules : grille de 7x7 tiles
    ///
    ///     W W W W W W W     (W = wall, F = floor/cell center)
    ///     W F W F W F W
    ///     W W W W W W W
    ///     W F W F W F W
    ///     W W W W W W W
    ///     W F W F W F W
    ///     W W W W W W W
    /// </summary>
    private void BuildInitialWallGrid()
    {
        // Calcul des dimensions de la grille de tiles
        // Formule : 2 * nbCellules + 1 (pour les murs bordures)
        gridW = cellsX * 2 + 1;
        gridH = cellsY * 2 + 1;

        isWall = new bool[gridW, gridH];

        // On met tout en mur par défaut
        for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                isWall[x, y] = true;

        // On « ouvre » le centre de chaque cellule (coordonnées impaires) pour en faire du sol
        for (int cx = 0; cx < cellsX; cx++)
        {
            for (int cy = 0; cy < cellsY; cy++)
            {
                Vector2Int t = CellToTile(cx, cy); // Conversion cellule -> tile
                isWall[t.x, t.y] = false;
            }
        }
    }

    /// <summary>
    /// Creuse le labyrinthe avec l'algorithme DFS (Depth-First Search) itératif.
    /// Principe :
    ///   1. On part d'une cellule aléatoire
    ///   2. On choisit un voisin non visité au hasard
    ///   3. On « casse » le mur entre la cellule courante et le voisin
    ///   4. On avance vers le voisin et on recommence
    ///   5. Si on est bloqué (tous les voisins visités), on revient en arrière (backtrack)
    /// Cela produit un labyrinthe parfait (un seul chemin entre deux points quelconques).
    /// </summary>
    private void CarveMazeDFS(System.Random rng)
    {
        bool[,] visited = new bool[cellsX, cellsY]; // Tableau pour savoir quelles cellules on a déjà visitées
        Stack<Vector2Int> stack = new Stack<Vector2Int>(); // Pile pour le backtracking (DFS itératif)

        // On commence à une cellule aléatoire
        Vector2Int current = new Vector2Int(rng.Next(cellsX), rng.Next(cellsY));
        visited[current.x, current.y] = true;
        stack.Push(current);

        // Boucle DFS : tant que la pile n'est pas vide
        while (stack.Count > 0)
        {
            current = stack.Peek(); // On regarde la cellule en haut de la pile (sans la retirer)

            // On collecte les voisins non visités (dans les 4 directions)
            List<Vector2Int> candidates = new List<Vector2Int>(4);
            foreach (var d in DIRS)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;
                if (nx < 0 || nx >= cellsX || ny < 0 || ny >= cellsY) continue; // Hors limites
                if (!visited[nx, ny]) candidates.Add(new Vector2Int(nx, ny));
            }

            // Si aucun voisin non visité -> on backtrack (on dépile)
            if (candidates.Count == 0)
            {
                stack.Pop();
                continue;
            }

            // On choisit un voisin au hasard parmi les non-visités
            Vector2Int next = candidates[rng.Next(candidates.Count)];

            // On casse le mur entre la cellule courante et le voisin choisi
            // Pour cela, on convertit en coordonnées tile et on prend la tile du milieu
            Vector2Int a = CellToTile(current.x, current.y);
            Vector2Int b = CellToTile(next.x, next.y);
            Vector2Int between = (a + b) / 2; // La tile entre les deux centres = le mur à casser

            isWall[between.x, between.y] = false; // On transforme le mur en passage

            // On marque le voisin comme visité et on avance
            visited[next.x, next.y] = true;
            stack.Push(next);
        }
    }

    /// <summary>
    /// Ajoute des boucles dans le labyrinthe en ouvrant un pourcentage de murs.
    /// Un labyrinthe DFS pur est un arbre couvrant (spanning tree) : il n'y a qu'un seul chemin
    /// entre deux points. En cassant des murs supplémentaires, on crée des cycles,
    /// ce qui donne plusieurs chemins possibles et rend le labyrinthe plus intéressant.
    /// </summary>
    private void AddLoops(System.Random rng)
    {
        if (loopPercent <= 0f) return; // Si 0%, pas de boucles à ajouter

        // On identifie tous les murs « candidats » : ceux qui séparent deux couloirs
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 1; x < gridW - 1; x++)
        {
            for (int y = 1; y < gridH - 1; y++)
            {
                if (!isWall[x, y]) continue; // On ne s'intéresse qu'aux murs

                // Mur vertical (entre 2 couloirs gauche/droite) : x pair, y impair
                // On vérifie que les tiles à gauche et à droite sont des sols
                if ((x % 2 == 0) && (y % 2 == 1))
                {
                    if (!isWall[x - 1, y] && !isWall[x + 1, y])
                        candidates.Add(new Vector2Int(x, y));
                }
                // Mur horizontal (entre 2 couloirs haut/bas) : x impair, y pair
                // On vérifie que les tiles au-dessus et en dessous sont des sols
                else if ((x % 2 == 1) && (y % 2 == 0))
                {
                    if (!isWall[x, y - 1] && !isWall[x, y + 1])
                        candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        // Mélange aléatoire des candidats avec l'algorithme de Fisher-Yates
        // (pour ouvrir les murs dans un ordre aléatoire, pas toujours les mêmes)
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        // On ouvre un pourcentage des murs candidats
        int openings = Mathf.RoundToInt(candidates.Count * loopPercent);
        openings = Mathf.Clamp(openings, 0, candidates.Count);

        for (int i = 0; i < openings; i++)
        {
            Vector2Int p = candidates[i];
            isWall[p.x, p.y] = false; // On transforme le mur en passage
        }
    }

    /// <summary>
    /// Construit la grille walkable à partir de la grille isWall.
    /// C'est simplement l'inverse : walkable[x,y] = !isWall[x,y].
    /// On sépare les deux grilles car isWall est utilisée pendant la génération,
    /// et walkable est exposée publiquement pour le pathfinding (A*).
    /// </summary>
    private void BuildWalkableGrid()
    {
        walkable = new bool[gridW, gridH];
        for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                walkable[x, y] = !isWall[x, y];
    }

    /// <summary>
    /// Affiche le labyrinthe sur la Tilemap Unity.
    /// On parcourt toute la grille et on place un wallTile ou un floorTile
    /// selon que la case est un mur ou un sol.
    /// </summary>
    private void RenderToTilemap()
    {
        tilemap.ClearAllTiles(); // On efface les tiles précédentes

        for (int x = 0; x < gridW; x++)
        {
            for (int y = 0; y < gridH; y++)
            {
                TileBase t = isWall[x, y] ? wallTile : floorTile; // Mur ou sol selon la grille
                tilemap.SetTile(new Vector3Int(x, y, 0), t);       // On place la tile
            }
        }

        tilemap.RefreshAllTiles(); // On force le rafraîchissement visuel de la tilemap
    }

    /// <summary>
    /// Place le joueur au milieu-gauche du labyrinthe (méthode alternative, non utilisée actuellement).
    /// On cible le premier couloir vertical (x=1) à mi-hauteur.
    /// Si la case ciblée est un mur, on cherche la case walkable la plus proche
    /// en alternant vers le bas et vers le haut.
    /// </summary>
    private void PlacePlayerMiddleLeft()
    {
        if (!player) return;

        // On cible le premier couloir (x=1, car x=0 est le mur bordure)
        int px = 1;
        // On calcule la hauteur du milieu en coordonnées tile (doit être impair pour tomber sur un centre de cellule)
        int py = (cellsY / 2) * 2 + 1;

        py = Mathf.Clamp(py, 1, gridH - 2); // On reste dans les limites de la grille

        // Sécurité : si la case cible est un mur, on cherche un sol en alternant bas/haut
        if (!IsWalkable(px, py))
        {
            bool found = false;

            for (int i = 1; i < gridH; i++)
            {
                int yDown = py - i;
                if (yDown >= 1 && IsWalkable(px, yDown)) { py = yDown; found = true; break; }

                int yUp = py + i;
                if (yUp <= gridH - 2 && IsWalkable(px, yUp)) { py = yUp; found = true; break; }
            }

            if (!found)
            {
                Debug.LogWarning("MazeGenerator2D: aucun spawn walkable trouvé pour le joueur.");
                return;
            }
        }

        // On convertit la position tile en position world et on place le joueur
        Vector3Int cell = new Vector3Int(px, py, 0);
        Vector3 worldPos = tilemap.GetCellCenterWorld(cell) + playerOffset;

        // On conserve le Z du joueur pour éviter les problèmes de tri des sprites
        worldPos.z = player.position.z;

        player.position = worldPos;
    }

    /// <summary>
    /// Convertit les coordonnées d'une cellule logique (cx, cy) en coordonnées de tile.
    /// Les centres de cellules sont toujours aux coordonnées impaires (2*c + 1).
    /// Exemple : cellule (0,0) -> tile (1,1), cellule (1,0) -> tile (3,1), etc.
    /// </summary>
    private Vector2Int CellToTile(int cx, int cy)
    {
        return new Vector2Int(2 * cx + 1, 2 * cy + 1);
    }

    /// <summary>
    /// Vérifie si une case de la grille de tiles est traversable (pas un mur).
    /// Utilisée par le pathfinding A* et par le placement des entités.
    /// Retourne false si la grille n'est pas encore générée ou si les coordonnées sont hors limites.
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        if (walkable == null) return false;
        if (x < 0 || x >= gridW || y < 0 || y >= gridH) return false;
        return walkable[x, y];
    }

    /// <summary>
    /// Retourne la taille de la grille de tiles (en nombre de tiles).
    /// Utilisée par A* pour connaître les limites de la grille.
    /// </summary>
    public Vector2Int GetTileGridSize() => new Vector2Int(gridW, gridH);

    /// <summary>
    /// Place le joueur sur une case walkable aléatoire du labyrinthe.
    /// On tire des coordonnées au hasard et on vérifie qu'elles sont walkable.
    /// On limite le nombre de tentatives (MAX_TRIES) pour éviter une boucle infinie
    /// dans le cas improbable où il n'y aurait presque aucune case walkable.
    /// </summary>
    private void PlacePlayerRandom(System.Random rng)
    {
        if (!player) return;

        if (floorTilemap == null)
        {
            Debug.LogError("MazeGenerator2D: assigne floorTilemap (Tilemap_Floor) dans l'inspector.");
            return;
        }

        const int MAX_TRIES = 500; // Nombre max de tentatives pour trouver un sol

        for (int tries = 0; tries < MAX_TRIES; tries++)
        {
            // On évite les bords de la grille (index 0 et max sont toujours des murs)
            int tx = rng.Next(1, gridW - 1);
            int ty = rng.Next(1, gridH - 1);

            if (!walkable[tx, ty]) continue; // C'est un mur, on retente

            // On convertit la position tile en position world via la Tilemap
            Vector3Int cell = new Vector3Int(tx, ty, 0);
            Vector3 world = floorTilemap.GetCellCenterWorld(cell);

            // Jitter optionnel : petit décalage aléatoire pour ne pas spawner pile au centre
            // (commenté pour l'instant, décommenter si besoin)
            Vector3 jitter = Vector3.zero;
            // jitter = new Vector3(((float)rng.NextDouble() - 0.5f) * 0.1f, ((float)rng.NextDouble() - 0.5f) * 0.1f, 0f);

            Vector3 p = world + playerOffset + jitter;
            p.z = player.position.z; // garde son Z actuel (ou mets 0)
            player.position = p;
            return; // Spawn réussi, on sort
        }

        Debug.LogWarning("MazeGenerator2D: impossible de trouver une case walkable pour spawn (MAX_TRIES dépassé).");
    }

    /// <summary>
    /// Place l'ennemi sur une case walkable aléatoire du labyrinthe.
    /// Même logique que PlacePlayerRandom : on tire des coordonnées au hasard
    /// jusqu'à trouver une case walkable (max 500 tentatives).
    /// Note : on ne vérifie pas que l'ennemi ne spawne pas sur le joueur
    /// (amélioration possible pour plus tard).
    /// </summary>
    private void PlaceEnemyRandom(System.Random rng)
    {
        if (!enemy || floorTilemap == null) return;

        for (int tries = 0; tries < 500; tries++)
        {
            int x = rng.Next(1, gridW - 1);
            int y = rng.Next(1, gridH - 1);

            if (!walkable[x, y]) continue; // Mur, on retente

            // Conversion tile -> world et placement de l'ennemi
            Vector3Int cell = new Vector3Int(x, y, 0);
            Vector3 pos = floorTilemap.GetCellCenterWorld(cell);
            pos.z = enemy.position.z; // On conserve le Z d'origine pour le tri des sprites
            enemy.position = pos;
            return;
        }
    }


}