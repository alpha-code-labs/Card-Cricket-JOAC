using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    public Locations location;
    public override void OnClick()
    {
        Debug.Log("Clicked on " + gameObject.name);
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