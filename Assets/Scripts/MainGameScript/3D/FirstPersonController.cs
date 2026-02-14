using UnityEngine;
using UnityEngine.Serialization;

public class FirstPersonController : MonoBehaviour
{
    [FormerlySerializedAs("MoveSpeed")]
    [Header("Player")]
    [Tooltip("Movement speed of the player in m/s")]
    public float moveSpeed = 5f;
    [Tooltip("Sprint speed of the player in m/s")]
    public float sprintSpeed = 15f;

    float _speed;
    float _verticalVelocity;

    CharacterController _controller;
    FirstPersonInputs _input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<FirstPersonInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        float targetSpeed = _input.sprint ? sprintSpeed : moveSpeed;
        
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;
        
        _speed = targetSpeed;
        
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }
        
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }
}
