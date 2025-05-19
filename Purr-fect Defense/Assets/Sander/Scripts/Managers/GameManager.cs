using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float startingCurrency = 100f;
    [SerializeField] private float startingBaseHealth = 100f;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI baseHealthText;

    private float currency;
    private float baseHealth;

    private void Start()
    {
        currency = startingCurrency;
        baseHealth = startingBaseHealth;
        UpdateUI();
    }

    public void AddCurrency(float amount)
    {
        currency += amount;
        UpdateUI();
    }

    public bool SpendCurrency(float amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void TakeBaseDamage(float damage)
    {
        baseHealth -= damage;
        UpdateUI();
        if (baseHealth <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
        if (currencyText != null)
            currencyText.text = "Currency: " + currency.ToString("F0");
        if (baseHealthText != null)
            baseHealthText.text = "Base Health: " + baseHealth.ToString("F0");
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0;
    }
}
