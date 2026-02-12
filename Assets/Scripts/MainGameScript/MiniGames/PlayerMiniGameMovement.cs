using UnityEngine;

public abstract class PlayerMiniGameMovement : MonoBehaviour
{
    protected PlayerControls playerControls;
    protected Vector2 worldMovement;

    // Rotate player graphics in mouse direction
    // Reference to the main camera, can be assigned in the Inspector
    public Camera camera;
    protected Vector2 mousePosition;

    // Player graphics GameObject
    public Transform playerGraphics;

    public float speed;
    private Rigidbody2D rb;
    private Vector2 movementInput;


    protected virtual void Awake()
    {
        playerControls = new PlayerControls();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Enable the control scheme    
        playerControls.MiniGameMovement.Enable();
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        movementInput = playerControls.MiniGameMovement.Move.ReadValue<Vector2>().normalized;
    }

    protected virtual void FixedUpdate()
    {
        //MovePlayer();
    }

    protected void MovePlayer()
    {
        // Convert to world movement based on player orientation
        worldMovement = (transform.right * movementInput.x) + (transform.up * movementInput.y);
        rb.linearVelocity = worldMovement * speed;
    }


    protected virtual void OnDisable()
    {
        playerControls.Disable();
    }

}
