using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardPlayAnimationController : MonoBehaviour
{
    public static CardPlayAnimationController Instance;
    
    [Header("Animation Targets")]
    [SerializeField] private Transform centerStage; // Center position for card animation
    [SerializeField] private Transform scorePosition; // Score UI position for number flying
    
    [Header("UI Prefabs")]
    [SerializeField] private GameObject shotTextPrefab; // Text showing shot name
    [SerializeField] private GameObject outcomeTextPrefab; // Text for OUT/WIDE
    [SerializeField] private GameObject flyingNumberPrefab; // Flying number for runs
    
    [Header("Animation Timings")]
    [SerializeField] private float cardFlyDuration = 0.3f;
    [SerializeField] private float shotTextDuration = 0.2f;
    [SerializeField] private float outcomeDisplayDuration = 0.5f;
    [SerializeField] private float statsUpdateDuration = 0.5f;
    [SerializeField] private float processingPause = 0.5f;
    
    private bool isAnimating = false;
    
    void Awake()
    {
        Instance = this;
    }
    
    public bool IsAnimating => isAnimating;
    
    public IEnumerator PlayCardSequence(GameObject cardObject, Sprite cardSprite, BattingStrategy strategy, OutCome outcome)
    {
        isAnimating = true;
        
        // Phase 1: Card flies to center (0.3s)
        yield return StartCoroutine(AnimateCardToCenter(cardObject, cardSprite));
        
        // Phase 2: Shot name appears (0.2s)
        yield return StartCoroutine(ShowShotName(strategy));
        
        // Phase 3: Outcome display (0.5s)
        yield return StartCoroutine(ShowOutcome(outcome));
        
        // Phase 4: Stats emphasis (0.5s)
        yield return StartCoroutine(AnimateStatsUpdate());
        
        // Phase 5: Processing pause
        yield return new WaitForSeconds(processingPause);
        
        isAnimating = false;
    }
    
    private IEnumerator AnimateCardToCenter(GameObject cardObject, Sprite cardSprite)
    {
        if (cardObject == null) yield break;
        
        // Create a copy of the card for animation
        GameObject animCard = new GameObject("AnimatingCard");
        animCard.transform.SetParent(transform);
        
        Image cardImage = animCard.AddComponent<Image>();
        cardImage.sprite = cardSprite;
        cardImage.raycastTarget = false;
        
        RectTransform rect = animCard.GetComponent<RectTransform>();
        rect.position = cardObject.transform.position;
        rect.sizeDelta = new Vector2(200, 280); // Standard card size
        
        // Hide original card
        cardObject.SetActive(false);
        
        // Animate to center
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOMove(centerStage.position, cardFlyDuration).SetEase(Ease.OutCubic));
        seq.Join(rect.DOScale(0.5f, cardFlyDuration));
        seq.Join(rect.DORotate(new Vector3(0, 0, 360), cardFlyDuration, RotateMode.FastBeyond360));
        seq.Join(cardImage.DOFade(0.5f, cardFlyDuration));
        
        yield return seq.WaitForCompletion();
        
        // Keep the card image there (will be cleaned up later)
        Destroy(animCard, 2f);
    }
    
    private IEnumerator ShowShotName(BattingStrategy strategy)
    {
        if (shotTextPrefab == null)
        {
            // Create text dynamically if no prefab
            GameObject textObj = new GameObject("ShotText");
            textObj.transform.SetParent(transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = FormatShotName(strategy);
            text.fontSize = 48;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.position = centerStage.position + Vector3.up * 50;
            rect.sizeDelta = new Vector2(400, 100);
            
            // Punch animation
            rect.localScale = Vector3.zero;
            rect.DOScale(1.2f, shotTextDuration * 0.5f).SetEase(Ease.OutBack);
            rect.DOScale(1f, shotTextDuration * 0.5f).SetDelay(shotTextDuration * 0.5f);
            
            Destroy(textObj, 2f);
        }
        else
        {
            GameObject textObj = Instantiate(shotTextPrefab, centerStage.position + Vector3.up * 50, Quaternion.identity, transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = FormatShotName(strategy);
            
            textObj.transform.DOPunchScale(Vector3.one * 0.2f, shotTextDuration);
            Destroy(textObj, 2f);
        }
        
        yield return new WaitForSeconds(shotTextDuration);
    }
    
    private IEnumerator ShowOutcome(OutCome outcome)
    {
        int runs = (int)outcome;
        
        if (runs > 0)
        {
            // Flying number animation
            yield return StartCoroutine(AnimateFlyingNumber(runs));
        }
        else if (runs == -1) // Out
        {
            yield return StartCoroutine(ShowOutcomeText("OUT!", Color.red));
        }
        else if (runs == -3) // Wide
        {
            yield return StartCoroutine(ShowOutcomeText("WIDE BALL", Color.yellow));
        }
        else // Dot ball
        {
            yield return StartCoroutine(ShowOutcomeText("DOT BALL", Color.gray));
        }
    }
    
    private IEnumerator AnimateFlyingNumber(int runs)
    {
        GameObject numberObj;
        
        if (flyingNumberPrefab == null)
        {
            // Create dynamically
            numberObj = new GameObject("FlyingNumber");
            numberObj.transform.SetParent(transform);
            TextMeshProUGUI text = numberObj.AddComponent<TextMeshProUGUI>();
            text.text = "+" + runs;
            text.fontSize = 64;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.green;
            text.fontStyle = FontStyles.Bold;
            
            RectTransform rect = numberObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 100);
        }
        else
        {
            numberObj = Instantiate(flyingNumberPrefab, transform);
            numberObj.GetComponent<TextMeshProUGUI>().text = "+" + runs;
        }
        
        RectTransform numRect = numberObj.GetComponent<RectTransform>();
        numRect.position = centerStage.position;
        
        // Appear animation
        numRect.localScale = Vector3.zero;
        numRect.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack);
        
        yield return new WaitForSeconds(0.2f);
        
        // Fly to score
        if (scorePosition != null)
        {
            numRect.DOMove(scorePosition.position, 0.3f).SetEase(Ease.InCubic);
            numRect.DOScale(0.3f, 0.3f);
        }
        
        yield return new WaitForSeconds(0.3f);
        Destroy(numberObj);
    }
    
    private IEnumerator ShowOutcomeText(string message, Color color)
    {
        GameObject textObj;
        
        if (outcomeTextPrefab == null)
        {
            textObj = new GameObject("OutcomeText");
            textObj.transform.SetParent(transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 56;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.fontStyle = FontStyles.Bold;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.position = centerStage.position;
            rect.sizeDelta = new Vector2(400, 100);
        }
        else
        {
            textObj = Instantiate(outcomeTextPrefab, centerStage.position, Quaternion.identity, transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = message;
            text.color = color;
        }
        
        // Animation
        textObj.transform.localScale = Vector3.zero;
        textObj.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        
        if (message == "OUT!")
        {
            textObj.transform.DOShakePosition(0.3f, 10f, 20);
        }
        
        yield return new WaitForSeconds(outcomeDisplayDuration);
        
        textObj.GetComponent<TextMeshProUGUI>().DOFade(0, 0.2f);
        Destroy(textObj, 0.3f);
    }
    
    private IEnumerator AnimateStatsUpdate()
    {
        // Find and animate score/wickets/balls UI elements
        // This would pulse or highlight the updated stats
        
        GameObject scoreUI = GameObject.Find("ScoreText"); // Adjust based on your UI
        if (scoreUI != null)
        {
            scoreUI.transform.DOPunchScale(Vector3.one * 0.1f, statsUpdateDuration);
        }
        
        GameObject ballsUI = GameObject.Find("BallsAndOversText"); // Adjust based on your UI
        if (ballsUI != null)
        {
            ballsUI.transform.DOPunchScale(Vector3.one * 0.1f, statsUpdateDuration);
        }
        
        yield return new WaitForSeconds(statsUpdateDuration);
    }
    
    private string FormatShotName(BattingStrategy strategy)
    {
        string name = strategy.ToString();
        name = name.Replace("Shot", " Shot");
        name = name.Replace("Drive", " Drive");
        name = name.Replace("Glance", " Glance");
        name = name.Replace("Defense", " Defense");
        return name + "!";
    }
}