using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickAbleObjectHandler : MonoBehaviour
{
    public GameObject availabilityIndicatorPrefab;
    protected void Start()
    {
        RefreshCollider();
    }
    private void OnMouseDown()
    {
        if (WorldIntractionDialougeManager.instance.IsDialogueCurrentlyRunning()) return;
        Debug.Log("Clicked on " + gameObject.name);
        if (!PanAndZoomManager.isSwiping)
        {
            OnClick();
        }
    }
    // Hover Effect
    private void OnMouseEnter()
    {
        if (WorldIntractionDialougeManager.instance.IsDialogueCurrentlyRunning()) return;
        availabilityIndicatorPrefab.transform.localScale = Vector3.one * 1.2f;
    }
    private void OnMouseExit()
    {
        availabilityIndicatorPrefab.transform.localScale = Vector3.one * 0.8f;
    }
    public abstract void OnClick();
    protected void RefreshCollider()
    {
        Collider2D Collider2D = GetComponent<Collider2D>();
        Collider2D.enabled = false;
        Collider2D.enabled = true;
    }
    protected void CheckAvailability()
    {
        // if (WorldIntractionDialougeManager.instance.IsDialogueCurrentlyRunning()) return;
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
