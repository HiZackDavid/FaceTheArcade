using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaserShootSpace : MonoBehaviour
{
    public LaserWeapon2D weapon;

    // direction fiable (input), 4 directions
    private PlayerControls controls;
    private Vector2 lastDir = Vector2.right;

    void Awake()
    {
        controls = new PlayerControls();
        controls.MiniGameMovement.Enable();
    }

    void OnDestroy()
    {
        controls.Disable();
    }

    void Update()
    {
        Vector2 move = controls.MiniGameMovement.Move.ReadValue<Vector2>();

        // lock 4 directions
        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            move = new Vector2(Mathf.Sign(move.x), 0f);
        else if (Mathf.Abs(move.y) > 0f)
            move = new Vector2(0f, Mathf.Sign(move.y));

        if (move.sqrMagnitude > 0.1f)
            lastDir = move.normalized;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && weapon != null)
        {
            weapon.Fire(lastDir, gameObject);
        }
    }
}