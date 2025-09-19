using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    public Locations location;
    public override void OnClick()
    {
        WorldIntractionDialougeManager.instance.location = location;
        WorldIntractionDialougeManager.instance.dialogueRunner.StartDialogue("ConfirmLocation");
    }
    [YarnCommand("confirm_location")]
    public static void ConfirmLocation(bool userConfirmed)
    {
        if (userConfirmed)
        {
            LocationSwitcher.instance.SwitchLocation(WorldIntractionDialougeManager.instance.location);
        }
        else
        {
            Debug.Log("User cancelled location switch.");
        }
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