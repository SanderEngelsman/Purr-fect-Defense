using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float speed = 2f;
    [SerializeField] public bool isFlying = false;
    [SerializeField] public float currencyValue = 10f;
    [SerializeField] public float shieldAttackRange = 0.5f;
    [SerializeField] public float shieldAttackDamage = 10f;
    [SerializeField] public float shieldAttackInterval = 1f;
    public float health;
    public Path path;
    public int currentWaypointIndex = 0;
    public GameManager gameManager;
    public ShieldTower shieldTarget;
    public float shieldAttackTimer = 0f;
    public bool isAttackingBase = false;
    private EnemyHealthBar healthBar;
    private SpriteRenderer spriteRenderer;
    private float previousX;
    private static int enemySpawnOrderCounter = 0; // Tracks enemy spawn order
    private Animator animator; // Added for animation control

    public virtual void Start()
    {
        health = maxHealth;
        path = FindObjectOfType<Path>();
        gameManager = FindObjectOfType<GameManager>();
        healthBar = GetComponent<EnemyHealthBar>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Cache Animator
        previousX = transform.position.x;
        // Assign spawn order for layering
        ZLayering zLayering = GetComponent<ZLayering>();
        if (zLayering != null)
        {
            zLayering.SetOrder(++enemySpawnOrderCounter);
        }
        if (spriteRenderer != null)
        {
            Debug.Log($"Enemy {gameObject.name}: SpriteRenderer enabled={spriteRenderer.enabled}, sprite={(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "null")}, sortingLayer={spriteRenderer.sortingLayerName}, order={spriteRenderer.sortingOrder}, color={spriteRenderer.color}", this);
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: No SpriteRenderer found! Sprite flipping will not work.", this);
        }
        if (animator == null)
        {
            Debug.LogWarning($"Animator missing on {gameObject.name}. Animations will not work.", this);
        }
    }

    public virtual void Update()
    {
        if (isAttackingBase)
        {
            AttackBase();
        }
        else if (shieldTarget != null && !isFlying)
        {
            AttackShield();
        }
        else
        {
            Move();
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false); // Revert to idle when moving
            }
        }
    }

    public void Move()
    {
        if (currentWaypointIndex >= path.GetWaypointCount())
        {
            isAttackingBase = true;
            return;
        }

        Vector3 targetPos = path.GetWaypoint(currentWaypointIndex);
        // Ignore waypoint Z to preserve ZLayering.cs control
        targetPos.z = transform.position.z;
        float currentX = transform.position.x;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Flip sprite based on X movement direction
        if (spriteRenderer != null)
        {
            float deltaX = transform.position.x - currentX;
            if (deltaX < 0) // Moving left
            {
                spriteRenderer.flipX = true;
            }
            else if (deltaX > 0) // Moving right
            {
                spriteRenderer.flipX = false;
            }
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            currentWaypointIndex++;
        }

        if (!isFlying)
        {
            foreach (var shield in FindObjectsOfType<ShieldTower>())
            {
                if (Vector3.Distance(transform.position, shield.transform.position) < shieldAttackRange)
                {
                    shieldTarget = shield;
                    break;
                }
            }
        }
    }

    public void AttackShield()
    {
        if (shieldTarget == null)
        {
            shieldTarget = null;
            return;
        }

        shieldAttackTimer += Time.deltaTime;
        if (shieldAttackTimer >= shieldAttackInterval)
        {
            shieldTarget.TakeDamage(shieldAttackDamage);
            shieldAttackTimer = 0f;
            if (animator != null)
            {
                animator.SetBool("IsAttacking", true); // Trigger attack animation
                Debug.Log($"Playing attack animation on {gameObject.name} (AttackShield)", this);
            }
        }
        else if (animator != null)
        {
            animator.SetBool("IsAttacking", false); // Revert to idle between attacks
        }
    }

    public void AttackBase()
    {
        shieldAttackTimer += Time.deltaTime;
        if (shieldAttackTimer >= shieldAttackInterval)
        {
            gameManager.TakeBaseDamage(shieldAttackDamage);
            shieldAttackTimer = 0f;
            if (animator != null)
            {
                animator.SetBool("IsAttacking", true); // Trigger attack animation
                Debug.Log($"Playing attack animation on {gameObject.name} (AttackBase)", this);
            }
        }
        else if (animator != null)
        {
            animator.SetBool("IsAttacking", false); // Revert to idle between attacks
        }
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (healthBar != null)
        {
            healthBar.ShowHealthBar(); // Fixed: Use correct method
        }
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager.AddCurrency(currencyValue);
        Destroy(gameObject);
    }

    public virtual void ReachEnd()
    {
        // No longer used
    }

    public float Health => health;
}