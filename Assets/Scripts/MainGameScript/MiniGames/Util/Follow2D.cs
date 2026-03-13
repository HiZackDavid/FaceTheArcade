using UnityEngine;

public class Follow2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offsetXY = Vector2.zero;
    public float fixedZ = -10f;
    public float smooth = 0f; // 0 = instant

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = new Vector3(
            target.position.x + offsetXY.x,
            target.position.y + offsetXY.y,
            fixedZ
        );

        if (smooth <= 0f) transform.position = desired;
        else transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
    }
}