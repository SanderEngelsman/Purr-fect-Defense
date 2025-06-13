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
    [SerializeField] private GameEndManager gameEndManager; // Added reference
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
    }

    private void Start()
    {
        currency = startingCurrency;
        baseHealth = startingBaseHealth;
        UpdateCurrencyLabel();
        UpdateBaseHealthLabel();
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