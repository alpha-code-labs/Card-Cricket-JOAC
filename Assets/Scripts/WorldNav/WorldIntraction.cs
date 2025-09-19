using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldIntraction : ClickAbleObjectHandler
{
    public IntractionType intractionType;
    public Locations location;
    public override void OnClick()
    {
        Debug.Log("Clicked on " + gameObject.name);
        ContinueIntraction();
    }
    void ContinueIntraction()
    {
        if (intractionType == IntractionType.GoToLocation)
        {
            LocationSwitcher.instance.SwitchLocation(location);
        }
    }
}
public enum IntractionType
{
    GoToLocation,
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