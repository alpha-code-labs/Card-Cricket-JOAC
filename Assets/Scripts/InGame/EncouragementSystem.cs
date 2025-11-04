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
    
    [Header("Character Expression")]
    [SerializeField] Sprite defaultExpression; // Only using default expression
    
    [Header("Animation Settings")]
    [SerializeField] float slideInDuration = 0.5f;
    [SerializeField] float dialogueBoxExpandDuration = 0.3f;
    [SerializeField] float textFadeInDuration = 0.3f;
    [SerializeField] float slideOutDuration = 0.4f;
    [SerializeField] Vector2 offScreenPosition = new Vector2(800f, 0f);
    [SerializeField] Vector2 onScreenPosition = Vector2.zero;
    
    [Header("Display Settings")]
    [SerializeField] float autoSkipDelay = 3f; // Auto skip after this many seconds (0 = disabled)
    [SerializeField] bool pauseGameDuringEncouragement = true;
    [SerializeField] MusicIntensity musicIntensity;
    
    [Header("Milestone Tracking")]
    private bool milestone50Triggered = false;
    private bool milestone75Triggered = false;
    private bool milestone10RunsTriggered = false;
    private bool _isShowingEncouragement = false;
    public bool isShowingEncouragement { get { return _isShowingEncouragement; } }
    private Queue<string> pendingDialogues = new Queue<string>();
    
    private static EncouragementSystem instance;
    public static EncouragementSystem Instance
    {
        get { return instance; }
    }
    
    void Awake()
    {
        instance = this;
        
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
        
        // Set default expression
        if (characterImage != null && defaultExpression != null)
        {
            characterImage.sprite = defaultExpression;
        }
    }
    
    public void CheckMilestones(int currentRuns, int targetScore)
    {
        //Checking Milestones
        Debug.Log($"Checking milestones: {currentRuns}/{targetScore}");
        // Don't check if we're batting first (no target to chase)
        if (targetScore <= 0) return;
        
        // Don't check if already showing encouragement
        if (_isShowingEncouragement) return;
        
        // Calculate percentage and runs needed
        float percentage = (float)currentRuns / targetScore * 100f;
        int runsNeeded = targetScore - currentRuns;
        
        string triggeredDialogue = null;    
        
        // Check for 10 runs to go (highest priority)
        if (runsNeeded <= 10 && runsNeeded > 0 && !milestone10RunsTriggered)
        {
            milestone10RunsTriggered = true;
            triggeredDialogue = "This is it my boy, Final 10 runs. It's now or never.";
            musicIntensity.SetExcitement(.8f);
        }
        // Check for 75% milestone
        else if (percentage >= 75f && !milestone75Triggered && !milestone10RunsTriggered)
        {
            milestone75Triggered = true;
            triggeredDialogue = "Come on Raju, getting close. Do not lose it from here.";
            musicIntensity.SetExcitement(.65f);
        }
        // Check for 50% milestone
        else if (percentage >= 50f && !milestone50Triggered && !milestone75Triggered && !milestone10RunsTriggered)
        {
            milestone50Triggered = true;
            triggeredDialogue = "Come on Raju, half way there. Keep your focus.";
            musicIntensity.SetExcitement(.55f);
        }
        
        if (triggeredDialogue != null)
        {
            pendingDialogues.Clear();
            pendingDialogues.Enqueue(triggeredDialogue);
            TryShowPendingEncouragement();
        }
    }
    
    public void TryShowPendingEncouragement()
    {
        if (pendingDialogues.Count > 0 && !_isShowingEncouragement)
        {
            string dialogue = pendingDialogues.Dequeue();
            ShowEncouragement(dialogue);
        }
    }
    
    void ShowEncouragement(string dialogue)
    {
        if (_isShowingEncouragement) return;
        
        _isShowingEncouragement = true;
         // Stop any active commentary immediately
        if (CardPlayAnimationController.Instance != null)
        {
            CardPlayAnimationController.Instance.StopCommentary();
        }
        // Pause the game if configured
        if (pauseGameDuringEncouragement)
        {
            Timer.Instance?.PauseTimer();
            CardsPoolManager.Instance?.SetCardsInteractable(false);
        }
        
        // Start the encouragement sequence
        StartCoroutine(ShowEncouragementSequence(dialogue));
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
        // skipButton.gameObject.SetActive(true);
        skipButton.transform.localScale = Vector3.zero;
        skipButton.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        
        // Auto-skip if configured
        if (autoSkipDelay > 0)
        {
            yield return new WaitForSeconds(autoSkipDelay);
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
        HideEncouragement();
    }
    
    void HideEncouragement()
    {
        if (!_isShowingEncouragement) return;
        
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
        
        _isShowingEncouragement = false;
        
        // Check if there are more pending milestones
        TryShowPendingEncouragement();
    }
    
    public void ResetMilestones()
    {
        milestone50Triggered = false;
        milestone75Triggered = false;
        milestone10RunsTriggered = false;
        pendingDialogues.Clear();
    }
    
    void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        }
    }
}