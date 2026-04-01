using UnityEngine;

public class ControllerManager : MonoBehaviour
{

    public static ControllerManager instance;

    GameObject player;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ActivateController()
    {

        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        controller.enabled = true;
        player.SetActive(true);
        SetCursorState(true);
    }

    public void DeactivateController()
    {
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        controller.enabled = false;

        player.SetActive(false);
        SetCursorState(false);
    }

    private void SetCursorState(bool newState)
    {
        bool shouldLock = newState;
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }

}
