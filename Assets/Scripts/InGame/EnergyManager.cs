using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        RefreshEnergyText();
    }

    [SerializeField] TextMeshProUGUI EnergyText; // Text to display current energy
    private int energy = 24;
    private const int maxEnergy = 24;
    void DecreaseEnergy(int amount)
    {
        energy -= amount;
        if (energy < 0)
        {
            energy = 24; // Prevent negative energy
            ScoreManager.Instance.LooseWicket();
            Debug.LogWarning("Energy dropped below zero. Resetting to 24 and losing a wicket.");
        }
        RefreshEnergyText();
    }
    void IncreaseEnergy(int amount)
    {
        energy += amount;
        if (energy > maxEnergy)
        {
            energy = maxEnergy; // Cap energy at 10
            Debug.LogWarning("Energy cannot exceed 10. Capping to 10.");
        }
        RefreshEnergyText();
    }
    public void HandelEnergyChange(int amount)// Positive amount to increase, negative to decrease
    {
        Debug.Log("Handling energy change: " + amount);
        if (amount > 0)
        {
            DecreaseEnergy(amount);
        }
        else
        {
            Debug.Log("Increasing energy by: " + (-amount));
            IncreaseEnergy(-amount);
        }
    }
    public int GetEnergy()
    {
        return energy;
    }
    public void RefreshEnergyText()
    {
        EnergyText.text = "Energy: " + energy.ToString() + " / " + maxEnergy; // Display current energy and max energy
    }
}
