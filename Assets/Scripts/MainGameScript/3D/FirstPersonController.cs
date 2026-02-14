using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FirstPersonController : MonoBehaviour
{
    [FormerlySerializedAs("MoveSpeed")]
    [Header("Player")]
    [Tooltip("Movement speed of the player in m/s")]
    public float moveSpeed = 5f;
    [Tooltip("Sprint speed of the player in m/s")]
    public float sprintSpeed = 15f;
    [Tooltip("Rotation speed of the character")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;
    
    float _cinemachineTargetPitch;

    float _speed;
    float _verticalVelocity;
    float _rotationVelocity;

    private PlayerInput _playerInput;
    CharacterController _controller;
    FirstPersonInputs _input;
    
    const float _threshold = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<FirstPersonInputs>();
        _playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
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

    void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            float mouseX = _input.look.x * mouseSensitivity;
            float mouseY = _input.look.y * mouseSensitivity;
            
            _cinemachineTargetPitch += mouseY;
            _rotationVelocity = mouseX;
            
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
            
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
