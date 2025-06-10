using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorTower : Tower
{
    [SerializeField] private float currencyPerSecond = 2f;
    private GameManager gameManager;
    private float currencyTimer = 0f;
    private WaveManager waveManager;

    public override void Start()
    {
        base.Start();
        gameManager = FindObjectOfType<GameManager>();
        waveManager = FindObjectOfType<WaveManager>();
    }

    public override void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
                isStunned = false;
            return;
        }

        if (waveManager.IsPreGame || !waveManager.IsWaveActive)
            return;

        currencyTimer += Time.deltaTime;
        if (currencyTimer >= 1f)
        {
            gameManager.AddCurrency(currencyPerSecond);
            currencyTimer = 0f;
        }
    }

    public override void FindTarget()
    {
        target = null; // No targeting needed
    }

    public override bool CanAttack()
    {
        return false; // No attacks
    }

    public override void Attack()
    {
        // No attacks
    }
}