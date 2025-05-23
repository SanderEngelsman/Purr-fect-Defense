using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemy : Enemy
{
    [SerializeField] private GameObject stunProjectilePrefab;
    [SerializeField] private float stunRange = 2f;
    [SerializeField] private float stunDuration = 5f;
    [SerializeField] private float stunInterval = 10f;
    [SerializeField] private float projectileSpeed = 5f;
    private float stunTimer = 0f;

    protected override void Update()
    {
        base.Update();
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunInterval)
        {
            FireStunProjectile();
            stunTimer = 0f;
        }
    }

    private void FireStunProjectile()
    {
        Tower target = null;
        float closestDistance = stunRange;
        foreach (var tower in FindObjectsOfType<Tower>())
        {
            if (tower.GetComponent<ShieldTower>() == null) // Exclude ShieldTower
            {
                float distance = Vector3.Distance(transform.position, tower.transform.position);
                if (distance <= stunRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = tower;
                }
            }
        }

        if (target != null)
        {
            GameObject projectile = Instantiate(stunProjectilePrefab, transform.position, Quaternion.identity);
            StunProjectile projScript = projectile.GetComponent<StunProjectile>();
            projScript.SetTarget(target, stunDuration);
        }
    }
}
