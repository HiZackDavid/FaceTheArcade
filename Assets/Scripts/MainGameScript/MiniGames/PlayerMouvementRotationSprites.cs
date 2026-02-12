using UnityEngine;

public class PlayerMouvementRotationSprites : PlayerMiniGameMovement, IPlayerRotationStrategy
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
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        MovePlayer();
        RotatePlayer();
    }

    public void RotatePlayer()
    {
        // Animation for Pokemon style

        
    }
}
