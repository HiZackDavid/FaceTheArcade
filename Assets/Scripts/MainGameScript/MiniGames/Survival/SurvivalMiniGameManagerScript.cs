using System;
using UnityEngine;

public class SurvivalMiniGameManagerScript : MonoBehaviour, IMinigame
{

    public CharacterHealthScript playerHealth;
    public CharacterHealthScript anomalyHealth;
    
    private void Update()
    {
        CheckHealth();
    }

    public void StartGame()
    {
        gameObject.SetActive(true);
    }

    public void ResetGame(){ }

    public void EndGame()
    {
        CameraManager.instance.SwitchToPrimaryCamera();
        gameObject.SetActive(false);
    }

    private void CheckHealth()
    {
        if (playerHealth != null && playerHealth.IsDead() || (anomalyHealth != null && anomalyHealth.IsDead()))
        {
            EndGame();
        }
    }
}
