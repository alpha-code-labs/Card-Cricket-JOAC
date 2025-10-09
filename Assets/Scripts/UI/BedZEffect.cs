using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Yarn.Unity;

//Put on a Canvas
public class BedZEffect : MonoBehaviour
{
    public static BedZEffect instance;
    [Header("Z Sprite Settings")]
    [SerializeField] private Sprite zSprite;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int maxZSprites = 5;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private float moveDistance = 200f;
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Snoring Pattern")]
    [SerializeField] private bool isSnoring = false;
    [SerializeField] private float snoringDelay = 2f;

    private List<GameObject> activeZSprites = new List<GameObject>();
    private Coroutine snoringCoroutine;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        if (zSprite == null)
        {
            Debug.LogWarning("Z Sprite is not assigned!");
        }
    }
    [YarnCommand("start_snoring")]
    public static void StartSnoring()
    {
        if (instance == null) return;
        if (!instance.isSnoring && instance.zSprite != null)
        {
            instance.isSnoring = true;
            instance.snoringCoroutine = instance.StartCoroutine(instance.SnoringLoop());
        }
    }

    [YarnCommand("stop_snoring")]
    public static void StopSnoring()
    {
        if (instance == null) return;
        instance.isSnoring = false;
        if (instance.snoringCoroutine != null)
        {
            instance.StopCoroutine(instance.snoringCoroutine);
            instance.snoringCoroutine = null;
        }

        // Clean up existing Z sprites
        foreach (GameObject zObj in instance.activeZSprites)
        {
            if (zObj != null)
            {
                zObj.transform.DOKill();
                Object.Destroy(zObj);
            }
        }
        instance.activeZSprites.Clear();
    }

    private IEnumerator SnoringLoop()
    {
        yield return new WaitForSeconds(snoringDelay);

        while (isSnoring)
        {
            if (activeZSprites.Count < maxZSprites)
            {
                CreateZSprite();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void CreateZSprite()
    {
        // Create a new GameObject for the Z sprite
        GameObject zObject = new GameObject("Z_Sprite");
        zObject.transform.SetParent(transform, false);

        // Add Image component and set the sprite
        Image zImage = zObject.AddComponent<Image>();
        zImage.sprite = zSprite;
        zImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent

        // Set initial position
        RectTransform rectTransform = zObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = spawnPoint.localPosition;
        rectTransform.localScale = Vector3.zero;

        // Add to active list
        activeZSprites.Add(zObject);

        // Calculate positions with random drift from the start
        float randomDrift = Random.Range(-50f, 50f);
        Vector2 endPosition = (Vector2)spawnPoint.localPosition + Vector2.up * moveDistance + Vector2.right * randomDrift;

        // Create animation sequence
        Sequence zSequence = DOTween.Sequence();

        // Fade in and scale up
        zSequence.Append(zImage.DOFade(1f, 0.3f));
        zSequence.Join(rectTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

        // Move up with drift and scale - all happening simultaneously during the main duration
        zSequence.Append(rectTransform.DOAnchorPos(endPosition, animationDuration).SetEase(moveCurve));
        zSequence.Join(rectTransform.DOScale(Vector3.one * scaleMultiplier, animationDuration));

        // Start fading out before the movement completes for a smoother effect
        zSequence.Insert(animationDuration * 0.7f + 0.3f, zImage.DOFade(0f, animationDuration * 0.3f + 0.2f));

        // Clean up when animation completes
        zSequence.OnComplete(() =>
        {
            activeZSprites.Remove(zObject);
            if (zObject != null)
                Destroy(zObject);
        });
    }

    void OnDestroy()
    {
        StopSnoring();
    }
}
