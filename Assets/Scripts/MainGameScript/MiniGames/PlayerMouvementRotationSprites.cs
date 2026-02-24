using UnityEngine;

public class PlayerMouvementRotationSprites : PlayerMiniGameMovement, IPlayerRotationStrategy
{

    Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
    }


    protected override void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
        Debug.Log($"movementInput = {movementInput}");
    }

    protected override void MovePlayer()
    {      
        // Restraint diagonal mouvement and prioritize horizontal movement over vertical
        if (Mathf.Abs(movementInput.x) == Mathf.Abs(movementInput.y) && movementInput.x != 0 && movementInput.y != 0)
        {
            movementInput.x = 1 * Mathf.Sign(movementInput.x);
            movementInput.y = 0;
        }
        base.MovePlayer();
    }

    public void RotatePlayer()
    {
        // Animation for Pokemon style
        animator.SetFloat("Horizontal", movementInput.x);
        animator.SetFloat("Vertical", movementInput.y);
       

        animator.SetFloat("Speed", movementInput.sqrMagnitude);

    }
}
