using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float speed = 2f;
    [SerializeField] public bool isFlying = false;
    [SerializeField] protected float currencyValue = 10f;
    [SerializeField] protected float shieldAttackRange = 0.5f;
    [SerializeField] protected float shieldAttackDamage = 10f;
    [SerializeField] protected float shieldAttackInterval = 1f;
    protected float health;
    protected Path path;
    protected int currentWaypointIndex = 0;
    protected GameManager gameManager;
    protected ShieldTower shieldTarget;
    protected float shieldAttackTimer = 0f;

    protected virtual void Start()
    {
        health = maxHealth;
        path = FindObjectOfType<Path>();
        gameManager = FindObjectOfType<GameManager>();
    }

    protected virtual void Update()
    {
        if (shieldTarget != null && !isFlying)
        {
            AttackShield();
        }
        else
        {
            Move();
        }
    }

    protected void Move()
    {
        if (currentWaypointIndex >= path.GetWaypointCount())
        {
            ReachEnd();
            return;
        }

        Vector3 target = path.GetWaypoint(currentWaypointIndex);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentWaypointIndex++;
        }

        // Check for ShieldTower in range
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

    protected void AttackShield()
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

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        gameManager.AddCurrency(currencyValue);
        Destroy(gameObject);
    }

    protected virtual void ReachEnd()
    {
        gameManager.TakeBaseDamage(shieldAttackDamage);
        Destroy(gameObject);
    }
}
