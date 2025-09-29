using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Simplified manager that works with your existing AttackCardProps
public class SimpleHandArcManager : MonoBehaviour
{
    [Header("Arc Configuration")]
    [SerializeField] private float arcRadius = 800f;
    [SerializeField] private float arcAngle = 30f;
    [SerializeField] private float maxArcAngle = 60f;
    [SerializeField] private Vector3 arcCenterOffset = new Vector3(0, -400, 0);

    [Header("Card Hover Settings")]
    [SerializeField] private float hoverScale = 1.3f;
    [SerializeField] private float hoverYOffset = 50f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float neighborSeparation = 40f;
    [SerializeField] private float centerHoverYOffset = 10f; // Min Y offset for center cards
    [SerializeField] private float minHoverYOffset = 10f; // Minimum Y offset for center cards
    [SerializeField] private bool useDynamicYOffset = true; // Enable dynamic Y offset based on position


    [Header("Animation")]
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private float hoverAnimSpeed = 12f;

    [Header("Visual Effects")]
    [SerializeField] private Color hoverGlowColor = new Color(1f, 0.9f, 0.3f, 1f);
    [SerializeField] private bool useOutlineEffect = true;
    [SerializeField] private bool useShadowEffect = true;

    [Header("Distance-Based Spread Settings")]
    [SerializeField] private float neighbor1Push = 1.0f;  // 100% for direct neighbors
    [SerializeField] private float neighbor2Push = 0.6f;  // 60% for second neighbors
    [SerializeField] private float neighbor3Push = 0.3f;  // 30% for third neighbors
    [SerializeField] private float neighborFarPush = 0.1f; // 10% for further cards
    [SerializeField] private bool useRotationEffect = true;
    [SerializeField] private float rotationMultiplier = 5f;

    private Dictionary<GameObject, CardInfo> cardInfos = new Dictionary<GameObject, CardInfo>();
    private GameObject currentHoveredCard = null;
    
    private class CardInfo
    {
        public RectTransform rectTransform;
        public Canvas canvas;
        public Vector3 targetPosition;
        public Vector3 basePosition;
        public Quaternion targetRotation;
        public Quaternion baseRotation;
        public Vector3 targetScale;
        public int index;
        public int baseSortingOrder;
        public Outline outline;
        public Shadow shadow;
    }

    void Start()
    {
        RemoveLayoutGroups();
        StartCoroutine(InitialSetup());
    }

