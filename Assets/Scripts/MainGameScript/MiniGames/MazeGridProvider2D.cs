using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Couche d'abstraction entre le labyrinthe (MazeGenerator2D) et les systèmes
/// qui ont besoin d'interroger la grille (pathfinding A*, IA ennemie, etc.).
/// Ce script agit comme un « fournisseur de grille » : il expose les méthodes
/// de conversion de coordonnées et de vérification de walkabilité
/// sans que les autres scripts aient besoin de connaître les détails internes du générateur.
/// C'est un bon exemple de principe d'encapsulation / séparation des responsabilités.
/// </summary>
public class MazeGridProvider2D : MonoBehaviour
{
    // Références aux composants nécessaires (à assigner dans l'Inspector Unity)
    [SerializeField] private MazeGenerator2D maze;      // Le générateur de labyrinthe (contient la grille walkable)
    [SerializeField] private Tilemap floorTilemap;      // La Tilemap du sol (pour les conversions world <-> cell)

    /// <summary>
    /// Indique si le provider est prêt à être utilisé (les deux références sont assignées).
    /// Les scripts qui dépendent de la grille vérifient IsReady avant d'appeler les autres méthodes.
    /// </summary>
    public bool IsReady => maze != null && floorTilemap != null;

    /// <summary>
    /// Convertit une position world (Vector3) en coordonnées de cellule de la grille (Vector3Int).
    /// Utilise la Tilemap pour faire la conversion (tient compte de la taille et position des tiles).
    /// </summary>
    public Vector3Int WorldToCell(Vector3 world) => floorTilemap.WorldToCell(world);

    /// <summary>
    /// Retourne la position world du centre d'une cellule donnée.
    /// Utilisée par le pathfinding pour savoir vers quelle position world se déplacer.
    /// </summary>
    public Vector3 CellCenterWorld(Vector3Int cell) => floorTilemap.GetCellCenterWorld(cell);

    /// <summary>
    /// Retourne la taille de la grille de tiles (largeur x hauteur).
    /// Utilisée par A* pour connaître les limites de la grille.
    /// </summary>
    public Vector2Int GridSize => maze.GetTileGridSize();

    /// <summary>
    /// Vérifie si une cellule est traversable (pas un mur).
    /// Délègue au MazeGenerator2D qui maintient la grille walkable.
    /// </summary>
    public bool IsWalkable(Vector3Int cell) => maze.IsWalkable(cell.x, cell.y);
}