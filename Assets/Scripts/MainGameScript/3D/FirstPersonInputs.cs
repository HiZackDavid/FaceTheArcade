using UnityEngine;
using UnityEngine.InputSystem;

/**
 * This class serves as a bridge between the Input System and a control logic (e.g. Player movement).
 * It's main use is to store the current state of commands (e.g. Is the player pressing Jump right now?)
 * so other scripts can read them.
 * 
 * Source of inspiration: https://assetstore.unity.com/packages/essentials/starter-assets-firstperson-updates-in-new-charactercontroller-pa-196525#content
 */
public class FirstPersonInputs : MonoBehaviour
{
    [Header("Player Input Values")]
    public Vector2 move;
    public bool sprint;
    
    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;

    public void OnMove(InputValue value) => MoveInput(value.Get<Vector2>());
    public void OnSprint(InputValue value) => SprintInput(value.isPressed);
    public void OnApplicationFocus(bool hasFocus) =>  SetCursorState(hasFocus);

    public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;
    public void SprintInput(bool newSprintState) => sprint = newSprintState;
    private void SetCursorState(bool newState)
    {
        bool shouldLock = newState && cursorLocked;
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }
}
