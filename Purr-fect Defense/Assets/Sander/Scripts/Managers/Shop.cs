using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private TowerData[] towers;
    [SerializeField] private TextMeshProUGUI[] costLabels;
    private TilemapManager tilemapManager;

    private void Start()
    {
        tilemapManager = FindObjectOfType<TilemapManager>();
        UpdateCostLabels();
    }

    private void UpdateCostLabels()
    {
        for (int i = 0; i < towers.Length && i < costLabels.Length; i++)
        {
            if (costLabels[i] != null)
            {
                costLabels[i].text = $"{towers[i].cost}";
            }
        }
    }

    public void SelectTower(int index)
    {
        if (index >= 0 && index < towers.Length)
        {
            tilemapManager.StartPlacingTower(towers[index].prefab, towers[index].cost);
        }
        else
        {
            Debug.LogWarning($"Invalid tower index: {index}");
        }
    }
}
