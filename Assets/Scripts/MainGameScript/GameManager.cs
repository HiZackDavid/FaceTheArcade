using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Timer dayTimer;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject player;

    [SerializeField] private int score = 0;


    public UnityEvent startMachines;
    public UnityEvent stopMachines;

    public UnityEvent fadeIn;

    private bool wasPaused = false;

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

    public int GetScore() => score;
    public void IncScoreBy(int inc)
    {
        score += inc;
        UIManager.instace.OnScoreUpdate(score);
    }

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

        if (wasPaused)
        {
            wasPaused = false;
            dayTimer.ResumeTimer();
        }
        else
        {
            dayTimer.StartTimer();
        }

        startMachines.Invoke();
    }

    public void PauseGame(bool isReset = false)
    {

        dayTimer.StopTimer();
        stopMachines.Invoke();
        ControllerManager.instance.DeactivateController();

        if (!isReset)
            wasPaused = true;

        StartCoroutine(waitForShowMenu());

    }

    IEnumerator waitForShowMenu()
    {
        yield return new WaitUntil(() => !CameraManager.instance.isInTransition());
        UIManager.instace.ShowMainMenu();
        ControllerManager.instance.DeactivateController();
    }

    public void StopGame()
    {
        stopMachines.Invoke();
        fadeIn.Invoke();
    }

    public void ResetPlayerPosition()
    {
        Debug.Log("Player position reset");
        player.transform.position = spawnPoint.transform.position;
        player.transform.rotation = spawnPoint.transform.rotation;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

}
