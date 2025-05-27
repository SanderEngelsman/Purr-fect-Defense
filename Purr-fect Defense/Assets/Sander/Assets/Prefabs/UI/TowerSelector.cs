using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSelector : MonoBehaviour
{
    private GameObject selectedTower;
    private RangeVisualizer rangeVisualizer;
    private TilemapManager tilemapManager;

    private void Start()
    {
        tilemapManager = FindObjectOfType<TilemapManager>();
        if (tilemapManager == null)
        {
            Debug.LogWarning("TilemapManager not found. TowerSelector will not function.", this);
        }
    }

    private void Update()
    {
        if (tilemapManager == null || tilemapManager.IsPlacingTower()) return; // Ignore clicks during placement

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int layerMask = LayerMask.GetMask("Default"); // Adjust if towers use a custom layer
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                Tower tower = hit.collider.GetComponent<Tower>();
                if (tower != null && CanAttack(tower))
                {
                    Debug.Log($"Selected tower: {tower.gameObject.name}", tower.gameObject);
                    SelectTower(tower.gameObject);
                }
                else
                {
                    Debug.Log("Clicked non-attacking tower or other object. Deselecting.", hit.collider.gameObject);
                    DeselectTower();
                }
            }
            else
            {
                Debug.Log("Clicked empty space. Deselecting.");
                DeselectTower();
            }
        }
    }

    private void SelectTower(GameObject tower)
    {
        if (selectedTower == tower)
        {
            Debug.Log($"Tower {tower.name} already selected. Showing range.", tower);
            if (rangeVisualizer != null)
            {
                rangeVisualizer.Show();
            }
            return;
        }

        DeselectTower(); // Clear previous selection
        selectedTower = tower;
        rangeVisualizer = selectedTower.GetComponent<RangeVisualizer>();
        if (rangeVisualizer == null)
        {
            rangeVisualizer = selectedTower.AddComponent<RangeVisualizer>();
            Debug.Log($"Added new RangeVisualizer to {selectedTower.name}", selectedTower);
        }
        else
        {
            Debug.Log($"Reusing existing RangeVisualizer on {selectedTower.name}", selectedTower);
        }
        rangeVisualizer.SetRange(tower.GetComponent<Tower>().range);
        rangeVisualizer.Show();
    }

    private void DeselectTower()
    {
        if (selectedTower != null)
        {
            Debug.Log($"Deselecting tower: {selectedTower.name}", selectedTower);
            if (rangeVisualizer != null)
            {
                Debug.Log($"Hiding RangeVisualizer on {selectedTower.name}", selectedTower);
                rangeVisualizer.Hide();
                // Do not destroy rangeVisualizer to allow reuse
            }
            selectedTower = null;
            rangeVisualizer = null;
        }
    }

    private bool CanAttack(Tower tower)
    {
        // Exclude GeneratorTower and ShieldTower
        return tower is ProjectileTower || tower is ScratchTower;
    }
}
