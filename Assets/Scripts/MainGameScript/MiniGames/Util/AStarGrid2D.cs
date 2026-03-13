using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe utilitaire statique qui implémente l'algorithme A* sur une grille 2D.
/// Elle est utilisée pour calculer le chemin le plus court entre deux cellules
/// dans le labyrinthe, en tenant compte des murs (cellules non-walkable).
/// La classe est statique car elle ne dépend d'aucun état interne : on lui passe
/// tout ce dont elle a besoin en paramètres (start, goal, grille, etc.).
/// </summary>
public static class AStarGrid2D
{
    // Les 4 directions cardinales : droite, gauche, haut, bas
    // On les stocke en static readonly pour ne pas les recréer à chaque appel
    // (petite optimisation mémoire, bonne pratique en C#)
    private static readonly Vector3Int[] Neigh4 =
    {
        new Vector3Int(1,0,0), new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0), new Vector3Int(0,-1,0)
    };

    /// <summary>
    /// Trouve le chemin le plus court entre deux cellules de la grille avec l'algorithme A*.
    /// </summary>
    /// <param name="start">Cellule de départ (coordonnées grille)</param>
    /// <param name="goal">Cellule d'arrivée (coordonnées grille)</param>
    /// <param name="isWalkable">Prédicat qui indique si une cellule est traversable (pas un mur)</param>
    /// <param name="gridSize">Dimensions de la grille (largeur x hauteur)</param>
    /// <returns>Liste ordonnée des cellules du chemin (start -> goal), ou null si aucun chemin</returns>
    public static List<Vector3Int> FindPath(
        Vector3Int start,
        Vector3Int goal,
        System.Predicate<Vector3Int> isWalkable,
        Vector2Int gridSize)
    {
        // Fonction locale : vérifie qu'une cellule est bien dans les limites de la grille
        bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < gridSize.x && c.y < gridSize.y;

        // Vérifications préliminaires : si start ou goal est hors limites ou non-walkable,
        // on retourne null directement (pas la peine de lancer A*)
        if (!InBounds(start) || !InBounds(goal)) return null;
        if (!isWalkable(start) || !isWalkable(goal)) return null;

        // Heuristique : distance de Manhattan (somme des écarts absolus en x et y)
        // C'est l'heuristique optimale pour une grille 4-connexe (pas de diagonales)
        int Heur(Vector3Int a, Vector3Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        // --- Structures de données de A* ---
        // Open set : ensemble des noeuds à explorer (on commence avec start)
        var open = new List<Vector3Int> { start };
        // cameFrom : pour chaque noeud, stocke son prédécesseur (permet de reconstruire le chemin)
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        // gScore[n] : coût réel du chemin le plus court connu depuis start jusqu'à n
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };
        // fScore[n] = gScore[n] + heuristique(n, goal) : estimation du coût total
        var fScore = new Dictionary<Vector3Int, int> { [start] = Heur(start, goal) };

        // Boucle principale de A* : tant qu'il reste des noeuds à explorer
        while (open.Count > 0)
        {
            // On cherche le noeud avec le fScore le plus bas dans l'open set
            // (recherche linéaire en O(n), suffisante pour nos tailles de labyrinthe)
            int bestIdx = 0;
            int bestF = fScore[open[0]];
            for (int i = 1; i < open.Count; i++)
            {
                int f = fScore[open[i]];
                if (f < bestF) { bestF = f; bestIdx = i; }
            }

            // On extrait le meilleur noeud de l'open set
            var current = open[bestIdx];
            open.RemoveAt(bestIdx);

            // Si on a atteint le goal, on reconstruit le chemin et on le retourne
            if (current == goal)
                return Reconstruct(cameFrom, current);

            // On explore les 4 voisins du noeud courant
            foreach (var d in Neigh4)
            {
                var next = current + d;
                // On ignore les voisins hors grille ou non-walkable (murs)
                if (!InBounds(next) || !isWalkable(next)) continue;

                // Le coût pour aller au voisin est gScore[current] + 1 (chaque pas coûte 1)
                int tentativeG = gScore[current] + 1;

                // Si on a trouvé un chemin plus court vers ce voisin, on met à jour
                if (!gScore.TryGetValue(next, out int gOld) || tentativeG < gOld)
                {
                    cameFrom[next] = current;                       // On enregistre d'où on vient
                    gScore[next] = tentativeG;                      // Mise à jour du coût réel
                    fScore[next] = tentativeG + Heur(next, goal);   // Mise à jour du coût estimé
                    if (!open.Contains(next)) open.Add(next);       // On ajoute à l'open set si pas déjà dedans
                }
            }
        }

        // Si on sort de la boucle, c'est qu'aucun chemin n'existe (goal inaccessible)
        return null;
    }

    /// <summary>
    /// Reconstruit le chemin en remontant le dictionnaire cameFrom depuis le goal.
    /// On part du goal et on suit les prédécesseurs jusqu'au start (qui n'a pas de prédécesseur).
    /// Le chemin est construit à l'envers puis inversé pour obtenir l'ordre start -> goal.
    /// </summary>
    private static List<Vector3Int> Reconstruct(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var path = new List<Vector3Int> { current }; // On commence par le goal
        // On remonte la chaîne de prédécesseurs
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }
        path.Reverse(); // Inversion : on veut start -> goal, pas goal -> start
        return path;
    }
}