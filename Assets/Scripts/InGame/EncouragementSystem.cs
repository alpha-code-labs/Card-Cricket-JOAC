using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EncouragementSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject encouragementPanel; // Main panel that slides in
    [SerializeField] Image characterImage; // Character sprite
    [SerializeField] GameObject dialogueBox; // Dialogue container
    [SerializeField] TextMeshProUGUI dialogueText; // Text component for dialogue
    [SerializeField] Button skipButton; // Skip/Got it button
    [SerializeField] TextMeshProUGUI skipButtonText;
    
    [Header("Character Expressions")]
    [SerializeField] Sprite happyExpression;
    [SerializeField] Sprite lovingExpression;
    [SerializeField] Sprite relievedExpression;
    [SerializeField] Sprite seriousExpression;
    [SerializeField] Sprite overjoyedExpression;
    [SerializeField] Sprite sadExpression;
    
    [Header("Animation Settings")]
    [SerializeField] float slideInDuration = 0.5f;
    [SerializeField] float dialogueBoxExpandDuration = 0.3f;
    [SerializeField] float textFadeInDuration = 0.3f;
    [SerializeField] float slideOutDuration = 0.4f;
    [SerializeField] Vector2 offScreenPosition = new Vector2(800f, 0f);
    [SerializeField] Vector2 onScreenPosition = Vector2.zero;
    
    [Header("Display Settings")]
    [SerializeField] float autoSkipDelay = 10f; // Auto skip after this many seconds (0 = disabled)
    [SerializeField] bool pauseGameDuringEncouragement = true;
    
    [Header("Milestone Tracking")]
    private bool milestone50Triggered = false;
    private bool milestone75Triggered = false;
    private bool milestone10RunsTriggered = false;
    private bool milestoneFinalRunsTriggered = false;
    private bool milestone10ballsTriggered = false;
    private bool valiant50Triggered = false;
    private bool valiant75Triggered = false;
    private bool _isShowingEncouragement = false;
    public bool isShowingEncouragement { get { return _isShowingEncouragement; } }
    private Queue<MilestoneData> pendingMilestones = new Queue<MilestoneData>();
    
    // Store context for dynamic dialogues
    private int currentRunsNeeded;
    private int currentBallsRemaining;
    private float currentRequiredRate;
    private SituationCategory currentSituation;
    
    private enum MilestoneType
    {
        Percent50,
        Percent75,
        TenRuns,
        FinalRuns,
        ValiantEffort50,
        ValiantEffort75,
        ValiantEffortFinal
    }
    
    private enum CharacterExpression
    {
        Happy,
        Loving,
        Relieved,
        Serious,
        Overjoyed,
        Sad
    }
    
    private enum SituationCategory
    {
        Cruising,      // RRR < 0.8
        Comfortable,   // RRR 0.8-1.2
        Manageable,    // RRR 1.2-1.6
        Challenging,   // RRR 1.6-2.0
        Difficult,     // RRR 2.0-3.0
        Critical,      // RRR 3.0-4.0
        Unreachable    // RRR > 4.0
    }
    
    private class MilestoneData
    {
        public MilestoneType type;
        public CharacterExpression expression;
        public SituationCategory situation;
        
        public MilestoneData(MilestoneType t, CharacterExpression e, SituationCategory s)
        {
            type = t;
            expression = e;
            situation = s;
        }
    }
    
    // Dialogue pools organized by milestone and situation
    private Dictionary<string, List<string>> dialoguePools = new Dictionary<string, List<string>>();
    
    private static EncouragementSystem instance;
    public static EncouragementSystem Instance
    {
        get { return instance; }
    }
    
    void Awake()
    {
        instance = this;
        InitializeDialoguePools();
        
        // Set initial state
        if (encouragementPanel != null)
        {
            encouragementPanel.SetActive(false);
            RectTransform rect = encouragementPanel.GetComponent<RectTransform>();
            rect.anchoredPosition = offScreenPosition;
        }
        
        if (dialogueBox != null)
        {
            dialogueBox.transform.localScale = Vector3.zero;
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            if (skipButtonText != null)
                skipButtonText.text = "Got it!";
        }
    }
    
    void InitializeDialoguePools()
    {
        // 50% Comfortable/Cruising dialogues
        dialoguePools["50_Comfortable"] = new List<string>
        {
            "Halfway there with {1} balls in hand! You're in complete control!",
            "50% done, {1} balls remaining - smooth sailing ahead!",
            "Brilliant! Halfway to victory with plenty of time!",
            "Outstanding! 50% achieved and the rate is comfortable!"
        };
        
        // 50% Challenging dialogues
        dialoguePools["50_Challenging"] = new List<string>
        {
            "Halfway to target! {0} needed in {1} balls - need {2} per ball. Stay sharp!",
            "50% reached but only {1} balls remain. Time to accelerate!",
            "Halfway there! The required rate is climbing - find the boundaries!",
            "50% done! Need to maintain {2} runs per ball from here."
        };
        
        // 50% Critical/Unreachable dialogues
        dialoguePools["50_Critical"] = new List<string>
        {
            "Halfway there but {0} needed in just {1} balls. Go for everything!",
            "50% scored - need boundaries every ball now. Fortune favors the brave!",
            "Half the target reached! This requires something special now!"
        };
        
        // 75% Comfortable dialogues
        dialoguePools["75_Comfortable"] = new List<string>
        {
            "Three-quarters done with {1} balls left! Victory is yours!",
            "75% complete! Just {0} needed from {1} balls - you've got this!",
            "Almost there! The finish line is in sight!",
            "Incredible! 75% done and cruising to victory!"
        };
        
        // 75% Challenging dialogues
        dialoguePools["75_Challenging"] = new List<string>
        {
            "75% done! {0} from {1} balls - keep pushing!",
            "Three-quarters complete but the rate is tight. Stay focused!",
            "Almost there! Need {2} per ball - you can do this!",
            "75% achieved! These last {0} need smart batting!"
        };
        
        // Final 10 balls dialogues (always mention balls)
        dialoguePools["Final10Balls"] = new List<string>
        {
            "Final {1} ! {0} needed - it's now or never!",
            "Last {1}, {0} required. Make every ball count!",
            "Last {1} and a bit! {0} - this is your moment!",
            "{1} remaining, {0} to win! Time for heroics!"
        };
        
        // Ten runs or less dialogues
        dialoguePools["TenRuns"] = new List<string>
        {
            "Just {0} to go! You're almost there!",
            "Only {0} needed! Victory is within grasp!",
            "{0} away from glory! Stay calm and focused!",
            "So close! Just {0} more for the win!"
        };
        
        // Final runs (1-2) dialogues
        dialoguePools["FinalRuns"] = new List<string>
        {
            "{0} to win! One good shot!",
            "Just {0} needed! This is it!",
            "{0} for victory! Make it count!",
            "The winning runs! Just {0}!"
        };
        
        // Valiant effort dialogues (when target is unreachable)
        dialoguePools["Valiant50"] = new List<string>
        {
            "You've scored half their total! That's an achievement in itself!",
            "50% of that massive target - you're showing real character!",
            "Halfway to their score! This is brave batting!",
            "Half their total reached! You're giving them a real fight!"
        };
        
        dialoguePools["Valiant75"] = new List<string>
        {
            "Three-quarters of their total! This has been an incredible effort!",
            "75% reached! Win or lose, this is impressive batting!",
            "What a fight! 75% of that huge target scored!",
            "You've nearly matched their total! This is something special!"
        };
        
        dialoguePools["ValiantFinal"] = new List<string>
        {
            "The target was steep, but what a fight you've shown!",
            "You've given it everything - that's what champions do!",
            "An incredible effort against tough odds. Be proud!",
            "You've shown tremendous courage out there!"
        };
    }
    
    public void CheckMilestones(int currentRuns, int targetScore, int ballsRemaining)
    {
        // Don't check if we're batting first (no target to chase)
        if (targetScore <= 0) return;
        
        // Don't check if already showing encouragement
        if (_isShowingEncouragement) return;
        
        // Calculate context
        int runsNeeded = targetScore - currentRuns;
        // int ballsRemaining = ScoreManager.Instance.MaxBalls - CardsPoolManager.Instance.CurrntTurn;
        float requiredRate = ballsRemaining > 0 ? (float)runsNeeded / ballsRemaining : 999f;
        float percentage = (float)currentRuns / targetScore * 100f;
        
        // Store context for dialogue formatting
        currentRunsNeeded = runsNeeded;
        currentBallsRemaining = ballsRemaining;
        currentRequiredRate = requiredRate;
        
        // Determine situation category
        currentSituation = GetSituationCategory(requiredRate);
        
        // Determine if target is unreachable
        bool isUnreachable = requiredRate > 4.0f;
        
        MilestoneData triggeredMilestone = null;
        
        // Check milestones based on situation
        if (isUnreachable)
        {
            // Valiant effort milestones
            if (percentage >= 75f && !valiant75Triggered)
            {
                valiant75Triggered = true;
                CharacterExpression expr = percentage >= 75f ? CharacterExpression.Loving : CharacterExpression.Sad;
                triggeredMilestone = new MilestoneData(MilestoneType.ValiantEffort75, expr, currentSituation);
            }
            else if (percentage >= 50f && !valiant50Triggered)
            {
                valiant50Triggered = true;
                triggeredMilestone = new MilestoneData(MilestoneType.ValiantEffort50, CharacterExpression.Loving, currentSituation);
            }
        }
        else
        {
            // Regular milestones
            if (runsNeeded <= 2 && runsNeeded > 0 && !milestoneFinalRunsTriggered)
            {
                Debug.Log("Pushing milestoneFinalRuns");
                milestoneFinalRunsTriggered = true;
                CharacterExpression expr = GetExpressionForSituation(currentSituation);
                triggeredMilestone = new MilestoneData(MilestoneType.FinalRuns, expr, currentSituation);
            }
            else if (runsNeeded <= 10 && runsNeeded > 2 && !milestone10RunsTriggered)
            {
                Debug.Log("Pushing milestone10Runs");
                milestone10RunsTriggered = true;
                CharacterExpression expr = GetExpressionForSituation(currentSituation);
                triggeredMilestone = new MilestoneData(MilestoneType.TenRuns, expr, currentSituation);
            }
           else if (percentage  >= 75f && !milestone75Triggered && requiredRate < 3.0f)
            {
                Debug.Log("Pushing milestone75");
                milestone75Triggered = true;
                CharacterExpression expr = GetExpressionForSituation(currentSituation);
                triggeredMilestone = new MilestoneData(MilestoneType.Percent75, expr, currentSituation);
            }
            else if (percentage >= 50f && !milestone50Triggered && requiredRate < 3.0f)
            {
                Debug.Log("Pushing milestone50");
                milestone50Triggered = true;
                CharacterExpression expr = GetExpressionForSituation(currentSituation);
                triggeredMilestone = new MilestoneData(MilestoneType.Percent50, expr, currentSituation);
            }
        }
        
        // Special check for final 10 balls
        if (ballsRemaining <= 10 && ballsRemaining > 0 && !isUnreachable && !milestone10ballsTriggered)
        {
            milestone10ballsTriggered = true;
            // This can trigger alongside other milestones
            CharacterExpression expr = requiredRate > 1.5f ? CharacterExpression.Serious : CharacterExpression.Happy;
            if (triggeredMilestone == null)
            {
                triggeredMilestone = new MilestoneData(MilestoneType.TenRuns, expr, currentSituation);
            }
        }
        
        if (triggeredMilestone != null)
        {
            pendingMilestones.Clear();
            Debug.Log("Pushed milestone " + triggeredMilestone.type);
            pendingMilestones.Enqueue(triggeredMilestone);
        }
    }
    
    private SituationCategory GetSituationCategory(float requiredRate)
    {
        if (requiredRate < 0.8f) return SituationCategory.Cruising;
        if (requiredRate < 1.2f) return SituationCategory.Comfortable;
        if (requiredRate < 1.6f) return SituationCategory.Manageable;
        if (requiredRate < 2.0f) return SituationCategory.Challenging;
        if (requiredRate < 3.0f) return SituationCategory.Difficult;
        if (requiredRate < 4.0f) return SituationCategory.Critical;
        return SituationCategory.Unreachable;
    }
    private CharacterExpression GetExpressionForSituation(SituationCategory situation)
    {
        switch (situation)
        {
            case SituationCategory.Cruising:
                return CharacterExpression.Overjoyed;
            case SituationCategory.Comfortable:
                return CharacterExpression.Happy;
            case SituationCategory.Manageable:
                return Random.Range(0, 2) == 0 ? CharacterExpression.Happy : CharacterExpression.Relieved;
            case SituationCategory.Challenging:
                return CharacterExpression.Serious;
            case SituationCategory.Difficult:
                return Random.Range(0, 2) == 0 ? CharacterExpression.Serious : CharacterExpression.Loving;
            case SituationCategory.Critical:
                return CharacterExpression.Loving;
            case SituationCategory.Unreachable:
                return CharacterExpression.Sad;
            default:
                return CharacterExpression.Happy;
        }
    }
    
    public void TryShowPendingEncouragement()
    {
        if (pendingMilestones.Count > 0 && !_isShowingEncouragement)
        {
            MilestoneData milestone = pendingMilestones.Dequeue();
            ShowEncouragement(milestone);
        }
    }
    
    void ShowEncouragement(MilestoneData milestone)
    {
        if (_isShowingEncouragement) return;
        
        _isShowingEncouragement = true;
        
        // Pause the game if configured
        if (pauseGameDuringEncouragement)
        {
            Timer.Instance?.PauseTimer();
            CardsPoolManager.Instance?.SetCardsInteractable(false);
        }
        
        // Set character expression
        SetCharacterExpression(milestone.expression);
        
        // Get appropriate dialogue
        string dialogue = GetContextualDialogue(milestone);
        
        // Start the encouragement sequence
        StartCoroutine(ShowEncouragementSequence(dialogue));
    }
    
    private void SetCharacterExpression(CharacterExpression expression)
    {
        if (characterImage == null) return;
        
        Sprite expressionSprite = happyExpression; // Default
        
        switch (expression)
        {
            case CharacterExpression.Happy:
                expressionSprite = happyExpression;
                break;
            case CharacterExpression.Loving:
                expressionSprite = lovingExpression;
                break;
            case CharacterExpression.Relieved:
                expressionSprite = relievedExpression;
                break;
            case CharacterExpression.Serious:
                expressionSprite = seriousExpression;
                break;
            case CharacterExpression.Overjoyed:
                expressionSprite = overjoyedExpression;
                break;
            case CharacterExpression.Sad:
                expressionSprite = sadExpression;
                break;
        }
        
        if (expressionSprite != null)
            characterImage.sprite = expressionSprite;
    }
    
    string GetContextualDialogue(MilestoneData milestone)
    {
        string poolKey = "";
        bool shouldMentionBalls = currentBallsRemaining <= 10; // Always mention in last 10 balls
        
        // Determine dialogue pool key based on milestone type and situation
        switch (milestone.type)
        {
            case MilestoneType.Percent50:
                if (currentSituation <= SituationCategory.Comfortable)
                    poolKey = "50_Comfortable";
                else if (currentSituation <= SituationCategory.Challenging)
                    poolKey = "50_Challenging";
                else
                    poolKey = "50_Critical";
                shouldMentionBalls = currentSituation >= SituationCategory.Challenging || currentBallsRemaining <= 30;
                break;
                
            case MilestoneType.Percent75:
                if (currentSituation <= SituationCategory.Comfortable)
                    poolKey = "75_Comfortable";
                else
                    poolKey = "75_Challenging";
                shouldMentionBalls = true; // Always mention balls at 75%
                break;
                
            case MilestoneType.TenRuns:
                if (currentBallsRemaining <= 10)
                    poolKey = "Final10Balls";
                else
                    poolKey = "TenRuns";
                break;
                
            case MilestoneType.FinalRuns:
                poolKey = "FinalRuns";
                break;
                
            case MilestoneType.ValiantEffort50:
                poolKey = "Valiant50";
                break;
                
            case MilestoneType.ValiantEffort75:
                poolKey = "Valiant75";
                break;
                
            case MilestoneType.ValiantEffortFinal:
                poolKey = "ValiantFinal";
                break;
        }
        
        if (dialoguePools.ContainsKey(poolKey) && dialoguePools[poolKey].Count > 0)
        {
            List<string> pool = dialoguePools[poolKey];
            string template = pool[Random.Range(0, pool.Count)];
            
            // Format the dialogue with context
            string runsText = currentRunsNeeded == 1 ? "1 run" : $"{currentRunsNeeded} runs";
            string ballsText = currentBallsRemaining == 1 ? "1 ball" : $"{currentBallsRemaining} balls";
            string rateText = $"{currentRequiredRate:F1}";
            
            // Replace placeholders
            string formatted = template;
            formatted = formatted.Replace("{0}", runsText);
            if (shouldMentionBalls || template.Contains("{1}"))
                formatted = formatted.Replace("{1}", ballsText);
            formatted = formatted.Replace("{2}", rateText);
            
            return formatted;
        }
        
        return "Keep going! You're doing great!";
    }
    
    IEnumerator ShowEncouragementSequence(string dialogue)
    {
        // Prepare UI
        encouragementPanel.SetActive(true);
        dialogueBox.transform.localScale = Vector3.zero;
        dialogueText.text = "";
        dialogueText.alpha = 0;
        skipButton.gameObject.SetActive(false);
        
        RectTransform panelRect = encouragementPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = offScreenPosition;

        // Slide in from right
        panelRect.DOAnchorPos(onScreenPosition, slideInDuration).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(slideInDuration);
        Debug.Log("Encouragement panel waited for slide in");
        
        // Expand dialogue box
        dialogueBox.transform.DOScale(Vector3.one, dialogueBoxExpandDuration).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(dialogueBoxExpandDuration);
        Debug.Log("Encouragement panel waited for dialogue box expand");
        
        // Fade in text with typewriter effect
        dialogueText.text = dialogue;
        dialogueText.DOFade(1f, textFadeInDuration);

        // Optional: Typewriter effect
        yield return TypewriterEffect(dialogue);
        Debug.Log("Encouragement panel waited for text fade in");
        
        // Show skip button
        skipButton.gameObject.SetActive(true);
        skipButton.transform.localScale = Vector3.zero;
        skipButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        Debug.Log("Encouragement panel waited for skip button scale in");
        Debug.Log(autoSkipDelay + " is the auto-skip delay");
        // Auto-skip if configured
        if (autoSkipDelay > 0)
        {
            yield return new WaitForSeconds(autoSkipDelay);
            Debug.Log("Encouragement panel waited for auto-skip delay");
            if (_isShowingEncouragement) // Check if not already skipped
            {
                HideEncouragement();
            }
        }
    }
    
    IEnumerator TypewriterEffect(string fullText)
    {
        dialogueText.text = "";
        dialogueText.alpha = 1f;
        
        foreach (char letter in fullText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
    }
    
    void OnSkipButtonClicked()
    {
        Debug.Log("Clicked on Skip Button");
        HideEncouragement();
    }
    
    void HideEncouragement()
    {
        if (!_isShowingEncouragement) return;
        
        StartCoroutine(HideEncouragementSequence());
    }
    
    IEnumerator HideEncouragementSequence()
    {
        Debug.Log("Hding Encouragment system");
        // Disable skip button
        skipButton.interactable = false;
        
        // Shrink dialogue box
        dialogueBox.transform.DOScale(Vector3.zero, dialogueBoxExpandDuration * 0.7f).SetEase(Ease.InBack);
        dialogueText.DOFade(0f, dialogueBoxExpandDuration * 0.5f);
        
        yield return new WaitForSeconds(dialogueBoxExpandDuration * 0.7f);
        
        // Slide out to right
        RectTransform panelRect = encouragementPanel.GetComponent<RectTransform>();
        panelRect.DOAnchorPos(offScreenPosition, slideOutDuration).SetEase(Ease.InBack);
        
        yield return new WaitForSeconds(slideOutDuration);
        
        // Hide panel
        encouragementPanel.SetActive(false);
        skipButton.interactable = true;
        
        // Resume game
        if (pauseGameDuringEncouragement)
        {
            Timer.Instance?.ResumeTimer();
            CardsPoolManager.Instance?.SetCardsInteractable(true);
        }
        
        _isShowingEncouragement = false;
        
        // Check if there are more pending milestones
        TryShowPendingEncouragement();
    }
    
    public void ResetMilestones()
    {
        milestone50Triggered = false;
        milestone75Triggered = false;
        milestone10RunsTriggered = false;
        milestoneFinalRunsTriggered = false;
        milestone10ballsTriggered = false;
        valiant50Triggered = false;
        valiant75Triggered = false;
        pendingMilestones.Clear();
    }
    
    void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        }
    }
}