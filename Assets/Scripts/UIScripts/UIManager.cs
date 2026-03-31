using System.ComponentModel;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager instace;

    [SerializeField] private UIElement mainMenu;
    [SerializeField] private UIHUD mainHUD;
    [SerializeField] private UIElement[] allUI;


    private UIHUD currentHUD;

    private void Awake()
    {
        if (instace != null) {
            Destroy(this.gameObject);

        } else {
            instace = this;
            DontDestroyOnLoad(this);
        }
    }

    public void ShowMainMenu()
    {
        OnChangeUI(mainMenu);
    }

    public void ShowMainHUD()
    {
        OnChangeUI(mainHUD);
    }

    public void showHideableHUD()
    {
        currentHUD.showHideable();
    }

    public void hideHideableHUD()
    {
        currentHUD.hideHideable();
    }

    public void OnRequestPlay() 
    {
        GameManager.instance.StartGame();
    }

    private void OnChangeUI(UIElement targetUI) 
    {
        foreach (UIElement element in allUI)
        {
            if (element != targetUI)
            {
                element.Hide();
            }
            else
            {
                if (element is UIHUD)
                    currentHUD = (UIHUD)element;

                element.Show();
            }
        }
    }

    public void OnTimerUpdate(float seconds)
    {
        currentHUD.UpdateTimer(seconds);
    } 

    public void OnScoreUpdate(int currentScore)
    {
        // If we want to have different scores per machine
        // We must call different methods here

        currentHUD.UpdateScore(currentScore);
    }

    public void OnRequestQuit() 
    {
        GameManager.instance.CloseGame();
    }

}
