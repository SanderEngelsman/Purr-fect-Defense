using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float startingCurrency = 100f;
    [SerializeField] private float startingBaseHealth = 100f;
    [SerializeField] private TextMeshProUGUI currencyLabel;
    [SerializeField] private TextMeshProUGUI baseHealthLabel;
    [SerializeField] private GameEndManager gameEndManager;
    [SerializeField] private SpriteRenderer baseSpriteRenderer; // Base sprite
    [SerializeField] private Sprite[] baseHealthSprites; // Sprites for 100, 90, ..., 10 HP
    private float currency;
    private float baseHealth;

    private void OnValidate()
    {
        if (currencyLabel == null)
            Debug.LogWarning("CurrencyLabel not assigned in GameManager.", this);
        if (baseHealthLabel == null)
            Debug.LogWarning("BaseHealthLabel not assigned in GameManager.", this);
        if (gameEndManager == null)
            Debug.LogWarning("GameEndManager not assigned in GameManager.", this);
        if (baseSpriteRenderer == null)
            Debug.LogWarning("BaseSpriteRenderer not assigned in GameManager.", this);
        if (baseHealthSprites == null || baseHealthSprites.Length != 10)
            Debug.LogWarning("BaseHealthSprites array should contain 10 sprites (100 to 10 HP).", this);
    }

    private void Start()
    {
        currency = startingCurrency;
        baseHealth = startingBaseHealth;
        UpdateCurrencyLabel();
        UpdateBaseHealthLabel();
        UpdateBaseSprite(); // Set initial sprite
    }

    public void AddCurrency(float amount)
    {
        currency += amount;
        UpdateCurrencyLabel();
    }

    public bool RemoveCurrency(float amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            UpdateCurrencyLabel();
            return true;
        }
        return false;
    }

    public bool HasEnoughCurrency(float amount)
    {
        return currency >= amount;
    }

    public void TakeBaseDamage(float damage)
    {
        baseHealth -= damage;
        UpdateBaseHealthLabel();
        UpdateBaseSprite(); // Update sprite based on health
        if (baseHealth <= 0)
        {
            GameOver();
        }
    }

    private void UpdateCurrencyLabel()
    {
        if (currencyLabel != null)
        {
            currencyLabel.text = Mathf.FloorToInt(currency).ToString();
            Debug.Log($"Currency UI updated: {currencyLabel.text}", this);
        }
        else
        {
            Debug.LogWarning("CurrencyLabel is null in GameManager.", this);
        }
    }

    private void UpdateBaseHealthLabel()
    {
        if (baseHealthLabel != null)
        {
            baseHealthLabel.text = Mathf.FloorToInt(baseHealth).ToString();
            Debug.Log($"Health UI updated: {baseHealthLabel.text}", this);
        }
        else
        {
            Debug.LogWarning("BaseHealthLabel is null in GameManager.", this);
        }
    }

    private void UpdateBaseSprite()
    {
        if (baseSpriteRenderer == null || baseHealthSprites == null || baseHealthSprites.Length != 10)
            return;

        int health = Mathf.FloorToInt(baseHealth);
        int index;

        // Map health to sprite index: 100 -> 0, 90 -> 1, ..., 10 -> 9
        if (health > 90)
            index = 0; // 100 HP
        else if (health > 80)
            index = 1; // 90 HP
        else if (health > 70)
            index = 2; // 80 HP
        else if (health > 60)
            index = 3; // 70 HP
        else if (health > 50)
            index = 4; // 60 HP
        else if (health > 40)
            index = 5; // 50 HP
        else if (health > 30)
            index = 6; // 40 HP
        else if (health > 20)
            index = 7; // 30 HP
        else if (health > 10)
            index = 8; // 20 HP
        else
            index = 9; // 10 HP or less

        baseSpriteRenderer.sprite = baseHealthSprites[index];
        Debug.Log($"Base sprite updated to index {index} for health {health}", this);
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        if (gameEndManager != null)
        {
            gameEndManager.TriggerLoseScreen();
        }
        else
        {
            Debug.LogWarning("GameEndManager is null in GameManager.", this);
        }
        Time.timeScale = 0;
    }
}