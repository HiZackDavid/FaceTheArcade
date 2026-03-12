using UnityEngine;
using System.Collections;

public class LoAMinigame : MonoBehaviour, IMinigame
{
    [Header("Characters References")]
    [SerializeField] Transform player;
    [SerializeField] Transform anomaly;
    
    [Header("Spawn Points")]
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] Transform anomalySpawnPoint;
    
    [Header("Gameplay References")]
    [SerializeField] private CharacterHealthScript playerHealth;
    [SerializeField] private CharacterHealthScript anomalyHealth;

    [Header("Minigame Wrapper")]
    [SerializeField] GameObject container;
    
    [Header("EndOfGame Text References")]
    [SerializeField] private GameObject winLoseTextContainer;
    [SerializeField] private GameObject winnerText;
    [SerializeField] private GameObject loserText;
    
    private float _shutDownGameDelay = 5.0f;

    private void Update()
    {
        CheckHealth();
    }

    public void StartGame()
    {
        ResetGame();
        SetGameplayEnabledState(true);
    }

    public void ResetGame()
    {
        ResetGameState();
        SetGameplayEnabledState(true);
    }

    public void EndGame()
    {
        SetGameplayEnabledState(false);
        CameraManager.instance.SwitchToPrimaryCamera();
    }

    void SetGameplayEnabledState(bool isEnabled)
    {
        if (container != null) container.SetActive(isEnabled);
    }

    void ResetGameState()
    {
        if (player != null && playerSpawnPoint != null) player.position = playerSpawnPoint.position;
        if (anomaly != null && anomalySpawnPoint != null) anomaly.position = anomalySpawnPoint.position;
        
        winLoseTextContainer.SetActive(false);
        winnerText.SetActive(false);
        loserText.SetActive(false);
    }

    void CheckHealth()
    {
        bool playerIsDead = playerHealth.IsDead();
        bool anomalyIsDead = anomalyHealth.IsDead();
        
        if (playerIsDead || anomalyIsDead)
        {
            winLoseTextContainer.SetActive(true);
            winnerText.SetActive(!playerIsDead);
            loserText.SetActive(playerIsDead);
            
            StartCoroutine(ShutDownMinigameAfterDelay());
        }
    }
    
    public IEnumerator ShutDownMinigameAfterDelay()
    {
        yield return new WaitForSeconds(_shutDownGameDelay);
        EndGame();
    }
}
