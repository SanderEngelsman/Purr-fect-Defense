using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorTower : Tower
{
    [SerializeField] private float currencyPerSecond = 2f;
    private GameManager gameManager;
    private float currencyTimer = 0f;
    private WaveManager waveManager;

    protected override void Start()
    {
        base.Start();
        gameManager = FindObjectOfType<GameManager>();
        waveManager = FindObjectOfType<WaveManager>();
    }

    protected override void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
                isStunned = false;
            return;
        }

        if (waveManager.IsPreGame)
            return;

        currencyTimer += Time.deltaTime;
        if (currencyTimer >= 1f)
        {
            gameManager.AddCurrency(currencyPerSecond);
            currencyTimer = 0f;
        }
    }

    protected override void FindTarget()
    {
        // No targeting needed
        target = null;
    }

    protected override bool CanAttack()
    {
        return false; // No attacks
    }

    protected override void Attack()
    {
        // No attacks
    }
}
