using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using TMPro;

public class TowerSelector : MonoBehaviour
{
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellButtonText;
    private GameObject selectedTower;
    private RangeVisualizer rangeVisualizer;
    private TilemapManager tilemapManager;
    private Shop shop;
    private GameManager gameManager;
    private bool isSellConfirm = false;
    private float sellRefundAmount;

    private void OnValidate()
    {
        if (sellButton == null)
            Debug.LogWarning("SellButton is not assigned in TowerSelector.", this);
        if (sellButtonText == null)
            Debug.LogWarning("SellButtonText is not assigned in TowerSelector.", this);
    }

    private void Start()
    {
        tilemapManager = FindObjectOfType<TilemapManager>();
        shop = FindObjectOfType<Shop>();
        gameManager = FindObjectOfType<GameManager>();
        if (tilemapManager == null)
        {
            Debug.LogWarning("TilemapManager not found. TowerSelector will not function.", this);
        }
        if (shop == null)
        {
            Debug.LogWarning("Shop not found. Selling towers will not work.", this);
        }
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found. Currency updates will not work.", this);
        }
        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (tilemapManager == null || tilemapManager.IsPlacingTower()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Click on UI element (e.g., SellButton), ignoring for tower selection.");
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int layerMask = LayerMask.GetMask("Default");
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);

            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<Tower>() ||
                    hit.collider.GetComponent<ShieldTower>() ||
                    hit.collider.GetComponent<GeneratorTower>())
                {
                    GameObject tower = hit.collider.gameObject;
                    Debug.Log($"Selected tower: {tower.name}", tower);
                    SelectTower(tower);
                }
                else
                {
                    Debug.Log("Clicked non-tower object. Deselecting.", hit.collider.gameObject);
                    DeselectTower();
                }
            }
            else
            {
                Debug.Log("Clicked empty space. Deselecting.");
                DeselectTower();
            }
        }

        if (selectedTower != null && sellButton != null)
        {
            Vector3 towerPos = selectedTower.transform.position;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(towerPos);
            sellButton.GetComponent<RectTransform>().position = new Vector3(screenPos.x, screenPos.y - 50f, screenPos.z);
        }
    }

    private void SelectTower(GameObject tower)
    {
        if (selectedTower == tower)
        {
            Debug.Log($"Tower {tower.name} already selected.", tower);
            if (rangeVisualizer != null)
            {
                rangeVisualizer.Show();
            }
            return;
        }

        DeselectTower();
        selectedTower = tower;

        Tower towerComponent = tower.GetComponent<Tower>();
        if (towerComponent != null && towerComponent.range > 0)
        {
            rangeVisualizer = tower.GetComponent<RangeVisualizer>();
            if (rangeVisualizer == null)
            {
                rangeVisualizer = tower.AddComponent<RangeVisualizer>();
                Debug.Log($"Added new RangeVisualizer to {tower.name}", tower);
            }
            else
            {
                Debug.Log($"Reusing existing RangeVisualizer on {tower.name}", tower);
            }
            try
            {
                rangeVisualizer.SetRange(towerComponent.range);
                rangeVisualizer.Show();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set up RangeVisualizer for {tower.name}: {e.Message}", tower);
                rangeVisualizer = null;
            }
        }
        else
        {
            Debug.Log($"No RangeVisualizer added for {tower.name} (no Tower component or range <= 0).", tower);
        }

        isSellConfirm = false;
        if (sellButton != null && sellButtonText != null)
        {
            sellButton.gameObject.SetActive(true);
            sellButtonText.text = "Sell";
            var colors = sellButton.colors;
            colors.normalColor = Color.white;
            sellButton.colors = colors;
        }
        Debug.Log($"Selected tower: {tower.name}", tower);
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
            }
            selectedTower = null;
            rangeVisualizer = null;
            isSellConfirm = false;
            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(false);
            }
        }
    }

    public void OnSellButtonClicked()
    {
        if (selectedTower == null)
        {
            Debug.LogWarning("Sell button clicked but no tower selected.");
            return;
        }

        if (!isSellConfirm)
        {
            sellRefundAmount = CalculateRefund(selectedTower);
            if (sellRefundAmount <= 0)
            {
                Debug.LogWarning($"Invalid refund amount ({sellRefundAmount}) for {selectedTower.name}. Cannot proceed with sell.", selectedTower);
                DeselectTower();
                return;
            }
            if (sellButtonText != null)
            {
                sellButtonText.text = $"Sell > {sellRefundAmount}";
            }
            if (sellButton != null)
            {
                var colors = sellButton.colors;
                colors.normalColor = new Color(0.7f, 0.7f, 0.7f);
                sellButton.colors = colors;
            }
            isSellConfirm = true;
            Debug.Log($"Sell confirm mode for {selectedTower.name}: Refund {sellRefundAmount}");
        }
        else
        {
            if (tilemapManager != null)
            {
                Tilemap targetTilemap;
                if (selectedTower.GetComponent<ShieldTower>() != null)
                {
                    targetTilemap = tilemapManager.PathTilemap;
                }
                else if (selectedTower.GetComponent<ScratchTower>() != null)
                {
                    Vector3Int cellPos = tilemapManager.RightFacingScratchTowerMap.WorldToCell(selectedTower.transform.position);
                    targetTilemap = tilemapManager.RightFacingScratchTowerMap.HasTile(cellPos) ?
                        tilemapManager.RightFacingScratchTowerMap : tilemapManager.PlacementTilemap;
                }
                else
                {
                    targetTilemap = tilemapManager.PlacementTilemap;
                }
                Vector3Int tilePos = targetTilemap.WorldToCell(selectedTower.transform.position);
                tilemapManager.RemoveTower(tilePos);
            }
            if (gameManager != null)
            {
                gameManager.AddCurrency(sellRefundAmount);
            }
            string towerName = selectedTower.name;
            Destroy(selectedTower);
            DeselectTower();
            Debug.Log($"Sold tower {towerName}, refunded {sellRefundAmount}");
        }
    }

    private float CalculateRefund(GameObject tower)
    {
        if (shop == null || shop.Towers == null)
        {
            Debug.LogWarning("Shop or Towers array is null. Returning 0 refund.", this);
            return 0f;
        }

        string towerName = tower.name.Replace("(Clone)", "");
        foreach (var shopTower in shop.Towers)
        {
            if (shopTower.towerPrefab != null && shopTower.towerPrefab.name == towerName)
            {
                return shopTower.resellPrice;
            }
        }
        Debug.LogWarning($"No matching tower found in Shop for {towerName}. Returning 0 refund.");
        return 0f;
    }
}