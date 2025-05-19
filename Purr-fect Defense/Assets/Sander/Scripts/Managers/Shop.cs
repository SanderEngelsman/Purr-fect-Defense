using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public TowerData[] towers;
    public TilemapManager tilemapManager;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void SelectTower(int index)
    {
        if (index >= 0 && index < towers.Length)
        {
            if (gameManager.SpendCurrency(towers[index].cost))
            {
                tilemapManager.StartPlacingTower(towers[index].prefab);
            }
        }
    }
}
