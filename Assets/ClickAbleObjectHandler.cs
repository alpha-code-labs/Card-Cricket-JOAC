using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickAbleObjectHandler : MonoBehaviour
{
    //This Object is to handle the click events on the objects in the world
    //This script is attached to a 2D Box Collider
    [SerializeField] UnityEvent onClickAction;
    private void OnMouseDown()
    {
        if (!PanAndZoomManager.isSwiping)
        {
            onClickAction?.Invoke();
        }
    }
}
