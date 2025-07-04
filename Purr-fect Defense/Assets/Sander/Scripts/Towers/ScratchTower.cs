using UnityEngine;

public class ScratchTower : Tower
{
    [SerializeField] private GameObject scratchEffectPrefab; // Effect prefab
    private bool faceRight = false; // Set by TilemapManager during placement

    // Called by TilemapManager after instantiation
    public void SetFacingDirection(bool shouldFaceRight)
    {
        faceRight = shouldFaceRight;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = faceRight;
            Debug.Log($"ScratchTower set to face {(faceRight ? "right" : "left")}", this);
        }
        else
        {
            Debug.LogWarning("SpriteRenderer not found on ScratchTower.", this);
        }
    }

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
                if (scratchEffectPrefab != null)
                {
                    Vector3 effectPosition = target.transform.position;
                    effectPosition.z = -1f; // Ensure above enemy sprite
                    Instantiate(scratchEffectPrefab, effectPosition, Quaternion.identity);
                    Debug.Log($"Scratch effect spawned on {target.name}", this);
                }
                else
                {
                    Debug.LogWarning("ScratchEffectPrefab is not assigned in ScratchTower.", this);
                }
            }
        }
    }

    public override bool IsValidTarget(Enemy enemy)
    {
        return !enemy.isFlying; // Only target non-flying enemies
    }

    private void OnValidate()
    {
        if (scratchEffectPrefab == null)
        {
            Debug.LogWarning("ScratchEffectPrefab is not assigned in ScratchTower.", this);
        }
    }
}