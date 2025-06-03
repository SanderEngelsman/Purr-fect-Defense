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

    public virtual void Start()
    {
        health = maxHealth;
        path = FindObjectOfType<Path>();
        gameManager = FindObjectOfType<GameManager>();
        healthBar = GetComponent<EnemyHealthBar>();
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
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

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
        }
    }

    public void AttackBase()
    {
        shieldAttackTimer += Time.deltaTime;
        if (shieldAttackTimer >= shieldAttackInterval)
        {
            gameManager.TakeBaseDamage(shieldAttackDamage);
            shieldAttackTimer = 0f;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (healthBar != null)
        {
            healthBar.ShowHealthBar();
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
    