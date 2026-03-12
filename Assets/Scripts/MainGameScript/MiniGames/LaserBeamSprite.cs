using System.Collections;
using UnityEngine;

public class LaserBeamSprite : MonoBehaviour
{
    public SpriteRenderer sr;

    void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        gameObject.SetActive(false);
    }

    public void Show(Vector2 start, Vector2 end, float widthWorld, float time, float z)
    {
        StopAllCoroutines();
        StartCoroutine(Co(start, end, widthWorld, time, z));
    }

    IEnumerator Co(Vector2 start, Vector2 end, float widthWorld, float time, float z)
    {
        Vector2 dir = end - start;
        float len = dir.magnitude;

        transform.position = new Vector3((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f, z);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // sprite carré 1x1 => scale X = longueur, scale Y = largeur
        transform.localScale = new Vector3(len, widthWorld, 1f);

        gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}