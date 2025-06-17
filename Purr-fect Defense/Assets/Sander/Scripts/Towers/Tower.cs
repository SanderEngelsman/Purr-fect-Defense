using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [SerializeField] public float range;
    [SerializeField] public float attackSpeed;
    [SerializeField] public float damage;
    [SerializeField, Tooltip("Prefab for stun overlay animation")]
    private GameObject stunOverlayPrefab;
    public float attackTimer;
    public GameObject target;
    public bool isStunned = false;
    public float stunTimer;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private GameObject stunOverlayInstance;
    private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger");

    private void OnValidate()
    {
        if (stunOverlayPrefab == null)
            Debug.LogWarning($"StunOverlayPrefab not assigned on {gameObject.name}. Stun visual effect will not work.", this);
    }

    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            Debug.LogWarning($"SpriteRenderer missing on {gameObject.name}.", this);
        if (animator == null)
            Debug.LogWarning($"Animator missing on {gameObject.name}. Animations will not work.", this);
    }

    public virtual void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                if (stunOverlayInstance != null)
                {
                    Destroy(stunOverlayInstance);
                    stunOverlayInstance = null;
                    Debug.Log($"Stun overlay removed from {gameObject.name}.", this);
                }
            }
            return;
        }

        FindTarget();
        if (target != null && CanAttack())
        {
            Attack();
        }
    }

    public virtual void FindTarget()
    {
        target = null; // Default: No targeting
    }

    public virtual bool CanAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackSpeed)
        {
            attackTimer = 0f;
            return true;
        }
        return false;
    }

    public virtual void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
            Debug.Log($"Triggered attack animation on {gameObject.name}", this);
        }
    }

    public virtual bool IsValidTarget(Enemy enemy)
    {
        return true;
    }

    public virtual void Stun(float duration)
    {
        if (isStunned && stunTimer > duration)
            return; // Don't override longer stun

        isStunned = true;
        stunTimer = duration;

        // Remove existing overlay
        if (stunOverlayInstance != null)
        {
            Destroy(stunOverlayInstance);
        }

        // Instantiate new overlay
        if (stunOverlayPrefab != null)
        {
            stunOverlayInstance = Instantiate(stunOverlayPrefab, transform.position, Quaternion.identity, transform);
            SpriteRenderer overlayRenderer = stunOverlayInstance.GetComponent<SpriteRenderer>();
            if (overlayRenderer != null && spriteRenderer != null)
            {
                overlayRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // Render above tower
            }
            Debug.Log($"Stun overlay instantiated on {gameObject.name} for {duration}s.", this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}