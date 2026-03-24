using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ArcadeMachineController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera arcadeCamera;
    [SerializeField] private MonoBehaviour minigameBehaviour;
    [SerializeField] private float startGameDelay = 1.0f;

    private IMinigame minigame;

    private void Awake()
    {
        minigame = minigameBehaviour as IMinigame;
    }

    public void Interact()
    {
        if (arcadeCamera != null)
        {
            CameraManager.instance.SwitchToCamera(arcadeCamera, false);
        }
        
        StartCoroutine(StartMinigameAfterDelay());
    }

    private IEnumerator StartMinigameAfterDelay()
    {
        yield return new WaitForSeconds(startGameDelay);
        minigame?.StartGame();
    }
}
