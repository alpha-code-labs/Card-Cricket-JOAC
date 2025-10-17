using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardsPoolManager : MonoBehaviour
{
    [Header("Initial UI elements")]
    [SerializeField] GameObject countdownPanel; // Panel to show countdown
    [SerializeField] TextMeshProUGUI countdownText; // Text for 3, 2, 1 countdown
    [SerializeField] float countdownDuration = 1f;
     [SerializeField] AnimationCurve countdownScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
    [Header("Card Decks")]
    [SerializeField] List<AttackCardData> Deck;
    [Header("Piles")]

    [SerializeField] List<AttackCardProps> DrawPile;
    [SerializeField] List<AttackCardProps> DiscardPile;
    [SerializeField] List<AttackCardProps> HandCards;
    [Header("Game State Vars")]
    public int CurrntTurn = 0; // Current turn number
    public List<BallThrow> BallThrows; // List to hold BallThrow instances
    [Header("Difficulty Settings")]
    [SerializeField] public int baseMaxHandSize = 4;
    private int maxHandSize;
    public int baseMaxRedraws = 1;
    private int maxRedraws; // Maximum redraws per game
    private int redraws = 0; // Track number of redraws used
    private bool cardsInteractable = true;

    [Header("Reffrences")]
    [SerializeField] Transform hand; // Transform to parent drawn cards
    [SerializeField] Transform ballerCardTransform;
    [SerializeField] TextMeshProUGUI BallThrowText; // Text to display current BallThrow

    [Header("Card Animation Settings")]
    [SerializeField] float cardOutroDuration = 0.4f;
    [SerializeField] AnimationCurve cardOutroCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public static CardsPoolManager Instance;
    public static event System.Action OnTurnStarted;
    public static event System.Action<int, int> OnHandRedrawn; // Event to notify when hand is redrawn
    public GameObject cardPrefab; // Assign in inspector

    public GameObject ballerCard;
    public GameplayConfig gameplayConfig;

    public GameObject ballerCardPrefab;
    void Awake()
    {
        DateRecord dateRecord = NewDayManager.currentDateRecord;
        if (GameManager.instance != null)
        {
            maxHandSize = baseMaxHandSize + GameManager.instance.currentSaveData.resourcefulness;
            maxRedraws = baseMaxRedraws + GameManager.instance.currentSaveData.courage;
        }
        else
        {
            maxHandSize = baseMaxHandSize;
            maxRedraws = baseMaxRedraws;
        }

        Instance = this;
    }

    void Start()
    {
        gameplayConfig = GameplayConfiguration.Instance.GetCurrentGameplayConfig();
        StartCoroutine(WaitAndStartTurn());
    }

    IEnumerator WaitAndStartTurn()
    {
        yield return StartCoroutine(InitialCountdown());
        if (gameplayConfig != null)
            InitTextDeck(gameplayConfig.pitchCondition); // Initialize the deck with random cards for batting and bowling disable to keep deck in scene
        else InitTextDeck(PitchCondition.Friendly);
        InstantiateCards();
         yield return new WaitForSeconds(1f);
        StartTurn();
    }


    [ContextMenu("Start Turn")]
    public void StartTurn(bool incrementBalls = true)
    {
        // Check if game is already over
        bool isBattingFirst = ScoreManager.Instance.TargetScore == 0;
        int currentScore = ScoreManager.Instance.currentRuns; // You'll need to make currentRuns public or add a getter

        // Check various game over conditions
        if (CurrntTurn >= ScoreManager.Instance.MaxBalls)
        {
            Debug.Log("Game Over - All balls used");
            ScoreManager.Instance.enableRaycasterOnMainDialogueSystem();
            return;
        }

        if (ScoreManager.Instance.getCurrentWickets() < 1)
        {
            Debug.Log("Game Over - All wickets lost");
            ScoreManager.Instance.enableRaycasterOnMainDialogueSystem();
            return;
        }

        // Check if target achieved (when chasing)
        if (!isBattingFirst && currentScore >= ScoreManager.Instance.TargetScore)
        {
            Debug.Log("Game Over - Target achieved!");
            ScoreManager.Instance.enableRaycasterOnMainDialogueSystem();
            return;
        }

        if (incrementBalls)
            ScoreManager.Instance.UpdateBallsAndOvers(CurrntTurn);

        if (ballerCard != null)
            Destroy(ballerCard);

        ballerCard = InstantiateBallerCard(CurrentBallThrow);
        BallThrowText.text = CurrentBallThrow.ToString();

        // Draw cards for new turn
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }

        Timer.Instance.StartTurnTimer();
        OnTurnStarted?.Invoke();
    }
    [ContextMenu("End Turn")]
    public void EndTurn(int maxBallsToBall, bool incrementBalls = true)
    {
        StartCoroutine(EndTurnWithAnimation( maxBallsToBall, incrementBalls));
    }

    private IEnumerator EndTurnWithAnimation(int maxBallsToBall, bool incrementBalls)
    {
        // Animate cards out first
        yield return AnimateCardsOut();
        
        // Then do the existing EndTurn logic
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card);
            card.gameObject.SetActive(false);
        }

        SetCardsInteractable(true);
        HandCards.Clear();
        CurrntTurn++;
        
        if (CurrntTurn >= maxBallsToBall)
            ScoreManager.Instance.UpdateBallsAndOvers(CurrntTurn);
    }
    [ContextMenu("Draw Card")]
    void DrawCard()
    {
        if (DrawPile.Count <= 0)
        {
            DrawPile = new List<AttackCardProps>(DiscardPile);
            DiscardPile.Clear();
        }
        AttackCardProps card = DrawPile[0];
        DrawPile.RemoveAt(0);
        HandCards.Add(card);
        card.gameObject.SetActive(true); // Activate the card when drawn 
        SimpleHandArcManager arcManager = hand.GetComponent<SimpleHandArcManager>();
        if (arcManager != null)
            arcManager.RefreshCardArrangement();
    }

    // Add this updated RedrawHand method to your CardsPoolManager script:

    public void RedrawHand()
    {
        if (redraws >= maxRedraws)
        {
            Debug.LogWarning($"Cannot redraw: Maximum redraws ({maxRedraws}) already used!");
            return;
        }

        if (HandCards.Count == 0)
        {
            Debug.LogWarning("No cards in hand to redraw!");
            return;
        }

        // Move current hand cards to discard pile
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card);
            card.gameObject.SetActive(false);
        }
        HandCards.Clear();

        // Draw new cards
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }

        redraws++;
        Debug.Log($"Hand redrawn! Redraws used: {redraws}/{maxRedraws}");

        // Reset the timer when redrawing
        if (Timer.Instance != null)
        {
            Timer.Instance.ResetTimerForRedraw();
        }

        // Trigger an event for UI updates
        OnHandRedrawn?.Invoke(redraws, maxRedraws);
    }

    void InstantiateCards()
    {
        DrawPile.Clear();
        foreach (var cardData in Deck)
        {
            AttackCardProps card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps>();
            card.cardData = cardData; // Set the card data
            DrawPile.Add(card);
            card.gameObject.SetActive(false); // Deactivate the card initially
        }
    }

    public IEnumerator AnimateCardsOut()
    {
        if (HandCards.Count == 0) yield break;
        
        List<Sequence> animations = new List<Sequence>();
        
        for (int i = 0; i < HandCards.Count; i++)
        {
            var card = HandCards[i];
            if (card == null || card.gameObject == null) continue;
            
            RectTransform cardRect = card.GetComponent<RectTransform>();
            Image cardImage = card.GetComponent<Image>();
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            
            // Create staggered animation for each card
            Sequence seq = DOTween.Sequence();
            
            // Add delay based on card position for staggered effect
            float delay = i * 0.05f;
            
            seq.AppendInterval(delay);
            
            // Animate card flying down and fading out
            seq.Append(cardRect.DOAnchorPosY(cardRect.anchoredPosition.y - 300f, cardOutroDuration)
                .SetEase(Ease.InBack));
            seq.Join(cardRect.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), cardOutroDuration));
            seq.Join(cardRect.DOScale(0.7f, cardOutroDuration));
            seq.Join(canvasGroup.DOFade(0, cardOutroDuration * 0.8f));
            
            animations.Add(seq);
        }
        
        // Wait for all animations to complete
        float maxDuration = cardOutroDuration + (HandCards.Count * 0.05f);
        yield return new WaitForSeconds(maxDuration);
    }
    public void DestroyCurrentBallCard()
    {
        if (ballerCard != null)
            ballerCard.SetActive(false);
        Destroy(ballerCard);
    }

    GameObject InstantiateBallerCard(BallThrow ballThrow)
    {
        GameObject ballerCard = Instantiate(ballerCardPrefab, ballerCardTransform);
        BallerCardProps cardProps = ballerCard.GetComponent<BallerCardProps>();
        cardProps.assignBallerProps(ballThrow);
        return ballerCard;
    }



    [ContextMenu("Init Text Deck")]
    // void InitTextDeck(PitchCondition pitchCondition = PitchCondition.Friendly)
    // {
    //     Deck.Clear();
    //     foreach (BattingStrategy strategy in System.Enum.GetValues(typeof(BattingStrategy)))
    //     {
    //         Deck.Add(new AttackCardData(strategy));
    //     }
    //     RandomizeDeck();
    //     BallThrows.Clear();

    //     //Over - Fast Bowler Right Arm (6 balls)
    //     // Initialize bowler variables outside the loop
    //     TypeOfBowler bowlerType = TypeOfBowler.Fast;
    //     Side bowlerSide = Side.RightArm;

    //     for (int i = 0; i < ScoreManager.Instance.MaxBalls; i++)
    //     {
    //         // Randomize bowler type and side every 6 balls (start of each over)
    //         if (i % 6 == 0)
    //         {
    //             bowlerType = (TypeOfBowler)Random.Range(0, System.Enum.GetValues(typeof(TypeOfBowler)).Length);
    //             bowlerSide = (Side)Random.Range(0, System.Enum.GetValues(typeof(Side)).Length);
    //         }
    //         BallThrows.Add(ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition));
    //     }
    // }

