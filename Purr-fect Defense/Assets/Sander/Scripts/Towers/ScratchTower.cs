using UnityEngine;

public class ScratchTower : Tower
{
    public override void FindTarget()
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

    public override bool CanAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackSpeed)
        {
            attackTimer = 0f;
            return true;
        }
        return false;
    }

    public override void Attack()
    {
        base.Attack(); // Triggers animation
        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    public override bool IsValidTarget(Enemy enemy)
    {
        return !enemy.isFlying; // Only target non-flying enemies
    }
}