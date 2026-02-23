using UnityEngine;

public abstract class PlayerMiniGameMovement : MonoBehaviour
{
    protected PlayerControls playerControls;

    // --- Compat (tes scripts s'en servent) ---
    protected Vector2 worldMovement;     // direction finale utilisée pour bouger / orienter
    protected Vector2 mousePosition;     // position souris (world si camera assignée)

    [Header("References")]
    public Camera camera;                // optionnel
    public Transform playerGraphics;     // optionnel

    [Header("Movement")]
    public float speed = 5f;
    [Range(0f, 0.5f)] public float deadzone = 0.2f;
    public bool lockTo4Directions = true;

    protected Rigidbody2D rb;
    protected Vector2 movementInput;

    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Top-down 2D propre
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = 0f;

        playerControls.MiniGameMovement.Enable();
    }

    protected virtual void Update()
    {
        // Pas de normalized ici (garde la force du stick)
        movementInput = playerControls.MiniGameMovement.Move.ReadValue<Vector2>().normalized;
        movementInput.x *= -1.0f; // Inversion de l'axe horizontal pour correspondre à la direction du stick

        if (Mathf.Abs(movementInput.x) > 0.1f)
            Debug.Log($"MoveX = {movementInput.x}");

        // mousePosition pour tes scripts Mouse rotation
        if (camera != null)
            mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        else
            mousePosition = Input.mousePosition; // fallback
    }

    protected virtual void FixedUpdate()
    {
        MovePlayer();
    }

    protected virtual void MovePlayer()
    {
        Vector2 input = movementInput;

        if (input.sqrMagnitude < deadzone * deadzone)
        {
            worldMovement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        if (lockTo4Directions)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                input = new Vector2(Mathf.Sign(input.x), 0f);
            else
                input = new Vector2(0f, Mathf.Sign(input.y));
        }

        worldMovement = input;
        rb.linearVelocity = worldMovement * speed;
    }

    protected virtual void OnDisable()
    {
        playerControls.Disable();
    }
}