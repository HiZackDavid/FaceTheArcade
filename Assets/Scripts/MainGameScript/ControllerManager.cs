using UnityEngine;

public class ControllerManager : MonoBehaviour
{

    public static ControllerManager instance;


    // private Controller currentController; <- On peu créer un controller géneral 

    // On peu ajouter d'autres controllers ici

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
        // currentController.setActive(true);

        player.SetActive(true);
        SetCursorState(true);
    }

    public void DeactivateController()
    {
        // currentController.setActive(false);

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
