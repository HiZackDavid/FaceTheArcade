using UnityEngine;
using UnityEngine.UI;

public class MainMenuScript : UIElement
{

    [SerializeField] private VerticalLayoutGroup mainMenuVL;
    [SerializeField] private VerticalLayoutGroup howToVL;

    public void onHowToButton()
    {
        howToVL.gameObject.SetActive(true);
        mainMenuVL.gameObject.SetActive(false);
    }

    public void onReturnButton()
    {
        howToVL.gameObject.SetActive(false);
        mainMenuVL.gameObject.SetActive(true);
    }
}
