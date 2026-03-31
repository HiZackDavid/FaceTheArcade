using System.Collections;
using UnityEngine;

public class GameplayAreaRed : MonoBehaviour, IMinigame
{
    [SerializeField] private CharacterHealthScript playerHealth;
    [SerializeField] private CharacterHealthScript anomalyHealth;
    
    public GameObject winLoseTextContainer;
    public GameObject winnerText;
    public GameObject loserText;
    
    private float shutDownGameDelay = 5f;

    private bool gameIsEnding = false;
    private Coroutine shutdownCoroutine;

    private ArcadeMachineController parentMachine;

    private void Update()
    {
        if (!gameObject.activeInHierarchy || gameIsEnding) return;

        CheckHealth();
    }

    public void StartGame(ArcadeMachineController parentMachine)
    {
        ResetGame();
        gameObject.SetActive(true);
        this.parentMachine = parentMachine;
    }

    public void ResetGame()
    {
        gameIsEnding = false;

        if (shutdownCoroutine != null)
        {
            StopCoroutine(shutdownCoroutine);
            shutdownCoroutine = null;
        }
    }

    public void EndGame()
    {
        if (shutdownCoroutine != null)
        {
            StopCoroutine(shutdownCoroutine);
            shutdownCoroutine = null;
        }

        winLoseTextContainer.SetActive(false);
        winnerText.SetActive(false);
        loserText.SetActive(false);

        gameObject.SetActive(false);

        if (CameraManager.instance != null)
            CameraManager.instance.SwitchToPrimaryCamera();
    }

    public IEnumerator ShutDownMinigameAfterDelay()
    {
        yield return new WaitForSeconds(shutDownGameDelay);
        EndGame();
    }

    private void CheckHealth()
    {
        if (playerHealth == null || anomalyHealth == null) return;

        bool playerIsDead = playerHealth.IsDead();
        bool anomalyIsDead = anomalyHealth.IsDead();

        if (playerIsDead || anomalyIsDead)
        {
            winLoseTextContainer.SetActive(true);
            winnerText.SetActive(!playerIsDead);
            loserText.SetActive(playerIsDead);
            
            gameIsEnding = true;
            shutdownCoroutine = StartCoroutine(ShutDownMinigameAfterDelay());
        }
    }
}