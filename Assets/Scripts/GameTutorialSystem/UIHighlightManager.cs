using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class UIHighlightManager : MonoBehaviour
{
    [System.Serializable]
    public class HighlightSettings
    {
        [Header("Pulse Animation")]
        [Range(0.1f, 2f)]
        public float pulseDuration = 0.5f;
        
        [Range(1f, 1.5f)]
        public float pulseScale = 1.1f;
        
        public Ease pulseEase = Ease.InOutSine;
        
        [Header("Overlay")]
        public Color overlayColor = new Color(0, 0, 0, 0.5f);
        
        [Range(0.1f, 1f)]
        public float fadeInDuration = 0.3f;
    }
    
    [Header("Configuration")]
    public HighlightSettings settings = new HighlightSettings();
    
    // Private runtime variables
    private GameObject currentOverlay;
    private GameObject currentHighlightedObject;
    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalScale;
    private Tweener pulseTween;
    
    private static UIHighlightManager instance;
    public static UIHighlightManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<UIHighlightManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        instance = this;
    }
    
    public void HighlightObject(GameObject targetObject, bool allowClickToDismiss = true)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Cannot highlight null object!");
            return;
        }
        
        // Clear any existing highlight
        ClearHighlight();
        
        currentHighlightedObject = targetObject;
        
        // Store original values
        originalParent = targetObject.transform.parent;
        originalSiblingIndex = targetObject.transform.GetSiblingIndex();
        originalScale = targetObject.transform.localScale;
        
        // Create overlay BEFORE moving the target
        CreateOverlay(allowClickToDismiss);
        
        // Move target object to be AFTER the overlay in hierarchy
        targetObject.transform.SetAsLastSibling();
        
        // Start pulse animation
        StartPulseAnimation(targetObject);
    }
    
    void CreateOverlay(bool allowClickToDismiss)
    {
        // Create overlay in the same parent as the target object
        GameObject overlayGO = new GameObject("HighlightOverlay");
        overlayGO.transform.SetParent(originalParent, false);
        
        // Add RectTransform and Image
        RectTransform overlayRect = overlayGO.AddComponent<RectTransform>();
        Image overlayImage = overlayGO.AddComponent<Image>();
        
        // Make it cover the entire parent area
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;
        
        // Set overlay color
        overlayImage.color = new Color(
            settings.overlayColor.r,
            settings.overlayColor.g,
            settings.overlayColor.b,
            0
        );
        overlayImage.raycastTarget = allowClickToDismiss;
        
        // Place overlay at the original position of the target
        overlayGO.transform.SetSiblingIndex(originalSiblingIndex);
        
        // Fade in
        overlayImage.DOFade(settings.overlayColor.a, settings.fadeInDuration);
        
        // Add click handler if enabled
        if (allowClickToDismiss)
        {
            Button button = overlayGO.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => ClearHighlight());
        }
        
        currentOverlay = overlayGO;
    }
    
    void StartPulseAnimation(GameObject targetObject)
    {
        if (pulseTween != null)
            pulseTween.Kill();
        
        // Pulse animation
        pulseTween = targetObject.transform
            .DOScale(originalScale * settings.pulseScale, settings.pulseDuration)
            .SetEase(settings.pulseEase)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    public void ClearHighlight()
    {
        // Kill animation
        if (pulseTween != null)
        {
            pulseTween.Kill();
            pulseTween = null;
        }
        
        // Reset object position and scale
        if (currentHighlightedObject != null)
        {
            currentHighlightedObject.transform.SetSiblingIndex(originalSiblingIndex);
            currentHighlightedObject.transform.DOScale(originalScale, 0.2f);
        }
        
        // Fade out and destroy overlay
        if (currentOverlay != null)
        {
            Image overlayImage = currentOverlay.GetComponent<Image>();
            overlayImage.DOFade(0, settings.fadeInDuration * 0.5f)
                .OnComplete(() => {
                    if (currentOverlay != null)
                        Destroy(currentOverlay);
                });
        }
        
        currentHighlightedObject = null;
    }
    
    void OnDestroy()
    {
        ClearHighlight();
    }
}