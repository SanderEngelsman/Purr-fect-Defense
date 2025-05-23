using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    [SerializeField] protected float range = 3f;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float damage = 10f;
    protected float attackTimer = 0f;
    protected GameObject target;
    public bool isStunned = false;
    protected float stunTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"SpriteRenderer missing on {gameObject.name}. Stun visual effect will not work.", this);
        }
    }

    protected virtual void Update()
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
            return;
        }

        FindTarget();
        if (target != null && CanAttack())
        {
            Attack();
        }
    }

    protected abstract void FindTarget();

    protected abstract bool CanAttack();

    protected abstract void Attack();

    protected virtual bool IsValidTarget(Enemy enemy)
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