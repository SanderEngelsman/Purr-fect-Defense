using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        isFlying = true; // Ensure flying status
    }
}
