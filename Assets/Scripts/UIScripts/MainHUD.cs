using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainHUD : UIHUD
{
    [SerializeField] private float fadeTime = 1f;

    [SerializeField] private Image fades;
    [SerializeField] private TextMeshProUGUI timeOverText;
    [SerializeField] private HorizontalLayoutGroup pointsHL;
    [SerializeField] private TextMeshProUGUI pointsNumber;


    private bool isFinished = false;


    private void Start()
    {
        GameManager.instance.fadeIn.AddListener(FadeIn);
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInAnimation());
    }


    IEnumerator FadeInAnimation()
    {
        StartCoroutine(FadeAlfa(0f, 1f, fadeTime));
        yield return new WaitUntil(() => isFinished);
        yield return new WaitForSeconds(0.65f);
        timeOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        pointsNumber.text = GameManager.instance.GetScore().ToString();
        pointsHL.gameObject.SetActive(true);

        GameManager.instance.ResetPlayerPosition();

        yield return new WaitForSeconds(1f);

        FadeOut();

    }
    public void FadeOut()
    {
        StartCoroutine(FadeOutAnimation());
    }

    IEnumerator FadeOutAnimation()
    {
        timeOverText.gameObject.SetActive(false);
        pointsHL.gameObject.SetActive(false);

        StartCoroutine(FadeAlfa(1f, 0f, fadeTime));
        yield return new WaitUntil(() => isFinished);

        CameraManager.instance.SetStartingCamera();
        GameManager.instance.PauseGame(isReset: true);
    }

    IEnumerator FadeAlfa(float start, float end, float time)
    {
        isFinished = false;
        float timePassed = 0f;
        Color originalColor = fades.color;

        while (timePassed < time)
        {
            timePassed += Time.deltaTime;

            float nuevoAlfa = Mathf.Lerp(start, end, timePassed / time);
            fades.color = new Color(originalColor.r, originalColor.g, originalColor.b, nuevoAlfa);
            yield return null; 
        }

        isFinished = true;

        fades.color = new Color(originalColor.r, originalColor.g, originalColor.b, end);
    }
}
