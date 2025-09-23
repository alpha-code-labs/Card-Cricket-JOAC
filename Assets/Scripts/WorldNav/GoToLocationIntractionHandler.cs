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
    HutInterior,
    KohliwadaGround,
    PublicSchool,
    SportsAcademy,
    WellingtonEstate,
    SurakshaHospital,
    ShivTemple,
    AutoStand,
    GeneralStore,
    FatimaDhaba,
    CobblerShop,
    MunnaTyreShop,
    ChaiStall,
    //
    MapSprite
}