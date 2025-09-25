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
    
    [System.Serializable]
    public class HighlightData
    {
        public GameObject target;           // The UI element being highlighted
        public GameObject overlay;          // The semi-transparent overlay behind it
        public Transform originalParent;    // Original parent in hierarchy
        public int originalSiblingIndex;    // Original sibling order
        public Vector3 originalScale;       // Original scale
        public Tweener pulseTween;          // DOTween instance for the pulsing animation
    }
    private Dictionary<GameObject, HighlightData> highlights = new Dictionary<GameObject, HighlightData>();

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

    // 👉 Check if this object is already highlighted
    if (highlights.ContainsKey(targetObject))
    {
        // Already highlighted: do nothing and leave the existing highlight intact
        return;
    }

    // Prepare new highlight entry
    HighlightData data = new HighlightData
    {
        target = targetObject,
        originalParent = targetObject.transform.parent,
        originalSiblingIndex = targetObject.transform.GetSiblingIndex(),
        originalScale = targetObject.transform.localScale
    };

    // Create overlay for this specific object
    // data.overlay = CreateOverlay(data, allowClickToDismiss);

    // Move target above overlay so it stays visible
    targetObject.transform.SetAsLastSibling();

    // Start pulse animation
    data.pulseTween = StartPulseAnimation(targetObject, data.originalScale);

    // Store in dictionary so it can be cleared individually later
    highlights[targetObject] = data;
}

    private Tweener StartPulseAnimation(GameObject targetObject, Vector3 originalScale)
    {
        return targetObject.transform
            .DOScale(originalScale * settings.pulseScale, settings.pulseDuration)
            .SetEase(settings.pulseEase)
            .SetLoops(-1, LoopType.Yoyo);
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
        List<GameObject> keys = new List<GameObject>(highlights.Keys);
        foreach (GameObject obj in keys)
        {
            ClearHighlightFromObj(obj);
        }
    }

    public void ClearHighlightFromObj(GameObject targetObject)
    {
        if (!highlights.ContainsKey(targetObject))
            return;

        HighlightData data = highlights[targetObject];

        // Kill animation
        if (data.pulseTween != null)
        {
            data.pulseTween.Kill();
        }

        // Reset transform
        if (data.target != null)
        {
            data.target.transform.SetSiblingIndex(data.originalSiblingIndex);
            data.target.transform.DOScale(data.originalScale, 0.2f);
        }

        // Fade out and destroy overlay
        if (data.overlay != null)
        {
            Image overlayImage = data.overlay.GetComponent<Image>();
            overlayImage.DOFade(0, settings.fadeInDuration * 0.5f)
                .OnComplete(() => Destroy(data.overlay));
        }

        highlights.Remove(targetObject);
    }

    
    void OnDestroy()
    {
        ClearHighlight();
    }
}