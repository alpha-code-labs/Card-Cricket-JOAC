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
        WorldIntractionDialougeManager.instance.StartConfirmationDialogue(GetYesChoice(), "No, stay here", OnConfirmed);
    }
    string GetYesChoice()
    {
        string locationName = AddSpacesToEnum(location.ToString());
        return "Yes, go to " + locationName;
    }
    void OnConfirmed()
    {
        LocationSwitcher.instance.SwitchLocation(location);
    }
    public static string AddSpacesToEnum(string enumName)
    {
        return Regex.Replace(enumName, "([a-z])([A-Z])", "$1 $2");
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
    MapSprite = 13 // Special case for map sprite
}