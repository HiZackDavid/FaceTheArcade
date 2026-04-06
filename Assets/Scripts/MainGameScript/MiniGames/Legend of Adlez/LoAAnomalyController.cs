using UnityEngine;

public class LoAAnomalyController : MonoBehaviour
{
    public enum HeartMonsterState
    {
        Idle,
        Telegraph,
        Charge
    }
    
    [Header("World references")]
    public Transform player;
    public Rigidbody2D rb;
    public Transform telegraphArrow;
    public SpriteRenderer telegraphArrowRenderer;

    [Header("Timings")]
    public float idleDuration = 0.75f;
    public float telegraphDuration = 0.75f;
    public float chargeDuration = 0.6f;
    public float recoverDuration = 0.4f;

    [Header("Charge")]
    public float chargeSpeed = 5f;
    
    [Header("Telegraph Colors")]
    public Color idleColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
    public Color telegraphColor = new Color(1f, 0.85f, 0.2f, 1f);
    public Color chargeColor = new Color(1f, 0.2f, 0.2f, 1f);

    private HeartMonsterState _currentState;
    private float _stateTimer;
    private Vector2 _chargeDirection;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (telegraphArrow != null)
        {
            telegraphArrow.gameObject.SetActive(true);
        }
        
        ChangeState(HeartMonsterState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        _stateTimer -= Time.deltaTime;

        switch (_currentState)
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
        }
    }
    
    void FixedUpdate()
    {
        if (_currentState == HeartMonsterState.Charge)
        {
            rb.linearVelocity = _chargeDirection * chargeSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void UpdateIdle()
    {
        if (_stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Telegraph);
        }
    }
    
    void UpdateTelegraph()
    {
        if (player)
        {
            _chargeDirection = ((Vector2)(player.position - transform.position)).normalized;
            UpdateTelegraphVisual();
        }
        
        if (_stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Charge);
        }
    }
    
    void UpdateCharge()
    {
        if (_stateTimer <= 0f)
        {
            ChangeState(HeartMonsterState.Idle);
        }
    }
    
    void ChangeState(HeartMonsterState newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case HeartMonsterState.Idle:
                _stateTimer = idleDuration;
                SetTelegraphColor(idleColor);
                break;

            case HeartMonsterState.Telegraph:
                _stateTimer = telegraphDuration;
                SetTelegraphColor(telegraphColor);
                break;

            case HeartMonsterState.Charge:
                _stateTimer = chargeDuration;
                SetTelegraphColor(chargeColor);
                if (player)
                {
                    _chargeDirection = ((Vector2)(player.position - transform.position)).normalized;
                }
                break;
        }
    }
    
    void SetTelegraphColor(Color color)
    {
        if (telegraphArrowRenderer)
        {
            telegraphArrowRenderer.color = color;
        }
    }

    void UpdateTelegraphVisual()
    {
        if (!telegraphArrow) return;

        telegraphArrow.up = _chargeDirection;
    }

    public void ResetControllerState()
    {
        _chargeDirection = Vector2.zero;
        _stateTimer = 0f;

        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        ChangeState(HeartMonsterState.Idle);
        UpdateTelegraphVisual();
    }
}
