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
    public Dictionary<Locations, GameObject> locationDictionary = new Dictionary<Locations, GameObject>();
    void Start()
    {
        BuildLocationDictionary();
    }
    void BuildLocationDictionary()
    {
        for (int i = 0; i < locations.Count; i++)
        {
            Locations enumKey = (Locations)(int)-1;
            if (!System.Enum.TryParse(locations[i].name, out enumKey))
            {
                Debug.LogError($"Failed to parse location name '{locations[i].name}' to Locations enum.");
                continue;
            }
            Debug.Log($"Mapping location '{locations[i].name}' to enum '{enumKey}'");

            locationDictionary.Add(enumKey, locations[i]);
        }
    }
    public void SwitchLocation(Locations location)
    {
        foreach (var loc in locations)
        {
            loc.SetActive(false); // Deactivate all locations
        }
        GameObject SwitchToThisLocation = locationDictionary[location];
        if (SwitchToThisLocation == null)
        {
            Debug.LogError($"Location '{location}' not found in dictionary.");
            return;
        }
        else
        {
            SwitchToThisLocation.SetActive(true);
            Debug.Log($"Switched to location: {location}");
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Add UI Button for this -Escape Key Pressed - Switching to MapSprite");
            SwitchLocation(Locations.MapSprite);
        }
    }
}
