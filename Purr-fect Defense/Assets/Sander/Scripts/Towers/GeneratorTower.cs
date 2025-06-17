using UnityEngine;

public class GeneratorTower : Tower
{
    [SerializeField, Tooltip("Amount of currency generated each interval")]
    private float currencyAmount = 20f;
    [SerializeField, Tooltip("Time interval (seconds) between currency generations")]
    private float currencyInterval = 10f;
    [SerializeField, Tooltip("Animator for the tower's animations")]
    private Animator animator;
    private GameManager gameManager;
    private WaveManager waveManager;
    private float currencyTimer = 0f;

    private void OnValidate()
    {
        if (currencyAmount < 0f)
        {
            Debug.LogWarning($"CurrencyAmount ({currencyAmount}) must be non-negative in GeneratorTower.", this);
            currencyAmount = 0f;
        }
        if (currencyInterval <= 0f)
        {
            Debug.LogWarning($"CurrencyInterval ({currencyInterval}) must be positive in GeneratorTower.", this);
            currencyInterval = 1f;
        }
        if (animator == null)
        {
            Debug.LogWarning("Animator not assigned in GeneratorTower. Currency generation animation will not play.", this);
        }
    }

    public override void Start()
    {
        base.Start();
        gameManager = FindObjectOfType<GameManager>();
        waveManager = FindObjectOfType<WaveManager>();
        if (gameManager == null)
            Debug.LogWarning("GameManager not found in GeneratorTower.", this);
        if (waveManager == null)
            Debug.LogWarning("WaveManager not found in GeneratorTower.", this);
        if (animator == null)
            animator = GetComponent<Animator>(); // Auto-assign if possible
    }

    public override void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
                isStunned = false;
            return;
        }

        if (waveManager == null || waveManager.IsPreGame || !waveManager.IsWaveActive)
            return;

        currencyTimer += Time.deltaTime;
        if (currencyTimer >= currencyInterval)
        {
            if (gameManager != null)
            {
                gameManager.AddCurrency(currencyAmount);
                Debug.Log($"Generated {currencyAmount} currency at {Time.time:F2}s", this);
            }
            if (animator != null)
            {
                animator.SetTrigger("GenerateCurrency");
                Debug.Log("Triggered GenerateCurrency animation", this);
            }
            currencyTimer = 0f;
        }
    }

    public override void FindTarget()
    {
        target = null; // No targeting needed
    }

    public override bool CanAttack()
    {
        return false; // No attacks
    }

    public override void Attack()
    {
        // No attacks
    }
}