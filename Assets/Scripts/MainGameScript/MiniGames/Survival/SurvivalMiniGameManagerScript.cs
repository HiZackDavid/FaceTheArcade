using UnityEngine;

public class SurvivalMiniGameManagerScript : MonoBehaviour, IMinigame
{
    public GameObject gameplayArena;
    
    public void StartGame()
    {
        gameplayArena.SetActive(true);
    }

    public void ResetGame()
    {
        
    }

    public void EndGame()
    {
        gameplayArena.SetActive(false);
        CameraManager.instance.SwitchToPrimaryCamera();
    }
}