//     [ContextMenu("Init Text Deck")]
// void InitTextDeck(PitchCondition pitchCondition = PitchCondition.Friendly)
// {
//     Deck.Clear();
//     foreach (BattingStrategy strategy in System.Enum.GetValues(typeof(BattingStrategy)))
//     {
//         Deck.Add(new AttackCardData(strategy));
//     }
//     RandomizeDeck();
//     BallThrows.Clear();

//     // Initialize bowler variables outside the loop (will be randomized each over)
//     TypeOfBowler bowlerType = TypeOfBowler.Fast;
//     Side bowlerSide = Side.RightArm;
    
//     for (int i = 0; i < ScoreManager.Instance.MaxBalls; i++)
//     {
//         // Randomize bowler type and side every 6 balls (start of each over)
//         if (i % 6 == 0)
//         {
//             bowlerType = (TypeOfBowler)Random.Range(0, System.Enum.GetValues(typeof(TypeOfBowler)).Length);
//             bowlerSide = (Side)Random.Range(0, System.Enum.GetValues(typeof(Side)).Length);
//         }
        
//         BallThrow ballThrow = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
        
//         // Check if this ball is a yorker
//         if (ballThrow.ballLength == BallLength.Yorker)
//         {
//             // Check if we already have a yorker in this over
//             int startOfCurrentOver = (i / 6) * 6;
//             bool yorkerExistsInOver = false;
            
