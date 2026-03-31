using System.Collections;

public interface IMinigame
{
    void StartGame(ArcadeMachineController parentMachine);
    void ResetGame();
    void EndGame();
    IEnumerator ShutDownMinigameAfterDelay();
}
