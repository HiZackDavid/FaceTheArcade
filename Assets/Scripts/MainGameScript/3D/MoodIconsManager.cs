using UnityEngine;

public class MoodIconsManager : MonoBehaviour
{
    public Transform player;
    public GameObject happyIcon;
    public GameObject angryIcon;
    

    void Start()
    {
        IsHappy();
    }
    
    void Update()
    {
        Vector3 target = player.position;
        target.y = transform.position.y;
        transform.LookAt(target);
    }



    public void IsHappy()
    {
        happyIcon.SetActive(true);
        angryIcon.SetActive(false);
    }

    public void IsAngry()
    {
        angryIcon.SetActive(true);
        happyIcon.SetActive(false);
    }
}
