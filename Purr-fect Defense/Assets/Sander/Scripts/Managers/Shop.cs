using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct ShopTowerData
{
    public GameObject towerPrefab;
    public float cost;
    public Button towerButton; // Existing button
    public TextMeshProUGUI costLabel; // Existing cost label
    public Sprite towerSprite; // Sprite for tower image
    public string description; // Tower description
    public float resellPrice; // Refund amount when sold
}

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button shopButton;
    [SerializeField] public ShopTowerData[] Towers; // Made public
    [SerializeField] private Transform contentParent; // ScrollView's Content RectTransform
    private GameManager gameManager;
    private TilemapManager tilemapManager;
    private ShopPanelAnimator shopPanelAnimator;
    private bool isOpen = false;

    private void OnValidate()
    {
        if (shopPanel == null)
            Debug.LogWarning("ShopPanel is not assigned in Shop.", this);
        if (shopButton == null)
            Debug.LogWarning("ShopButton is not assigned in Shop.", this);
        if (contentParent == null)
            Debug.LogWarning("ContentParent is not assigned in Shop.", this);
        if (Towers == null || Towers.Length == 0)
            Debug.LogWarning("Towers array is empty or null in Shop.", this);
        foreach (var data in Towers)
        {
            if (data.towerButton == null)
                Debug.LogWarning("TowerButton is not assigned in ShopTowerData.", this);
            if (data.costLabel == null)
                Debug.LogWarning("CostLabel is not assigned in ShopTowerData.", this);
            if (data.resellPrice < 0)
                Debug.LogWarning($"ResellPrice for {data.towerPrefab?.name} is negative.", this);
        }
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        tilemapManager = FindObjectOfType<TilemapManager>();
        shopPanelAnimator = shopPanel.GetComponent<ShopPanelAnimator>();
        if (shopPanelAnimator == null)
        {
            Debug.LogWarning("ShopPanelAnimator is missing on ShopPanel.", shopPanel);
        }
        shopButton.onClick.AddListener(ToggleShop);
        SetupTowerEntries();
        UpdateCosts();
        shopPanel.SetActive(false);
    }

    private void SetupTowerEntries()
    {
        // Clear existing children (if any)
        foreach (Transform child in contentParent)
        {
            if (!IsTowerButton(child)) // Avoid destroying towerButtons
                Destroy(child.gameObject);
        }
        // Setup each tower entry
        for (int i = 0; i < Towers.Length; i++)
        {
            int index = i; // Capture for lambda
            // Create container for image, button, description
            GameObject container = new GameObject($"TowerEntry_{Towers[i].towerPrefab.name}");
            container.transform.SetParent(contentParent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(200, 250); // Adjust as needed
            // Add Vertical Layout Group to container
            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            // Create Image
            GameObject imageObj = new GameObject("TowerImage");
            imageObj.transform.SetParent(container.transform, false);
            Image towerImage = imageObj.AddComponent<Image>();
            towerImage.sprite = Towers[i].towerSprite;
            towerImage.preserveAspect = true;
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(100, 100);
            // Reparent existing button
            Towers[i].towerButton.transform.SetParent(container.transform, false);
            Towers[i].towerButton.onClick.RemoveAllListeners();
            Towers[i].towerButton.onClick.AddListener(() => SelectTower(index));
            // Create Description
            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI descriptionText = descObj.AddComponent<TextMeshProUGUI>();
            descriptionText.text = Towers[i].description;
            descriptionText.fontSize = 16;
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.enableWordWrapping = true;
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(180, 80);
        }
        UpdateCosts();
    }

    private bool IsTowerButton(Transform child)
    {
        foreach (var data in Towers)
        {
            if (child != null && data.towerButton != null && data.towerButton.transform == child)
                return true;
        }
        return false;
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopPanel.SetActive(true); // Always activate for navigation
        if (isOpen)
        {
            shopPanelAnimator?.OpenPanel();
            tilemapManager.CancelPlacement();
            UpdateCosts();
            Debug.Log("Shop opened.", this);
        }
        else
        {
            shopPanelAnimator?.ClosePanel();
            tilemapManager.CancelPlacement();
            Debug.Log("Shop closed.", this);
        }
    }

    public void SelectTower(int index)
    {
        if (!gameManager.HasEnoughCurrency(Towers[index].cost))
        {
            Debug.Log($"Not enough currency to select {Towers[index].towerPrefab.name}. Required: {Towers[index].cost}", this);
            return;
        }
        tilemapManager.StartPlacingTower(Towers[index].towerPrefab, Towers[index].cost);
        ToggleShop();
        Debug.Log($"Selected tower: {Towers[index].towerPrefab.name}, Cost: {Towers[index].cost}", this);
    }

    public void UpdateCosts()
    {
        foreach (var data in Towers)
        {
            if (data.costLabel != null)
            {
                data.costLabel.text = data.cost.ToString(); // Update cost text
                bool canAfford = gameManager.HasEnoughCurrency(data.cost);
                data.costLabel.color = canAfford ? Color.white : Color.red;
            }
        }
    }
}