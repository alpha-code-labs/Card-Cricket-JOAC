using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldIntraction : MonoBehaviour
{
    public IntractionType intractionType;
    public Locations location;
    public void OnIntraction()
    {
        // Debug.Log("Intraction");
        ConfirmIntraction();
    }
    void ConfirmIntraction()
    {
        // Debug.Log("Confirm Intraction");
        ContinueIntraction();
    }
    void ContinueIntraction()
    {
        if (intractionType == IntractionType.GoToLocation)
        {
            LocationSwitcher.instance.SwitchLocation(location);
        }
        Debug.Log("Continue Intraction");
    }
}
public enum IntractionType
{
    GoToLocation,
}
public enum Locations
{
    Home,
    Map,
    Shop,
    Inventory,
    Profile,
}