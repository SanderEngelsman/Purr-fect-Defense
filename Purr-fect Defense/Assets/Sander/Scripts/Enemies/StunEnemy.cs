using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEnemy : Enemy
{
    [SerializeField] private float stunRange = 2f;
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private float stunInterval = 10f;
    private float stunTimer = 0f;

    protected override void Update()
    {
        base.Update();
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunInterval)
        {
            StunTowers();
            stunTimer = 0f;
        }
    }

    private void StunTowers()
    {
        foreach (var tower in FindObjectsOfType<Tower>())
        {
            if (Vector3.Distance(transform.position, tower.transform.position) <= stunRange)
            {
                tower.Stun(stunDuration);
            }
        }
    }
}
