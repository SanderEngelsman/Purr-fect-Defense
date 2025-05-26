using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    public override void Start()
    {
        base.Start();
        isFlying = true;
    }
}
