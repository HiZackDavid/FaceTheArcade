using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkTMP : MonoBehaviour
{
    public float interval = 0.35f;
    TMP_Text t;

    void Awake() => t = GetComponent<TMP_Text>();

    public void StartBlink()
    {
        gameObject.SetActive(true);
        StartCoroutine(Co());
    }

    IEnumerator Co()
    {
        while (true)
        {
            t.enabled = !t.enabled;
            yield return new WaitForSecondsRealtime(interval);
        }
    }
}