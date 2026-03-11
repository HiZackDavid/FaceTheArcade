using System;
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
    [SerializeField] private CharacterHealthScript playerHealth;
    [SerializeField] private CharacterHealthScript anomalyHealth;

    [Header("Minigame Wrapper")]
    [SerializeField] GameObject container;

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
    }

    void CheckHealth()
    {
        if (playerHealth != null && playerHealth.IsDead() 
            || (anomalyHealth != null && anomalyHealth.IsDead()))
        {
            EndGame();
        }
    }
}
