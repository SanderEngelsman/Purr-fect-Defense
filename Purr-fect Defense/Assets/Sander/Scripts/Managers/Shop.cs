using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct ShopTowerData
{
    public GameObject towerPrefab;
    public float cost;
    public float resellPrice;
}

public class Shop : MonoBehaviour
{
    [SerializeField] private ShopTowerData[] towers;
    [SerializeField] private TextMeshProUGUI[] costLabels;
    [SerializeField] private Button shopButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button[] towerButtons;
    public ShopTowerData[] Towers => towers; // Public getter
    private TilemapManager tilemapManager;
    private GameManager gameManager;
    private ShopPanelAnimator panelAnimator;
    private bool isPanelOpen = false;

    private void OnValidate()
    {
        if (shopButton == null)
            Debug.LogWarning("ShopButton is not assigned in Shop. Shop panel will not toggle.", this);
        if (shopPanel == null)
            Debug.LogWarning("ShopPanel is not assigned in Shop. Tower buttons will not be displayed.", this);
        if (towers.Length != costLabels.Length || towers.Length != towerButtons.Length)
            Debug.LogWarning("Towers, CostLabels, and TowerButtons arrays must have the same length in Shop.", this);
    }

    private void Start()
    {
        tilemapManager = FindObjectOfType<TilemapManager>();
        gameManager = FindObjectOfType<GameManager>();
        panelAnimator = shopPanel?.GetComponent<ShopPanelAnimator>();
        if (tilemapManager == null)
            Debug.LogWarning("TilemapManager not found in Shop.", this);
        if (gameManager == null)
            Debug.LogWarning("GameManager not found in Shop.", this);
        if (panelAnimator == null && shopPanel != null)
        {
            Debug.LogWarning("ShopPanel is missing ShopPanelAnimator component. Panel will not animate.", shopPanel);
        }
        UpdateCostLabels();
    }

    public void ToggleShopPanel()
    {
        isPanelOpen = !isPanelOpen;
        if (panelAnimator != null)
        {
            if (isPanelOpen)
            {
                panelAnimator.OpenPanel();
                Debug.Log("Opening shop panel.");
            }
            else
            {
                panelAnimator.ClosePanel();
                Debug.Log("Closing shop panel.");
            }
        }
        else
        {
            // Fallback: Toggle panel visibility without animation
            if (shopPanel != null)
            {
                shopPanel.SetActive(isPanelOpen);
                Debug.LogWarning("ShopPanelAnimator not found. Using SetActive fallback.", shopPanel);
            }
        }
    }

    public void SelectTower(int index)
    {
        Debug.Log($"SelectTower called with index: {index}");
        if (index >= 0 && index < towers.Length)
        {
            ShopTowerData tower = towers[index];
            if (tower.towerPrefab == null)
            {
                Debug.LogWarning($"Tower prefab at index {index} is null in Shop.", this);
                return;
            }
            Debug.Log($"Attempting to place {tower.towerPrefab.name} with cost: {tower.cost}");
            if (gameManager == null || !gameManager.HasEnoughCurrency(tower.cost))
            {
                Debug.Log($"Not enough currency to place {tower.towerPrefab.name}. Cost: {tower.cost}");
                return;
            }
            if (tilemapManager == null)
            {
                Debug.LogWarning("TilemapManager is null. Cannot start tower placement.", this);
                return;
            }
            tilemapManager.StartPlacingTower(tower.towerPrefab, tower.cost);
            Debug.Log($"Started placing {tower.towerPrefab.name}");
            if (isPanelOpen)
            {
                ToggleShopPanel(); // Close panel after selection
            }
        }
        else
        {
            Debug.LogWarning($"Invalid tower index: {index}");
        }
    }

    private void UpdateCostLabels()
    {
        for (int i = 0; i < costLabels.Length && i < towers.Length; i++)
        {
            if (costLabels[i] != null)
            {
                costLabels[i].text = $"{towers[i].cost}";
            }
            else
            {
                Debug.LogWarning($"CostLabel at index {i} is null in Shop.", this);
            }
        }
    }
}