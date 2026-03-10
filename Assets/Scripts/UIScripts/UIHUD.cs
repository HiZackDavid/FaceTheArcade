using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : UIElement
{
    [SerializeField] private TextMeshProUGUI timeCounterText;

    [SerializeField] private Image[] cheatCodeImages;

    [SerializeField] private GameObject hideableElement;
    
    public void UpdateTimer(float amount)
    {
        int minutes = Mathf.FloorToInt(amount / 60);
        int seconds = Mathf.FloorToInt(amount % 60);

        timeCounterText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    protected override void OnShow()
    {
        int times = GameManager.instance.GetCheatCodes();
        foreach (Image img in cheatCodeImages)
        {
            if (times > 0)
            {
                img.enabled = true;
                times--;
            }
        }
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
