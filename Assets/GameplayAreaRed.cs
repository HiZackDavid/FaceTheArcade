using System.Collections;
using UnityEngine;

public class GameplayAreaRed : MonoBehaviour, IMinigame
{
    [SerializeField] private CharacterHealthScript playerHealth;
    [SerializeField] private CharacterHealthScript anomalyHealth;
    [SerializeField] private float shutDownGameDelay = 5f;

    private bool gameIsEnding = false;
    private Coroutine shutdownCoroutine;

    private void Update()
    {
        if (!gameObject.activeInHierarchy || gameIsEnding) return;

        CheckHealth();
    }

    public void StartGame()
    {
        ResetGame();
        gameObject.SetActive(true);
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
            gameIsEnding = true;
            shutdownCoroutine = StartCoroutine(ShutDownMinigameAfterDelay());
        }
    }
}