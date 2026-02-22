using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject player;

    [SerializeField] private Timer dayTimer;

    [SerializeField] private int cheatCodeAmount = 0;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        } else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

    }

    public int GetCheatCodes() => cheatCodeAmount;

    private void Start()
    {
        ControllerManager.instance.DeactivateController();
        CameraManager.instance.SetStartingCamera();
        UIManager.instace.ShowMainMenu();
    }

    public void StartGame()
    {
        CameraManager.instance.SwitchToPrimaryCamera();
        UIManager.instace.ShowMainHUD();
        ControllerManager.instance.ActivateController();

        dayTimer.StartTimer();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

}
