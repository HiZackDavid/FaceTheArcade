using System.Security.Cryptography;
using UnityEngine;

public class PlayerMouvementRotationDirection : PlayerMiniGameMovement, IPlayerRotationStrategy
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
        
        //Debug.Log("Movement : " + worldMovement);
        //Debug.Log("sqrMagnitude : " + worldMovement.sqrMagnitude);
        if (worldMovement.sqrMagnitude >= 0.01f)
        {
            playerGraphics.up = worldMovement; // Rotate graphics to face movement direction
        }
        
    }
}
