using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeGridProvider2D : MonoBehaviour
{
    [SerializeField] private MazeGenerator2D maze;
    [SerializeField] private Tilemap floorTilemap;

    public bool IsReady => maze != null && floorTilemap != null;

    public Vector3Int WorldToCell(Vector3 world)
        => (floorTilemap != null) ? floorTilemap.WorldToCell(world) : Vector3Int.zero;

    public Vector3 CellCenterWorld(Vector3Int cell)
        => (floorTilemap != null) ? floorTilemap.GetCellCenterWorld(cell) : Vector3.zero;

    public Vector2Int GridSize
        => (maze != null) ? maze.GetTileGridSize() : Vector2Int.zero;

    public bool IsWalkable(Vector3Int cell)
        => (maze != null) && maze.IsWalkable(cell.x, cell.y);

    public float WorldScale
        => (floorTilemap != null) ? floorTilemap.transform.lossyScale.x : 1f;

    public float CellSizeWorld
    {
        get
        {
            if (floorTilemap == null) return 1f;
            float s = Mathf.Max(0.0001f, floorTilemap.transform.lossyScale.x);
            return floorTilemap.cellSize.x * s;
        }
    }
}