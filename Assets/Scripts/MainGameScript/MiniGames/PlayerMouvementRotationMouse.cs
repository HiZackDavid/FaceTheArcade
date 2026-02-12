using UnityEngine;

public class PlayerMouvementRotationMouse : PlayerMiniGameMovement, IPlayerRotationStrategy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        mousePosition = playerControls.MiniGameMovement.MousePosition.ReadValue<Vector2>();       
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        MovePlayer();
        RotatePlayer();
    }

    public void RotatePlayer()
    {
        // Convert mouse position to world position
        Vector3 worldMousePostion = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, camera.nearClipPlane));

        // Direction player graphics will face towards (Arrivée - Départ)
        Vector2 rotateDirection = (worldMousePostion - playerGraphics.position).normalized;

        // Calculate angle in degrees to where the player should face
        float angle = Mathf.Atan2(rotateDirection.y, rotateDirection.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees, Y axis will look at mouse
        playerGraphics.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        
    }
}
