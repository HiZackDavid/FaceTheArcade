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
    
    [Header("Controllers")]
    [SerializeField] private LoAAnomalyController anomalyController;
    [SerializeField] private PlayerMouvementRotationSprites playerMovement;
    
    [Header("Start Indicator")]
    [SerializeField] private GameObject movementIndicator;
    [SerializeField] private GameObject[] gameplayObjectsToEnableOnStart;

    [Header("Minigame Wrapper")]
    [SerializeField] GameObject container;
    
    [Header("EndOfGame Text References")]
    [SerializeField] private GameObject winLoseTextContainer;
    [SerializeField] private GameObject winnerText;
    [SerializeField] private GameObject loserText;
    
    private float _shutDownGameDelay = 5.0f;

    private ArcadeMachineController parentMachine;
    private bool _gameEnding;
    private bool _gameStarted;
    private Coroutine _shutdownCoroutine;

    void Update()
    {
        if (!_gameStarted) return;
        CheckHealth();
    }

    void OnEnable()
    {
        if (playerMovement)
        {
            playerMovement.OnPlayerMoved += HandleFirstMovement;
        }
    }
    
    void OnDisable()
    {
        if (playerMovement)
        {
            playerMovement.OnPlayerMoved -= HandleFirstMovement;
        }
    }

    void SetGameplayEnabledState(bool isEnabled)
    {
        if (container != null) container.SetActive(isEnabled);
    }

    void ResetPositions()
    {
        if (player && playerSpawnPoint) player.position = playerSpawnPoint.position;
        if (anomaly && anomalySpawnPoint) anomaly.position = anomalySpawnPoint.position;
    }

    void ResetEndOfGameTexts()
    {
        winLoseTextContainer.SetActive(false);
        winnerText.SetActive(false);
        loserText.SetActive(false);
    }

    void ResetHealth(CharacterHealthScript characterHealth)
    {
        if (characterHealth)
        {
            characterHealth.gameObject.SetActive(true);
            characterHealth.ResetHealthState();
        }
    }

    void SetEntitesActive(bool isActive)
    {
        playerHealth.gameObject.SetActive(isActive);
        anomalyHealth.gameObject.SetActive(isActive);
    }

    void CheckHealth()
    {
        if (_gameEnding) return;
        
        bool playerIsDead = playerHealth.IsDead();
        bool anomalyIsDead = anomalyHealth.IsDead();
        
        if (playerIsDead || anomalyIsDead)
        {
            _gameEnding = true;
            
            winLoseTextContainer.SetActive(true);
            winnerText.SetActive(!playerIsDead);
            loserText.SetActive(playerIsDead);
            
            playerHealth.gameObject.SetActive(anomalyIsDead);
            anomalyHealth.gameObject.SetActive(false);
            
            _shutdownCoroutine = StartCoroutine(ShutDownMinigameAfterDelay());
        }
    }

    void SetGameplayObjectsActive(bool isActive)
    {
        if (gameplayObjectsToEnableOnStart == null) return;

        foreach (GameObject obj in gameplayObjectsToEnableOnStart)
        {
            if (obj) obj.SetActive(isActive);
        }
    }

    void PrepareWaitingPhase()
    {
        if (movementIndicator) movementIndicator.SetActive(true);
        if (anomalyController)
        {
            anomalyController.ResetControllerState();
            anomalyController.enabled = false;
        }
        
        SetGameplayObjectsActive(false);
    }
    
    void StartGameplay() 
    {
        if (_gameStarted) return;
        
        _gameStarted = true;

        if (movementIndicator) movementIndicator.SetActive(false);
        if (anomalyController) anomalyController.enabled = true;

        SetGameplayObjectsActive(true);
    }

    void HandleFirstMovement()
    {
        if (_gameStarted) return;
        if (playerMovement) playerMovement.OnPlayerMoved -= HandleFirstMovement;
        
        StartGameplay();
    }
    
    public void StartGame(ArcadeMachineController parentMachine)
    {
        this.parentMachine = parentMachine;
        ResetGame();
    }

    public void ResetGame()
    {
        if (_shutdownCoroutine != null)
        {
            StopCoroutine(_shutdownCoroutine);
            _shutdownCoroutine = null;
        }
        
        _gameEnding = false;
        _gameStarted = false;
        
        ResetPositions();
        SetEntitesActive(true);
        ResetHealth(playerHealth);
        ResetHealth(anomalyHealth);
        ResetEndOfGameTexts();
        SetGameplayEnabledState(true);
        
        if (anomalyController) anomalyController.ResetControllerState();

        PrepareWaitingPhase();
    }

    public void EndGame()
    {
        SetGameplayEnabledState(false);
        CameraManager.instance.SwitchToPrimaryCamera();
    }
    
    public IEnumerator ShutDownMinigameAfterDelay()
    {
        yield return new WaitForSeconds(_shutDownGameDelay);
        EndGame();
        _shutdownCoroutine = null;
    }
}
