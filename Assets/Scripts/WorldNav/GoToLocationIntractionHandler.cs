using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    [SerializeField] Locations location;
    public override void OnClick()
    {
        WorldIntractionDialougeManager.instance.StartConfirmationDialogue("Yes, go to " + location.ToString(), "No, stay here", OnConfirmed);
    }
    void OnConfirmed()
    {
        LocationSwitcher.instance.SwitchLocation(location);
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