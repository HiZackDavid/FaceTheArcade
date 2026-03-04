using System;
using UnityEngine;

public class EnemySurvivalMouvementScript : MonoBehaviour
{
    private Rigidbody2D _rb;
    private float _speed;
    private Transform _player;

    public float Speed
    {
        set => _speed = value;
    }
    
    public  Transform Player
    {
        set => _player = value;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        MovePlayer();
        RotateEnemy();
    }
    
    private void MovePlayer()
    {
        _rb.linearVelocity = _speed * transform.up;
    }
    
    private void RotateEnemy()
    {
        // Direction enemy will face towards (Pt arrival - Pt departure)
        Vector2 rotateDirection = (_player.position - transform.position).normalized;

        // Calculate angle in degrees to where the player should face
        float angle = Mathf.Atan2(rotateDirection.y, rotateDirection.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees, Y axis will look at player
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    
}
