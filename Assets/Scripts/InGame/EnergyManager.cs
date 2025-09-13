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

    [SerializeField] TextMeshProUGUI EnergyText; // Text to display current energy
    private int energy = 10;
    public void DecreaseEnergy(int amount)
    {
        energy -= amount;
        if (energy < 0)
        {
            energy = 0; // Prevent negative energy
            Debug.LogWarning("Energy cannot be negative. Resetting to 0.");
        }
        RefreshEnergyText();
    }
    public void IncreaseEnergy(int amount)
    {
        RefreshEnergyText();
        energy += amount;
    }
    public int GetEnergy()
    {
        return energy;
    }
    public void RefreshEnergyText()
    {
        EnergyText.text = "Energy: " + energy.ToString() + " / 10"; // Display current energy and max energy
    }
}
