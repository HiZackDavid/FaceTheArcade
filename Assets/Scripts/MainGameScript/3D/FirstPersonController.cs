using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
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
    
    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15f;
    
    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float jumpTimeout = 0.1f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float fallTimeout = 0.15f;
    
    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool grounded = true;
    [Tooltip("Useful for rough ground")]
    public float groundedOffset = 0.7f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float groundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;
    
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject cinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -90.0f;
    
    float _cinemachineTargetPitch;

    float _speed;
    float _verticalVelocity;
    float _rotationVelocity;
    float _terminalVelocity = 53.0f;
    
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private float _cinemachineTargetYaw;

    private PlayerInput _playerInput;
    CharacterController _controller;
    FirstPersonInputs _input;
    InteractionArea _interactionArea;

    
    const float Threshold = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<FirstPersonInputs>();
        _playerInput = GetComponent<PlayerInput>();

        _interactionArea = transform.GetComponentsInChildren<InteractionArea>()[0];


        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;

        _cinemachineTargetYaw = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();
        JumpWithGravity();
        Interact();
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

    private void JumpWithGravity()
    {
        if (grounded)
        {
            _fallTimeoutDelta = fallTimeout;

            if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;
            if (_input.jump && _jumpTimeoutDelta <= 0.0f) _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout;
            
            if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
            
            _input.jump = false;
        }
        
        if (_verticalVelocity < _terminalVelocity) _verticalVelocity += gravity * Time.deltaTime;
    }

    void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= Threshold)
        {

            float mouseX = _input.look.x * mouseSensitivity;
            float mouseY = _input.look.y * mouseSensitivity;

            _cinemachineTargetPitch -= mouseY; 
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
            
            _cinemachineTargetYaw += mouseX;
            
            cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);
            
        }
    }

    void Interact() 
    {
        if (_input.interact && _interactionArea.canInteract()) 
        {
            _input.interact = false;

            GameObject obj = _interactionArea.getCurObject();

            ArcadeMachineController arcade = obj.GetComponentInParent<ArcadeMachineController>();
            if (arcade != null)
            {
                arcade.Interact();
                return;
            }
            
            CinemachineCamera cam = obj.GetComponentInChildren<CinemachineCamera>();
            if (cam != null)
            {
                CameraManager.instance.SwitchToCamera(cam, false);
            }
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
