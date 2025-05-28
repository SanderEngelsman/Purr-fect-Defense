using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct ShopTowerData
{
    public GameObject towerPrefab;
    public float cost;
}

public class Shop : MonoBehaviour
{
    [SerializeField] private ShopTowerData[] towers;
    [SerializeField] private TextMeshProUGUI[] costLabels;
    [SerializeField] private Button shopButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button[] towerButtons;
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
        if (index >= 0 && index < towers.Length)
        {
            ShopTowerData tower = towers[index];
            if (gameManager.HasEnoughCurrency(tower.cost))
            {
                tilemapManager.StartPlacingTower(tower.towerPrefab, tower.cost);
                if (isPanelOpen)
                {
                    ToggleShopPanel(); // Close panel after selection
                }
            }
            else
            {
                Debug.Log($"Not enough currency to place {tower.towerPrefab.name}. Cost: {tower.cost}");
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