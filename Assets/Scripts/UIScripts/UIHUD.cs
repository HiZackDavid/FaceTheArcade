using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : UIElement
{
    [SerializeField] private TextMeshProUGUI timeCounterText;

    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private GameObject hideableElement;
    
    public void UpdateTimer(float amount)
    {
        int minutes = Mathf.FloorToInt(amount / 60);
        int seconds = Mathf.FloorToInt(amount % 60);

        timeCounterText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateScore(int currentScore)
    {
        scoreText.text = currentScore.ToString();
    }

    protected override void OnShow()
    {
        scoreText.text = GameManager.instance.GetScore().ToString();
    }

    public void showHideable()
    {
        if (hideableElement)
            hideableElement.SetActive(true);
    }

    public void hideHideable()
    {
        if (hideableElement)
            hideableElement.SetActive(false);
    }

}
