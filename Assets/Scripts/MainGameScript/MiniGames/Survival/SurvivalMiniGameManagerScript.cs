using System;
using UnityEngine;
using System.Collections;

public class SurvivalMiniGameManagerScript : MonoBehaviour, IMinigame
{

    public CharacterHealthScript playerHealth;
    public CharacterHealthScript anomalyHealth;
    
    public GameObject winLoseTextContainer;
    public GameObject winnerText;
    public GameObject loserText;
    
    private float _shutDownGameDelay = 5.0f;
    
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
        winLoseTextContainer.SetActive(false);
        winnerText.SetActive(false);
        loserText.SetActive(false);
        
        gameObject.SetActive(false);
        
        CameraManager.instance.SwitchToPrimaryCamera();
    }

    private void CheckHealth()
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
