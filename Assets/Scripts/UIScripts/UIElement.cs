using UnityEngine;

public class UIElement : MonoBehaviour
{

    public void Show()
    {
        this.gameObject.SetActive(true);
        OnShow();
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }


    public void RequestPlay()
    {
        UIManager.instace.OnRequestPlay();
    }

    public void RequestQuit()
    {
        UIManager.instace.OnRequestQuit();
    }

    protected virtual void OnShow() { }

}
