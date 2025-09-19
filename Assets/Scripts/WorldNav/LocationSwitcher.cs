using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LocationSwitcher : MonoBehaviour
{
    public static LocationSwitcher instance;
    void Awake()
    {
        instance = this;
    }
    public List<GameObject> locations; // Assign location GameObjects in the Inspector
    public void SwitchLocation(Locations location)
    {
        foreach (var loc in locations)
        {
            loc.SetActive(false); // Deactivate all locations
        }
        locations[(int)location].SetActive(true); // Activate the selected location
    }
}
