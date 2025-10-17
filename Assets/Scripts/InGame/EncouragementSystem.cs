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
    
    [Header("Animation Settings")]
    [SerializeField] float slideInDuration = 0.5f;
    [SerializeField] float dialogueBoxExpandDuration = 0.3f;
    [SerializeField] float textFadeInDuration = 0.3f;
    [SerializeField] float slideOutDuration = 0.4f;
    [SerializeField] Vector2 offScreenPosition = new Vector2(800f, 0f); // Start position off-screen right
    [SerializeField] Vector2 onScreenPosition = Vector2.zero; // Center position
    
    [Header("Display Settings")]
    [SerializeField] float autoSkipDelay = 10f; // Auto skip after this many seconds (0 = disabled)
    [SerializeField] bool pauseGameDuringEncouragement = true;
    
    [Header("Milestone Tracking")]
    private bool milestone50Triggered = false;
    private bool milestone75Triggered = false;
    private bool milestone10RunsTriggered = false;
    private bool milestoneFinalRunsTriggered = false;
    public bool isShowingEncouragement = false;
    private Queue<MilestoneType> pendingMilestones = new Queue<MilestoneType>();
    
    private enum MilestoneType
    {
        Percent50,
        Percent75,
        TenRuns,
        FinalRuns
    }
    
    // Dialogue pools for each milestone
    private Dictionary<MilestoneType, List<string>> dialoguePools = new Dictionary<MilestoneType, List<string>>();
    
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
        // 50% milestone dialogues
        dialoguePools[MilestoneType.Percent50] = new List<string>
        {
            "Brilliant start! You're halfway to victory! Keep this momentum going!",
            "50% done! The target is within reach - stay focused!",
            "Halfway there, champion! You've got this!",
            "Outstanding batting! Half the target achieved already!",
            "You're crushing it! 50% of the target down!",
            "Magnificent! The halfway mark reached - victory is in sight!"
        };
        
        // 75% milestone dialogues
        dialoguePools[MilestoneType.Percent75] = new List<string>
        {
            "Incredible! Three-quarters done! The finish line is so close!",
            "75% complete! Just a few more good shots!",
            "You're dominating! Only 25% left to chase!",
            "Sensational batting! The target is almost yours!",
            "What a performance! Just a little push needed now!",
            "Amazing! You've nearly sealed the victory!"
        };
        
        // 10 runs remaining dialogues
        dialoguePools[MilestoneType.TenRuns] = new List<string>
        {
            "Just 10 runs to go! Two boundaries will do it!",
            "Single digits away from victory! Stay calm!",
            "10 runs! You can knock these off easily!",
            "So close! Just 10 more and you're the hero!",
            "The target is right there! 10 runs to glory!",
            "Almost home! These last 10 runs are yours!"
        };
        
        // 1-2 runs remaining dialogues
        dialoguePools[MilestoneType.FinalRuns] = new List<string>
        {
            "Just a single needed! This is your moment!",
            "One hit away from victory! Make it count!",
            "A couple of runs! Victory is guaranteed!",
            "This is it! One shot to seal the win!",
            "Just nudge it for a single! You've won this!",
            "The winning run! History awaits!"
        };
    }
    
    public void CheckMilestones(int currentRuns, int targetScore)
    {
        // Don't check if we're batting first (no target to chase)
        if (targetScore <= 0) return;
        
        // Don't check if already showing encouragement
        if (isShowingEncouragement) return;
        
        float percentage = (float)currentRuns / targetScore * 100f;
        int runsNeeded = targetScore - currentRuns;
        
        MilestoneType? triggeredMilestone = null;
        
        // Check which milestone to trigger (prioritize closer to victory)
        if (runsNeeded <= 2 && runsNeeded > 0 && !milestoneFinalRunsTriggered)
        {
            milestoneFinalRunsTriggered = true;
            triggeredMilestone = MilestoneType.FinalRuns;
        }
        else if (runsNeeded <= 10 && runsNeeded > 2 && !milestone10RunsTriggered)
        {
            milestone10RunsTriggered = true;
            triggeredMilestone = MilestoneType.TenRuns;
        }
        else if (percentage >= 75f && !milestone75Triggered)
        {
            milestone75Triggered = true;
            triggeredMilestone = MilestoneType.Percent75;
        }
        else if (percentage >= 50f && !milestone50Triggered)
        {
            milestone50Triggered = true;
            triggeredMilestone = MilestoneType.Percent50;
        }
        
        if (triggeredMilestone.HasValue)
        {
            pendingMilestones.Clear(); // Clear any previous pending milestones
            pendingMilestones.Enqueue(triggeredMilestone.Value);
        }
    }
    
    public void TryShowPendingEncouragement()
    {
        // This should be called after a ball is completed
        if (pendingMilestones.Count > 0 && !isShowingEncouragement)
        {
            MilestoneType milestone = pendingMilestones.Dequeue();
            ShowEncouragement(milestone);
        }
    }
    
    void ShowEncouragement(MilestoneType milestone)
    {
        if (isShowingEncouragement) return;
        
        isShowingEncouragement = true;
        
        // Pause the game if configured
        if (pauseGameDuringEncouragement)
        {
            Timer.Instance?.PauseTimer();
            CardsPoolManager.Instance?.SetCardsInteractable(false);
        }
        
        // Get random dialogue for this milestone
        string dialogue = GetRandomDialogue(milestone);
        
        // Start the encouragement sequence
        StartCoroutine(ShowEncouragementSequence(dialogue));
    }
    
    string GetRandomDialogue(MilestoneType milestone)
    {
        if (dialoguePools.ContainsKey(milestone) && dialoguePools[milestone].Count > 0)
        {
            List<string> pool = dialoguePools[milestone];
            return pool[Random.Range(0, pool.Count)];
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
        
        // Expand dialogue box
        dialogueBox.transform.DOScale(Vector3.one, dialogueBoxExpandDuration).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(dialogueBoxExpandDuration);
        
        // Fade in text with typewriter effect
        dialogueText.text = dialogue;
        dialogueText.DOFade(1f, textFadeInDuration);
        
        // Optional: Typewriter effect
        yield return TypewriterEffect(dialogue);
        
        // Show skip button
        skipButton.gameObject.SetActive(true);
        skipButton.transform.localScale = Vector3.zero;
        skipButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        
        // Auto-skip if configured
        if (autoSkipDelay > 0)
        {
            yield return new WaitForSeconds(autoSkipDelay);
            if (isShowingEncouragement) // Check if not already skipped
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
            yield return new WaitForSeconds(0.02f); // Adjust speed as needed
        }
    }
    
    void OnSkipButtonClicked()
    {
        HideEncouragement();
    }
    
    void HideEncouragement()
    {
        if (!isShowingEncouragement) return;
        
        StartCoroutine(HideEncouragementSequence());
    }
    
    IEnumerator HideEncouragementSequence()
    {
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
        
        isShowingEncouragement = false;
        
        // Check if there are more pending milestones
        TryShowPendingEncouragement();
    }
    
    public void ResetMilestones()
    {
        // Call this when starting a new chase
        milestone50Triggered = false;
        milestone75Triggered = false;
        milestone10RunsTriggered = false;
        milestoneFinalRunsTriggered = false;
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