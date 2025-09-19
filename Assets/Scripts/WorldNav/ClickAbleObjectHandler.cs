using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ClickAbleObjectHandler : MonoBehaviour
{
    protected void Start()
    {
        RefreshCollider();
        CreateAvailabilityIndicator();
    }
    private void CreateAvailabilityIndicator()
    {
        GameObject availabilityIndicator = new GameObject("AvailabilityIndicator");
        availabilityIndicator.transform.SetParent(transform);
        availabilityIndicator.transform.localPosition = Vector3.zero;
        availabilityIndicator.transform.localScale = Vector3.one * 0.1f; // Scale down the indicator
        SpriteRenderer sr = availabilityIndicator.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Textures/AvailabilityIndicator");
        sr.sortingOrder = 100; // Ensure it's rendered above other objects
        sr.color = new Color(1, 1, 1, 0.5f); // Semi-transparent
        availabilityIndicator.SetActive(true); // Initially hidden
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
