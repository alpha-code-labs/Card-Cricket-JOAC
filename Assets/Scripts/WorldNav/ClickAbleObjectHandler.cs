using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickAbleObjectHandler : MonoBehaviour
{
    protected void Start()
    {
        RefreshCollider();
    }
    private void OnMouseDown()
    {
        Debug.Log("Clicked on " + gameObject.name);
        if (!PanAndZoomManager.isSwiping)
        {
            OnClick();
        }
    }
    public abstract void OnClick();
    protected void RefreshCollider()
    {
        Collider2D Collider2D = GetComponent<Collider2D>();
        Collider2D.enabled = false;
        Collider2D.enabled = true;
    }
    protected void OnEnable()
    {
        LocationSwitcher.AreaSwitched += RefreshCollider;
    }
    protected void OnDisable()
    {
        LocationSwitcher.AreaSwitched -= RefreshCollider;
    }
}
