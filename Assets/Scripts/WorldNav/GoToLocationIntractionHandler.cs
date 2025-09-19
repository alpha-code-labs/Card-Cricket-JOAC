using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class GoToLocationIntractionHandler : ClickAbleObjectHandler
{
    public static bool selectingLocation = false;
    public Locations location;
    public override void OnClick()
    {
        // LocationSwitcher.instance.SwitchLocation(location);
        if (selectingLocation) return;
        selectingLocation = true;
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
        selectingLocation = false;
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