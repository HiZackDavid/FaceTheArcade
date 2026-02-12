using System.Security.Cryptography;
using UnityEngine;

public class PlayerMouvementRotationDirection : PlayerMiniGameMovement, IPlayerRotationStrategy
{
    

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
