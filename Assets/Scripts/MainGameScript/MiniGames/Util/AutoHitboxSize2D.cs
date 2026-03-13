using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AutoHitboxSize2D : MonoBehaviour
{
    public MazeGridProvider2D grid;
    [Range(0.1f, 0.9f)] public float radiusInTiles = 0.35f;

    void Start()
    {
        var col = GetComponent<CircleCollider2D>();
        float cell = (grid != null && grid.IsReady) ? grid.CellSizeWorld : 1f;
        col.isTrigger = true;
        col.radius = radiusInTiles * cell;
    }
}