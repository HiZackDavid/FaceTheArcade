using System.Collections;

public interface IMinigame
{
    void StartGame();
    void ResetGame();
    void EndGame();
    IEnumerator ShutDownMinigameAfterDelay();
}
