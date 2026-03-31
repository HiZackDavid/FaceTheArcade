using System.Collections;
using UnityEngine;

public class HideUIAfterDelay : MonoBehaviour
{
    [SerializeField] private GameObject targetUI;
    [SerializeField] private float delay = 10f;

    private void OnEnable()
    {
        if (targetUI != null)
            targetUI.SetActive(true);

        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (targetUI != null)
            targetUI.SetActive(false);
    }
}