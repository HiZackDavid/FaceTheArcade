using System;
using UnityEngine;
using System.Collections;

public class SurvivalMiniGameManagerScript : MonoBehaviour, IMinigame
{

    public EnemiesSpawnerManagerScript enemiesSpawnerManager;
    public GameObject playerPrefab;
    private CharacterHealthScript _playerHealth;
    private PlayerMouvementRotationMouse _playerMovement;
    public CharacterHealthScript anomalyHealth;
    
    public GameObject winLoseTextContainer;
    public GameObject winnerText;
    public GameObject loserText;
    
    public GameObject movementIndicator;
    
    private float _shutDownGameDelay = 5.0f;

    private void Awake()
    {
        _playerHealth = playerPrefab.GetComponent<CharacterHealthScript>();
        _playerMovement = playerPrefab.GetComponent<PlayerMouvementRotationMouse>();
    }

    private void OnEnable()
    {
        _playerMovement.OnPlayerMoved += EnableZombies;
    }

    private void OnDisable()
    {
        _playerMovement.OnPlayerMoved -= EnableZombies;
        movementIndicator.SetActive(true);
        enemiesSpawnerManager.gameObject.SetActive(false);
    }

    private void EnableZombies()
    {
        _playerMovement.OnPlayerMoved -= EnableZombies;
        movementIndicator.SetActive(false);
        enemiesSpawnerManager.gameObject.SetActive(true);
    }

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
        bool playerIsDead = _playerHealth.IsDead();
        bool anomalyIsDead = anomalyHealth.IsDead();
        
        if (playerIsDead || anomalyIsDead)
        {
            winLoseTextContainer.SetActive(true);
            winnerText.SetActive(!playerIsDead);
            loserText.SetActive(playerIsDead);
            
            if (playerIsDead) enemiesSpawnerManager.PlayerLost();
            if (anomalyIsDead) enemiesSpawnerManager.PlayerWon();
            
            StartCoroutine(ShutDownMinigameAfterDelay());
        }
    }
    
    public IEnumerator ShutDownMinigameAfterDelay()
    {
        yield return new WaitForSeconds(_shutDownGameDelay);
        EndGame();
    }
}
