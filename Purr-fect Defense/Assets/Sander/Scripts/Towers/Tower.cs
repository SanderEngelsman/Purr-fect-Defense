using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [SerializeField] public float range = 3f;
    [SerializeField] public float attackSpeed = 1f;
    [SerializeField] public float damage = 10f;
    public float attackTimer = 0f;
    public GameObject target;
    public bool isStunned = false;
    public float stunTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator; // Added for animation control

    public virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Cache Animator
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"SpriteRenderer missing on {gameObject.name}. Stun visual effect will not work.", this);
        }
        if (animator == null)
        {
            Debug.LogWarning($"Animator missing on {gameObject.name}. Animations will not work.", this);
        }
    }

    public virtual void Update()
    {
        if (isStunned)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.blue; // Stun indication
            }
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor; // Revert color
                }
            }
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false); // Ensure idle during stun
            }
            return;
        }

        FindTarget();
        if (target != null && CanAttack())
        {
            Attack();
        }
        else if (animator != null)
        {
            animator.SetBool("IsAttacking", false); // Revert to idle if not attacking
        }
    }

    public virtual void FindTarget()
    {
        target = null;
        // Default: No targeting
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
            animator.SetBool("IsAttacking", true); // Trigger attack animation
            Debug.Log($"Playing attack animation on {gameObject.name}", this);
        }
    }

    public virtual bool IsValidTarget(Enemy enemy)
    {
        return true;
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}