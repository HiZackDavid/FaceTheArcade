using UnityEngine;

public class MiniGameGameOver : MonoBehaviour
{
    public HealthDeathProxy playerDeathProxy;
    public BlinkTMP gameOverText; // "Game Over You're dead"

    void Awake()
    {
        playerDeathProxy.OnDied += _ => OnGameOver();
    }

    void OnDestroy()
    {
        if (playerDeathProxy != null) playerDeathProxy.OnDied -= _ => OnGameOver();
    }

    void OnGameOver()
    {
        if (gameOverText) gameOverText.StartBlink();
        Time.timeScale = 0f; 
    }
}