    void RemoveLayoutGroups()
    {
        // Remove all layout components
        HorizontalLayoutGroup hLayout = GetComponent<HorizontalLayoutGroup>();
        if (hLayout != null) DestroyImmediate(hLayout);

        VerticalLayoutGroup vLayout = GetComponent<VerticalLayoutGroup>();
        if (vLayout != null) DestroyImmediate(vLayout);

        GridLayoutGroup gLayout = GetComponent<GridLayoutGroup>();
        if (gLayout != null) DestroyImmediate(gLayout);

        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter != null) DestroyImmediate(fitter);
    }

    IEnumerator InitialSetup()
    {
        yield return new WaitForSeconds(0.1f);
        RefreshCardArrangement();
    }

    void Update()
    {
        // Animate all cards
        foreach (var kvp in cardInfos)
        {
            if (kvp.Key == null)
                continue;

            CardInfo info = kvp.Value;

            // Smooth position
            info.rectTransform.anchoredPosition = Vector3.Lerp(
                info.rectTransform.anchoredPosition,
                info.targetPosition,
                Time.deltaTime * animationSpeed
            );

            // Smooth rotation
            info.rectTransform.rotation = Quaternion.Lerp(
                info.rectTransform.rotation,
                info.targetRotation,
                Time.deltaTime * animationSpeed
            );

            // Smooth scale
            info.rectTransform.localScale = Vector3.Lerp(
                info.rectTransform.localScale,
                info.targetScale,
                Time.deltaTime * hoverAnimSpeed
            );
        }

        // Check if we need to refresh (new cards added/removed)
        int activeChildCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                activeChildCount++;
        }

        if (activeChildCount != cardInfos.Count)
        {
            RefreshCardArrangement();
        }
    }

    public void RefreshCardArrangement()
    {
        // Clear old data
        cardInfos.Clear();

        List<GameObject> activeCards = new List<GameObject>();

        // Collect all active cards
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.activeSelf && child.GetComponent<AttackCardProps>() != null)
            {
                activeCards.Add(child);
            }
        }

        if (activeCards.Count == 0) return;

        // Calculate positions for each card
        int cardCount = activeCards.Count;
        float dynamicArcAngle = Mathf.Min(arcAngle + (cardCount - 3) * 5f, maxArcAngle);
        float angleStep = cardCount > 1 ? dynamicArcAngle / (cardCount - 1) : 0;
        float startAngle = -dynamicArcAngle / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = activeCards[i];
            CardInfo info = new CardInfo();

            info.rectTransform = card.GetComponent<RectTransform>();
            info.index = i;

            // Ensure canvas for sorting
            info.canvas = card.GetComponent<Canvas>();
            if (info.canvas == null)
            {
                info.canvas = card.AddComponent<Canvas>();
                GraphicRaycaster raycaster = card.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                    card.AddComponent<GraphicRaycaster>();
            }
            info.canvas.overrideSorting = true;

            // Calculate arc position (bottom-up arc)
            float angle = startAngle + angleStep * i;
            float radians = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(radians) * arcRadius;
            float y = Mathf.Cos(radians) * arcRadius - arcRadius; // Changed sign for bottom-up arc

            info.basePosition = arcCenterOffset + new Vector3(x, y, 0);
            info.targetPosition = info.basePosition;
            info.baseRotation = Quaternion.Euler(0, 0, -angle * 0.5f);
            info.targetRotation = info.baseRotation;
            info.targetScale = Vector3.one * normalScale;
            info.baseSortingOrder = i * 2 + 3;

            info.canvas.sortingOrder = info.baseSortingOrder;

            cardInfos[card] = info;
        }
    }

    // Called directly from AttackCardProps
    public void OnCardHoverEnterDirect(GameObject card)
    {
        if (!cardInfos.ContainsKey(card)) return;
        if (currentHoveredCard == card) return;

        currentHoveredCard = card;
        CardInfo hoveredInfo = cardInfos[card];

        // Bring to front
        hoveredInfo.canvas.sortingOrder = 1000;

        // Scale and elevate
        hoveredInfo.targetScale = Vector3.one * hoverScale;
        // hoveredInfo.targetPosition = hoveredInfo.basePosition + Vector3.up * hoverYOffset;
        // Calculate dynamic Y offset based on card position
        float dynamicYOffset = CalculateDynamicYOffset(hoveredInfo);
        hoveredInfo.targetPosition = hoveredInfo.basePosition + Vector3.up * dynamicYOffset;
        hoveredInfo.targetRotation = Quaternion.identity; // Straighten

        // Adjust ALL cards based on distance from hovered card
        foreach (var kvp in cardInfos)
        {
            if (kvp.Key == card) continue; // Skip the hovered card itself

            CardInfo otherInfo = kvp.Value;
            int indexDifference = otherInfo.index - hoveredInfo.index;

            // Calculate push amount based on distance from hovered card
            // Cards closer to hovered card get pushed more
            float distance = Mathf.Abs(indexDifference);
            float pushStrength = neighborSeparation;

            if (distance == 1)
            {
                // Direct neighbors
                pushStrength = neighborSeparation * neighbor1Push;
            }
            else if (distance == 2)
            {
                // Second neighbors
                pushStrength = neighborSeparation * neighbor2Push;
            }
            else if (distance == 3)
            {
                // Third neighbors
                pushStrength = neighborSeparation * neighbor3Push;
            }
            else
            {
                // Further cards
                pushStrength = neighborSeparation * neighborFarPush;
            }

            // Apply the push in the correct direction
            float pushDirection = Mathf.Sign(indexDifference);
            otherInfo.targetPosition = otherInfo.basePosition + Vector3.right * (pushStrength * pushDirection);

            // Optional: Slightly rotate cards away from hovered card for more dynamic look
            if (useRotationEffect)
            {
                float rotationAdjust = pushDirection * (rotationMultiplier / (distance + 1));
                otherInfo.targetRotation = Quaternion.Euler(otherInfo.baseRotation.eulerAngles + new Vector3(0, 0, rotationAdjust));
            }
        }

        // Add visual effects
        AddHoverEffects(card);
    }

    public void OnCardHoverExitDirect(GameObject card)
    {
        if (!cardInfos.ContainsKey(card)) return;
        if (currentHoveredCard != card) return;

        currentHoveredCard = null;
        CardInfo hoveredInfo = cardInfos[card];

        // Reset sorting
        hoveredInfo.canvas.sortingOrder = hoveredInfo.baseSortingOrder;

        // Reset transform
        hoveredInfo.targetScale = Vector3.one * normalScale;
        hoveredInfo.targetPosition = hoveredInfo.basePosition;
        hoveredInfo.targetRotation = hoveredInfo.baseRotation;

        // Reset ALL card positions and rotations
        foreach (var kvp in cardInfos)
        {
            CardInfo info = kvp.Value;
            info.targetPosition = info.basePosition;
            info.targetRotation = info.baseRotation;
        }

        // Remove visual effects
        RemoveHoverEffects(card);
    }

    void AddHoverEffects(GameObject card)
    {
        if (!cardInfos.ContainsKey(card)) return;
        CardInfo info = cardInfos[card];

        // Add outline effect
        if (useOutlineEffect)
        {
            if (info.outline == null)
                info.outline = card.AddComponent<Outline>();

            info.outline.effectColor = hoverGlowColor;
            info.outline.effectDistance = new Vector2(3, 3);
            info.outline.enabled = true;
        }

        // Enhance shadow
        if (useShadowEffect)
        {
            if (info.shadow == null)
                info.shadow = card.AddComponent<Shadow>();

            info.shadow.effectColor = new Color(0, 0, 0, 0.8f);
            info.shadow.effectDistance = new Vector2(10, -10);
            info.shadow.enabled = true;
        }
    }

    void RemoveHoverEffects(GameObject card)
    {
        if (!cardInfos.ContainsKey(card)) return;
        CardInfo info = cardInfos[card];

        // Disable outline
        if (info.outline != null)
            info.outline.enabled = false;

        // Reset shadow
        if (info.shadow != null)
        {
            info.shadow.effectDistance = new Vector2(3, -3);
            info.shadow.effectColor = new Color(0, 0, 0, 0.5f);
        }
    }
    
    private float CalculateDynamicYOffset(CardInfo card)
    {
        if (!useDynamicYOffset)
            return hoverYOffset; // Use static offset if dynamic is disabled
        
        int totalCards = cardInfos.Count;
        if (totalCards <= 1)
            return hoverYOffset;
        
        // Calculate normalized position (0 = leftmost, 0.5 = center, 1 = rightmost)
        float normalizedPosition = totalCards > 1 ? (float)card.index / (totalCards - 1) : 0.5f;
        
        // Calculate distance from center (0 = center, 1 = edges)
        float distanceFromCenter = Mathf.Abs(normalizedPosition - 0.5f) * 2f;
        
        // Interpolate Y offset based on position
        // Edge cards get full hoverYOffset, center cards get centerHoverYOffset
       float dynamicOffset = Mathf.Lerp(hoverYOffset, centerHoverYOffset, distanceFromCenter) + minHoverYOffset;
        return dynamicOffset;
    }
}