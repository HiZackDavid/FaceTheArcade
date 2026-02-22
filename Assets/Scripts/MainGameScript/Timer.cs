using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class MyIntEvent : UnityEvent<float> { }

public class Timer : MonoBehaviour
{
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool oneShot = false;
    [SerializeField] private bool paused = false;
    [SerializeField] private bool notifyEachSecond = false;

    [SerializeField] private float timeLeft = 1;
    [SerializeField] private float waitTime = 1;

    public UnityEvent onTimerStopped;
    public MyIntEvent onSecondPassed;

    private int lastSecond = -1;
    private bool canCount = true;

    private void Start()
    {
        if (!autoStart)
            canCount = false;
    }

    public bool isStoped() => paused;

    public void StartTimer(float wait = -1) 
    {
        if (wait > 0)
            waitTime = wait;

        timeLeft = waitTime;
        lastSecond = Mathf.FloorToInt(waitTime);

        paused = false;
        canCount = true;
    }

    public void StopTimer() 
    {
        paused = true;
    }

    public float GetTimeLeft() => timeLeft;


    void Update()
    {
        if (canCount && !paused && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
                timeLeft = 0.0f;

            if (notifyEachSecond)
            {
                int timeLeftFloor = Mathf.FloorToInt(timeLeft);
                if ((lastSecond - timeLeftFloor) == 1)
                {
                    lastSecond = timeLeftFloor;
                    onSecondPassed.Invoke(timeLeft);
                }
            }

        } else if (timeLeft == 0)
        {
            OneTimerEnd();
        }
    }

    private void OneTimerEnd()
    {
        onTimerStopped.Invoke();
        if (!oneShot)
            StartTimer(waitTime);
    }
}
