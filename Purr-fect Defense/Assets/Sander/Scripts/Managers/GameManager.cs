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
    private float currency;
    private float baseHealth;

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
            currencyLabel.text = $"Currency: {currency}";
        }
    }

    private void UpdateBaseHealthLabel()
    {
        if (baseHealthLabel != null)
        {
            baseHealthLabel.text = $"Base Health: {baseHealth}";
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0;
    }
}
