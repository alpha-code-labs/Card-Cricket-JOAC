using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkToLocationIntractionHandler : ClickAbleObjectHandler
{
    // public Locations location;
    public override void OnClick()
    {
        Debug.Log("Clicked on " + gameObject.name);
        // LocationSwitcher.instance.SwitchLocation(location);
    }
}