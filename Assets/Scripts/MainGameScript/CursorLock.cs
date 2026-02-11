using UnityEngine;

/**
 * This script has a simple purpose: Lock the cursor at the center of the screen and make it disappear.
 * Use case(s):
 * - This script is designed to be a component of a camera.
 * Good to know:
 * - While in dev mode, press the game scene to lock the cursor to it, if it's not already locked.
 * - While in dev mode, press escape (ESC) to unlock the cursor from the game scene.
 */
public class CursorLock : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
