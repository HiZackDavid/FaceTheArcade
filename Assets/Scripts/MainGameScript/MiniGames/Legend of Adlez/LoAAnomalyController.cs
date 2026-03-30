using UnityEngine;

public class LoAAnomalyController : MonoBehaviour
{
    public enum HeartMonsterState
    {
        Idle,
        Telegraph,
        Charge,
        Recover
    }
    
    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public Transform telegraphArrow;

    [Header("Timings")]
    public float idleDuration = 0.75f;
    public float telegraphDuration = 0.75f;
    public float chargeDuration = 0.6f;
    public float recoverDuration = 0.4f;

    [Header("Charge")]
    public float chargeSpeed = 5f;

    private HeartMonsterState currentState;
    private float stateTimer;
    private Vector2 chargeDirection;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeState(HeartMonsterState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case HeartMonsterState.Idle:
                UpdateIdle();
                break;

            case HeartMonsterState.Telegraph:
                UpdateTelegraph();
                break;

            case HeartMonsterState.Charge:
                UpdateCharge();
                break;

            case HeartMonsterState.Recover:
                UpdateRecover();
                break;
        }
    }
    
    void FixedUpdate()
    {
        if (currentState == HeartMonsterState.Charge)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == HeartMonsterState.Charge)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(HeartMonsterState.Recover);
        }
    }

    void UpdateIdle()
    {
        if (stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Telegraph);
        }
    }
    
    void UpdateTelegraph()
    {
        if (player != null)
        {
            chargeDirection = ((Vector2)(player.position - transform.position)).normalized;
            UpdateTelegraphVisual();
        }

        if (stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Charge);
        }
    }
    
    void UpdateCharge()
    {
        if (stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Recover);
        }
    }
    
    private void UpdateRecover()
    {
        if (stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Idle);
        }
    }
    
    void ChangeState(HeartMonsterState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case HeartMonsterState.Idle:
                stateTimer = idleDuration;
                HideTelegraph();
                break;

            case HeartMonsterState.Telegraph:
                stateTimer = telegraphDuration;
                ShowTelegraph();
                break;

            case HeartMonsterState.Charge:
                stateTimer = chargeDuration;
                HideTelegraph();
                break;

            case HeartMonsterState.Recover:
                stateTimer = recoverDuration;
                HideTelegraph();
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
    
    private void ShowTelegraph()
    {
        if (telegraphArrow != null)
            telegraphArrow.gameObject.SetActive(true);
    }

    private void HideTelegraph()
    {
        if (telegraphArrow != null)
            telegraphArrow.gameObject.SetActive(false);
    }

    private void UpdateTelegraphVisual()
    {
        if (telegraphArrow == null) return;

        telegraphArrow.up = chargeDirection;
    }
}
