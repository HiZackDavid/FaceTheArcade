using UnityEngine;

public class LoAMinigame : MonoBehaviour, IMinigame
{
    [Header("Characters References")]
    [SerializeField] Transform player;
    [SerializeField] Transform anomaly;
    
    [Header("Spawn Points")]
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] Transform anomalySpawnPoint;
    
    [Header("Gameplay References")]
    [SerializeField] LoAAnomalyController anomalyController;
    [SerializeField] LoACharacterTrapDamage playerTrapDamage;
    [SerializeField] LoACharacterTrapDamage anomalyTrapDamage;
    [SerializeField] LoAMonsterDamageZone anomalyDamageZone;
    
    [Header("UI")]
    [SerializeField] GameObject minigameCanvas;

    bool isRunning;

    public void StartGame()
    {
        ResetGame();
        SetGameplayEnabledState(true);
        isRunning = true;
    }

    public void ResetGame()
    {
        ResetGameState();
        SetGameplayEnabledState(true);
        isRunning = true;
    }

    public void EndGame()
    {
        isRunning = false;
        SetGameplayEnabledState(false);
        CameraManager.instance.SwitchToPrimaryCamera();
    }
    
    void SetGameplayEnabledState(bool isEnabled)
    {
        if (anomalyController != null) anomalyController.enabled = isEnabled;
        if (playerTrapDamage != null) playerTrapDamage.enabled = isEnabled;
        if (anomalyTrapDamage != null) anomalyTrapDamage.enabled = isEnabled;
        if (anomalyDamageZone != null) anomalyDamageZone.enabled = isEnabled;
        if (minigameCanvas != null) minigameCanvas.SetActive(isEnabled);
    }

    void ResetGameState()
    {
        if (player != null && playerSpawnPoint != null) player.position = playerSpawnPoint.position;
        if (anomaly != null && anomalySpawnPoint != null) anomaly.position = anomalySpawnPoint.position;
    }
}
