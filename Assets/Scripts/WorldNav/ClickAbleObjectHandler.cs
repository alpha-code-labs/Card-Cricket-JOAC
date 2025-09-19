using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickAbleObjectHandler : MonoBehaviour
{
    void Start()
    {
        RefreshCollider();
    }
    //This Object is to handle the click events on the objects in the world
    //This script is attached to a 2D Box Collider
    // [SerializeField] UnityEvent onClickAction;
    private void OnMouseDown()
    {
        Debug.Log("Clicked on " + gameObject.name);
        if (!PanAndZoomManager.isSwiping)
        {
            // onClickAction?.Invoke();
            GetComponent<WorldIntraction>()?.OnIntraction();
        }
    }
    void RefreshCollider()
    {
        Collider2D Collider2D = GetComponent<Collider2D>();
        Collider2D.enabled = false;
        Collider2D.enabled = true;
    }
    void OnEnable()
    {
        LocationSwitcher.AreaSwitched += RefreshCollider;
    }
    void OnDisable()
    {
        LocationSwitcher.AreaSwitched -= RefreshCollider;
    }
}
