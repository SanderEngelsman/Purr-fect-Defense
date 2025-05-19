using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScratchTower : Tower
{
    protected void Start()
    {
        // Removed base.Start() as Tower.cs has no Start method
    }

    protected override bool IsValidTarget(Enemy enemy)
    {
        return !enemy.isFlying; // Only target ground enemies
    }

    protected override void Attack()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}
