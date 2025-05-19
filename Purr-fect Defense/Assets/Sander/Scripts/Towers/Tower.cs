using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] protected float range = 3f;
    [SerializeField] protected float attackSpeed = 1f; // Attacks per second
    [SerializeField] protected float damage = 10f;
    protected float attackTimer = 0f;
    protected GameObject target;
    public bool isStunned = false;
    protected float stunTimer = 0f;

    protected virtual void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
                isStunned = false;
            return;
        }

        FindTarget();
        if (target != null && CanAttack())
        {
            Attack();
        }
    }

    protected virtual void FindTarget()
    {
        target = null;
        float closestDistance = range;
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= range && IsValidTarget(enemy))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = enemy.gameObject;
                }
            }
        }
    }

    protected virtual bool IsValidTarget(Enemy enemy)
    {
        return true; // Override in derived classes
    }

    protected virtual bool CanAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackSpeed)
        {
            attackTimer = 0f;
            return true;
        }
        return false;
    }

    protected virtual void Attack()
    {
        // Implemented in derived classes
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
