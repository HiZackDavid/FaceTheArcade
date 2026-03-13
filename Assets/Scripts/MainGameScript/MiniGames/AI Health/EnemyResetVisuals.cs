using UnityEngine;

public class EnemyResetVisuals : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform laserOrigin;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (laserOrigin == null)
            laserOrigin = transform;

        ResetLaser();
    }

    public void ResetLaser()
    {
        if (lineRenderer == null) return;

        Vector3 p = laserOrigin != null ? laserOrigin.position : transform.position;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, p);
        lineRenderer.SetPosition(1, p);
    }

    private void OnDisable()
    {
        ResetLaser();
    }

    private void OnEnable()
    {
        ResetLaser();
    }
}