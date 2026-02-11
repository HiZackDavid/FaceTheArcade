using UnityEngine;

public class PlayerMiniGameMovement : MonoBehaviour
{
    private PlayerControls playerControls;

    // Rotate player graphics in mouse direction
    // Reference to the main camera, can be assigned in the Inspector
    public Camera camera;
    private Vector2 mousePosition;

    public float speed;
    private Rigidbody2D rb;
    private Vector2 movementInput;

    // Player graphics GameObject
    public Transform playerGraphics;

    // Dropdown to select the current mini-game mode
    public enum GameMode {NoMode, MiniGame1, MiniGame2, MiniGame3 } // 0: NoMode 1: Pokemon style, 2: Top-down shooter, 3: Top-down adventure
    public GameMode currentGameMode;



    void Awake()
    {
        playerControls = new PlayerControls();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Enable the appropriate control scheme based on the current game mode
        if (currentGameMode == GameMode.NoMode)
        {
            Debug.LogWarning("Current game mode is set to NoMode. Please set it to a valid mini-game mode.");
        }
        else
        {
            playerControls.MiniGameMovement.Enable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        movementInput = playerControls.MiniGameMovement.Move.ReadValue<Vector2>().normalized;

        if (currentGameMode == GameMode.MiniGame2)
        {
            mousePosition = playerControls.MiniGameMovement.MousePosition.ReadValue<Vector2>();
        }
    }

    void FixedUpdate()
    {
        // For Pokemon style, restrain diagonal movement
        //Debug.Log("Move in X : " + movementInput.x);
        //Debug.Log("Move in Y : " + movementInput.y);

        // Convert to world movement based on player orientation
        Vector2 worldMovement = (transform.right * movementInput.x) + (transform.up * movementInput.y);
        rb.linearVelocity = worldMovement * speed;

        if (currentGameMode == GameMode.MiniGame1) // Pokemon style movement
        {
            // Animation for Pokemon style

        }
        else if(currentGameMode == GameMode.MiniGame2) // Top-down shooter style movement
        {
            // Convert mouse position to world position
            Vector3 worldMousePostion = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));

            // Direction player graphics will face towards (Arrivée - Départ)
            Vector2 rotateDirection = (worldMousePostion - playerGraphics.position).normalized;

            // Calculate angle in degrees to where the player should face
            float angle = Mathf.Atan2(rotateDirection.y, rotateDirection.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees, Y axis will look at mouse
            playerGraphics.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else if(currentGameMode == GameMode.MiniGame3) // Top-down adventure style movement
        {
            Debug.Log("Movement : " + worldMovement);
            Debug.Log("sqrMagnitude : " + worldMovement.sqrMagnitude);
            if (worldMovement.sqrMagnitude >= 0.01f)
            {
                playerGraphics.up = worldMovement; // Rotate graphics to face movement direction
            }
        }
        
    }

    //public void EnableMiniGameControls(GameMode gameMode)
    //{
    //    switch (gameMode)
    //    {
    //        case GameMode.MiniGame1:
    //            playerControls.MiniGame1.Enable();
    //            break;
    //    }
    //}

    private void OnDisable()
    {
        playerControls.Disable();
    }

}
