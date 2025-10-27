using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Yarn.Unity;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    [SerializeField] Locations location;
    public override void OnClick()
    {
        WorldIntractionDialougeManager.instance.StartConfirmationDialogue("Yes, go to " + GetLocationString(), "No, stay here", OnConfirmed);
    }
    public override void CheckAvaliability()
    {
        return; // Always available
    }
    void OnConfirmed()
    {
        LocationSwitcher.instance.SwitchLocation(location);
    }

    public override void OnHoveringTip()
    {
        CurrencyToolTip.instance.ShowToolTip("Go to " + GetLocationString());
    }
    string GetLocationString()
    {
        string locationName = PrettyStrings.GetPrettyEnumString(location.ToString());
        switch (location)
        {
            case Locations.MapSprite:
                locationName = "the Map";
                break;
            case Locations.HutInterior:
                locationName = "Home";
                break;
        }
        return locationName;
    }

}
public enum Locations
{
    Inavlid = -1,
    HutInterior = 0,
    KohliwadaGround = 1,
    PublicSchool = 2,
    SportsAcademy = 3,
    WellingtonEstate = 4,
    SurakshaHospital = 5,
    ShivTemple = 6,
    AutoStand = 7,
    GeneralStore = 8,
    FatimaDhaba = 9,
    CobblerShop = 10,
    MunnaTyreShop = 11,
    ChaiStall = 12,
    //
    MapSprite = 13, // Special case for map sprite
    SummitHeightsSchool = 14,
    Library = 15,

}