//             for (int j = startOfCurrentOver; j < i && j < BallThrows.Count; j++)
//             {
//                 if (BallThrows[j].ballLength == BallLength.Yorker)
//                 {
//                    yorkerExistsInOver = true;
//                     break; 
//                 }
//             }
            
//             // If yorker already exists in this over, generate a new non-yorker ball
//             if (yorkerExistsInOver)
//             {
//                 int attempts = 0;
//                 do
//                 {
//                     ballThrow = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
//                     attempts++;
                    
//                     // Safety check to avoid infinite loop
//                     if (attempts > 50)
//                     {
//                         Debug.LogWarning($"Could not generate non-yorker after 50 attempts at ball {i + 1}, using current ball");
//                         break;
//                     }
//                 } while (ballThrow.ballLength == BallLength.Yorker);
//             }
//         }
        
//         BallThrows.Add(ballThrow);
//     }
// }

[ContextMenu("Init Text Deck")]
void InitTextDeck(PitchCondition pitchCondition = PitchCondition.Friendly)
{
    Deck.Clear();
    foreach (BattingStrategy strategy in System.Enum.GetValues(typeof(BattingStrategy)))
    {
        Deck.Add(new AttackCardData(strategy));
    }
    RandomizeDeck();
    BallThrows.Clear();

    // Initialize bowler variables outside the loop (will be randomized each over)
    TypeOfBowler bowlerType = TypeOfBowler.Fast;
    Side bowlerSide = Side.RightArm;
    
    for (int i = 0; i < ScoreManager.Instance.MaxBalls; i++)
    {
        // Randomize bowler type and side every 6 balls (start of each over)
        if (i % 6 == 0)
        {
            bowlerType = (TypeOfBowler)Random.Range(0, System.Enum.GetValues(typeof(TypeOfBowler)).Length);
            bowlerSide = (Side)Random.Range(0, System.Enum.GetValues(typeof(Side)).Length);
        }
        
        BallThrow ballThrow = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
        
        // Check if this ball is a yorker
        if (ballThrow.ballLength == BallLength.Yorker)
        {
            // Check if we already have a yorker in the last 10 balls
            int startCheckIndex = Mathf.Max(0, i - 9); // Check last 9 balls (current would be the 10th)
            bool yorkerExistsInRange = false;
            
            for (int j = startCheckIndex; j < i && j < BallThrows.Count; j++)
            {
                if (BallThrows[j].ballLength == BallLength.Yorker)
                {
                    yorkerExistsInRange = true;
                    break; 
                }
            }
            
            // If yorker already exists in the last 10 balls window, generate a new non-yorker ball
            if (yorkerExistsInRange)
            {
                int attempts = 0;
                do
                {
                    ballThrow = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
                    attempts++;
                    
                    // Safety check to avoid infinite loop
                    if (attempts > 50)
                    {
                        Debug.LogWarning($"Could not generate non-yorker after 50 attempts at ball {i + 1}, using current ball");
                        break;
                    }
                } while (ballThrow.ballLength == BallLength.Yorker);
            }
        }
        
        BallThrows.Add(ballThrow);
    }
}
    /// <summary>
    /// Randomizes the order of cards in the deck using Fisher-Yates shuffle algorithm
    /// </summary>
    [ContextMenu("Randomize Deck")]
    void RandomizeDeck()
    {
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AttackCardData temp = Deck[i];
            Deck[i] = Deck[randomIndex];
            Deck[randomIndex] = temp;
        }
        Debug.Log("Deck has been randomized!");
    }

    public bool CanRedraw()
    {
        Debug.Log($"Redraws used: {redraws}, Max redraws: {maxRedraws}, Cards in hand: {HandCards.Count}");
        return redraws < maxRedraws && HandCards.Count > 0;
    }

    public int GetRedrawsRemaining()
    {
        return Mathf.Max(0, maxRedraws - redraws);
    }

    public void ResetRedraws()
    {
        redraws = 0;
    }

    public void SetCardsInteractable(bool interactable)
    {
        cardsInteractable = interactable;

        // Disable/enable all card interactions
        foreach (var card in HandCards)
        {
            if (card != null && card.gameObject != null)
            {
                var canvasGroup = card.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = card.gameObject.AddComponent<CanvasGroup>();

                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;

                // Optional: visual feedback
                canvasGroup.alpha = interactable ? 1f : 1f;
            }
        }
    }

    public bool AreCardsInteractable()
    {
        return cardsInteractable;
    }

    public BallThrow CurrentBallThrow
    {
        get
        {
            if (BallThrows.Count > 0)
            {
                return BallThrows[CurrntTurn % BallThrows.Count]; // Cycle through BallThrows based on current turn
            }
            return null; // No BallThrow available
        }
    }
    public void StartInitialCountdown()
    {
        Debug.Log("Starting Initial Countdown");
       StartCoroutine(InitialCountdown());
    }
    public IEnumerator InitialCountdown()
    {
        Debug.Log("Starting Initial Countdown coroutine");
        // Show countdown panel
        if (countdownPanel != null && countdownText != null)
        {
            countdownPanel.SetActive(true);
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null; // Wait one frame for UI to update
            Debug.Log("Countdown panel activated");

            // Countdown from 3 to 1
            for (int i = 3; i >= 1; i--)
            {
                countdownText.text = i.ToString();

                // Animate countdown number
                yield return AnimateCountdownNumber();
            }

            // Show "GO!" or "PLAY!"
            countdownText.text = "Go!";
            yield return AnimateCountdownNumber();

            countdownPanel.SetActive(false);
            Debug.Log("Countdown panel deactivated");
        }
        else
        {
            // Fallback if UI elements are not set
            Debug.LogWarning("Countdown UI elements not configured. Starting timer directly.");
        }
    }
    
    private IEnumerator AnimateCountdownNumber()
    {
        Debug.Log("Starting Initial Countdown number animation");
        if (countdownText == null) yield break;
        
        float elapsed = 0;
        Vector3 originalScale = Vector3.one;
        
        while (elapsed < countdownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / countdownDuration;
            
            // Apply scale animation
            float scale = countdownScaleCurve.Evaluate(t);
            countdownText.transform.localScale = originalScale * scale;
            
            // Fade out near the end
            if (t > 0.7f)
            {
                Color color = countdownText.color;
                color.a = 1f - ((t - 0.7f) / 0.3f);
                countdownText.color = color;
            }
            else
            {
                Color color = countdownText.color;
                color.a = 1f;
                countdownText.color = color;
            }
            
            yield return null;
        }
        
        // Reset for next number
        countdownText.transform.localScale = originalScale;
        Color finalColor = countdownText.color;
        finalColor.a = 1f;
        countdownText.color = finalColor;
    }

}
