using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    [SerializeField] Locations location;
    public override void OnClick()
    {
        WorldIntractionDialougeManager.instance.StartGoToDialogue(location);
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