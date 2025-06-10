using UnityEngine;

public class ProjectileTower : Tower
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    public override void FindTarget()
    {
        target = null;
        float closestDistance = range;
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            // Use 2D distance to ignore Z position
            Vector2 towerPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 enemyPos = new Vector2(enemy.transform.position.x, enemy.transform.position.y);
            float distance = Vector2.Distance(towerPos, enemyPos);

            if (distance <= range && IsValidTarget(enemy))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = enemy.gameObject;
                    Debug.Log($"ProjectileTower {gameObject.name} targeting {enemy.gameObject.name}: distance={distance}, isFlying={enemy.isFlying}", this);
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
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetTarget(target, damage, projectileSpeed);
            }
            else
            {
                Debug.LogWarning($"Projectile prefab missing Projectile script on {gameObject.name}", this);
                Destroy(projectile);
            }
        }
    }
}