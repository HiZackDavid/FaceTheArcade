using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ArcadeMachineController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera arcadeCamera;
    [SerializeField] private MonoBehaviour minigameBehaviour;
    [SerializeField] private float startGameDelay = 1.0f;

    [Header("Points System")]
    [SerializeField] private int pointsPerSecond = 5;
    [SerializeField] private float minTimeForBreaking = 15.0f;
    [SerializeField] private float maxTimeForBreaking = 30.0f;

    [SerializeField] private Timer machineTimer;
    private bool canInteract = false;

    private IMinigame minigame;


    public bool isAvailable() => canInteract;

    private void Awake()
    {
        minigame = minigameBehaviour as IMinigame;
        machineTimer = GetComponentInChildren<Timer>();

    }

    private void Start()
    {
        GameManager.instance.startMachines.AddListener(resetTimer);
        GameManager.instance.stopMachines.AddListener(stopTimer);
    }

    public void Interact()
    {
        if (canInteract)
        {
            if (arcadeCamera != null)
            {
                CameraManager.instance.SwitchToCamera(arcadeCamera, false);
            }

            StartCoroutine(StartMinigameAfterDelay());
        }
    }

    private IEnumerator StartMinigameAfterDelay()
    {
        yield return new WaitForSeconds(startGameDelay);
        minigame?.StartGame(this);
    }

    public void onMachineTimerSecond()
    {
        GameManager.instance.IncScoreBy(pointsPerSecond);
    }

    public void onMachineTimerFinished()
    {
        canInteract = true;

        // Show mad client points

        // Play a good SFX
    }

    public void resetTimer()
    {
        machineTimer.StartTimer(Random.Range(minTimeForBreaking, maxTimeForBreaking));
        canInteract = false;

        // Show Icon points

        // Play a bad SFX
    }

    public void stopTimer()
    {
        machineTimer.StopTimer();
    }
}
