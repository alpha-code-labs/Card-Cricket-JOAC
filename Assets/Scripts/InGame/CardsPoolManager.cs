using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

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
    private int ballCount = 0;
    public List<BallThrow> BallThrows; // List to hold BallThrow instances
    [Header("Difficulty Settings")]
    [SerializeField] public int baseMaxHandSize = 4;
    private int maxHandSize;
    public int baseMaxRedraws = 1;
    private int maxRedraws; // Maximum redraws per game
    private int redraws = 0; // Track number of redraws used
    private bool cardsInteractable = true;
    
    [Header("Smart Card Selection")]
    [SerializeField] bool useSmartCardSelection = true; // Toggle for smart card selection
    [SerializeField] float debugRunRateOverride = -1f; // For testing specific run rates, -1 means use actual

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
    
    // Track which cards have been created for smart selection
    private Dictionary<BattingStrategy, AttackCardProps> cardPropsMap = new Dictionary<BattingStrategy, AttackCardProps>();

    void Awake()
    {
        if (GameManager.instance != null)
        {
            switch (GameManager.instance.currentSaveData.resourcefulness)
            {
                case 1: maxHandSize = baseMaxHandSize + 1; break;
                case 2: maxHandSize = baseMaxHandSize + 1; break;
                case 3: maxHandSize = baseMaxHandSize + 2; break;
                default: maxHandSize = baseMaxHandSize; break;
            }
            maxRedraws = baseMaxRedraws + GameManager.instance.currentSaveData.courage;
        }
        else
        {
            maxHandSize = baseMaxHandSize;
            maxRedraws = baseMaxRedraws;
        }

        Instance = this;
    }

    public void HandleWideBall(int x)
    {
        //when wide occurs add a ball
        
        //get current overs bowlerType and bowlerSide
        BallThrow CurrentBallThrow = BallThrows[ballCount];
        TypeOfBowler bowlerType = CurrentBallThrow.bowlerType;
        Side bowlerSide = CurrentBallThrow.bowlerSide;

        //generate a random ball for the same over
        BallThrow ball = GenerateSafeBall(bowlerType, bowlerSide);

        //insert it in the list
        BallThrows.Insert(ballCount + 1, ball);

        Debug.Log("Inserted one ball of bowlerType " + bowlerType + " and side " + bowlerSide + " after encountering wide ball");
    }
    
    private BallThrow GenerateSafeBall(TypeOfBowler bowlerType, Side bowlerSide)
    {
        BallThrow safeBall = null;
        int attempts = 0;
        const int MAX_ATTEMPTS = 100;
        
        // Get pitch condition from gameplayConfig
        PitchCondition pitchCondition = gameplayConfig != null ? gameplayConfig.pitchCondition : PitchCondition.Friendly;
        
        while (attempts < MAX_ATTEMPTS)
        {
            BallThrow candidateBall = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(
                bowlerType, bowlerSide, pitchCondition);
            attempts++;
            
            // Ensure it's not an extreme ball
            if (candidateBall.ballLine != BallLine.WayOutsideOff && 
                candidateBall.ballLine != BallLine.WayDowntheLeg &&
                candidateBall.ballLength != BallLength.Yorker)
            {
                safeBall = candidateBall;
                break;
            }
        }
        
        // Fallback to create a default safe ball if generation fails
        if (safeBall == null)
        {
            Debug.LogWarning("Could not generate safe ball, using default");
            safeBall = new BallThrow()
            {
                ballLength = BallLength.FullLength,
                ballLine = BallLine.OffStump
            };
        }
        
        return safeBall;
    }


    void Start()
    {
        StartCoroutine(WaitAndStartTurn());
         if (GameplayConfiguration.Instance != null)
        {
            gameplayConfig = GameplayConfiguration.Instance.GetCurrentGameplayConfig();
        }
        else
        {
            Debug.LogError("GameplayConfiguration.Instance is null!");
        }
        if (gameplayConfig == null)
        {
            Debug.Log("loading gameplay 2 in CardsPoolManager as date is null");
            gameplayConfig = GameplayConfiguration.Instance.GetConfigForDate("1989/02/02");
        }
         ScoreManager.OnWideBall += HandleWideBall;
    }

    IEnumerator WaitAndStartTurn()
    {
        yield return StartCoroutine(InitialCountdown());
        if (gameplayConfig != null)
            InitTextDeck(gameplayConfig.pitchCondition); // Initialize the deck with random cards for batting and bowling disable to keep deck in scene
        else InitTextDeck(PitchCondition.Friendly);
        
        if (useSmartCardSelection)
        {
            InstantiateAllPossibleCards(); // Create all cards for smart selection
        }
        else
        {
            InstantiateCards(); // Original random card instantiation
        }
        
         yield return new WaitForSeconds(1f);
        StartTurn();
    }


    [ContextMenu("Start Turn")]
    public void StartTurn(bool incrementBalls = true)
    {
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

        // Check if target achieved
        if (currentScore >= gameplayConfig.winScore)
        {
            Debug.Log("Game Over - Target achieved!");
            ScoreManager.Instance.enableRaycasterOnMainDialogueSystem();
            return;
        }

        if (ballerCard != null)
            Destroy(ballerCard);

        ballerCard = InstantiateBallerCard(CurrentBallThrow);
        BallThrowText.text = CurrentBallThrow.ToString();

        // Use smart card selection or traditional random draw
        if (useSmartCardSelection)
        {
            DrawSmartCards();
        }
        else
        {
            // Original random card drawing
            for (int i = 0; i < maxHandSize; i++)
            {
                DrawCard();
            }
        }

        Timer.Instance.StartTurnTimer();
        OnTurnStarted?.Invoke();
    }
    
    void DrawSmartCards()
    {
        Debug.Log("target score :" + ScoreManager.Instance.TargetScore);
        Debug.Log("current score :" + ScoreManager.Instance.currentRuns);
        Debug.Log("Max balls: " + ScoreManager.Instance.MaxBalls);
        Debug.Log("CurrentTurn: " + CurrntTurn);
        // Calculate run rate
       float runRate = ScoreManager.Instance.MaxBalls - CurrntTurn > 0 
        ? (float)(ScoreManager.Instance.TargetScore - ScoreManager.Instance.currentRuns) 
            / (ScoreManager.Instance.MaxBalls - CurrntTurn) 
        : 0f;
        
        // Get smart card selection based on current game state
        List<AttackCardData> smartSelection = CardSelectionLogic.GetSmartCardSelection(
            CurrentBallThrow,
            maxHandSize,
            runRate,
            CurrntTurn,
            ScoreManager.Instance.MaxBalls,
            ScoreManager.Instance.currentRuns
        );
        
        Debug.Log($"Drawing Smart Cards - Run Rate: {runRate:F2}, Ball: {CurrentBallThrow.ballLength} {CurrentBallThrow.ballLine}");
        
        // Clear current hand
        foreach (var card in HandCards)
        {
            if (card != null && card.gameObject != null)
            {
                card.gameObject.SetActive(false);
            }
        }
        HandCards.Clear();
        
        // Add the smart selected cards to hand
        foreach (var cardData in smartSelection)
        {
            AttackCardProps cardProp = GetOrCreateCardProp(cardData.excelBattinStrategy);
            
            if (cardProp != null)
            {
                // Move from draw pile or discard pile to hand
                if (DrawPile.Contains(cardProp))
                {
                    DrawPile.Remove(cardProp);
                }
                else if (DiscardPile.Contains(cardProp))
                {
                    DiscardPile.Remove(cardProp);
                }
                
                HandCards.Add(cardProp);
                cardProp.gameObject.SetActive(true);
            }
        }
        
        // Refresh hand arrangement
        SimpleHandArcManager arcManager = hand.GetComponent<SimpleHandArcManager>();
        if (arcManager != null)
            arcManager.RefreshCardArrangement();
    }
    
    AttackCardProps GetOrCreateCardProp(BattingStrategy strategy)
    {
        // Check if we already have this card created
        if (cardPropsMap.ContainsKey(strategy))
        {
            return cardPropsMap[strategy];
        }
        
        // Create new card if not exists
        AttackCardProps card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps>();
        card.cardData = new AttackCardData(strategy);
        cardPropsMap[strategy] = card;
        card.gameObject.SetActive(false);
        
        return card;
    }
    
    void InstantiateAllPossibleCards()
    {
        DrawPile.Clear();
        cardPropsMap.Clear();
        
        // Create one card for each possible batting strategy
        foreach (BattingStrategy strategy in System.Enum.GetValues(typeof(BattingStrategy)))
        {
            AttackCardProps card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps>();
            card.cardData = new AttackCardData(strategy);
            cardPropsMap[strategy] = card;
            DrawPile.Add(card);
            card.gameObject.SetActive(false);
        }
        
        Debug.Log($"Created {cardPropsMap.Count} unique cards for smart selection");
    }
    
    [ContextMenu("End Turn")]
    public void EndTurn(int maxBallsToBall, bool isNormalDelivery = true)
    {
        StartCoroutine(EndTurnWithAnimation( maxBallsToBall, isNormalDelivery));
    }

    private IEnumerator EndTurnWithAnimation(int maxBallsToBall, bool isNormalDelivery)
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
        //Increment current turn if it's a normal delivery i.d not wide or no ball
        ballCount++;
        if(isNormalDelivery)
            CurrntTurn++;
        
        // if (CurrntTurn >= maxBallsToBall)
        //     ScoreManager.Instance.UpdateBallsAndOvers(CurrntTurn);
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

        // Use smart redraw or normal redraw
        if (useSmartCardSelection)
        {
            DrawSmartCards();
        }
        else
        {
            // Draw new cards normally
            for (int i = 0; i < maxHandSize; i++)
            {
                DrawCard();
            }
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
            bool validBallGenerated = false;
            int attempts = 0;
            const int MAX_ATTEMPTS = 100; // Increased attempts for better chance of success

            while (!validBallGenerated && attempts < MAX_ATTEMPTS)
            {
                var candidateBall = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
                attempts++;

                bool isValid = true;

                // Check yorker constraint (1 per 10 balls)
                if (candidateBall.ballLength == BallLength.Yorker)
                {
                    int yorkerCheckStart = Mathf.Max(0, i - 9);
                    for (int j = yorkerCheckStart; j < i; j++)
                    {
                        if (j < BallThrows.Count && BallThrows[j].ballLength == BallLength.Yorker)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                // Check extreme line constraint (1 per 20 balls)
                if (isValid && (candidateBall.ballLine == BallLine.WayOutsideOff || candidateBall.ballLine == BallLine.WayDowntheLeg))
                {
                    int lineCheckStart = Mathf.Max(0, i - 19);
                    for (int j = lineCheckStart; j < i; j++)
                    {
                        if (j < BallThrows.Count &&
                            (BallThrows[j].ballLine == BallLine.WayOutsideOff || BallThrows[j].ballLine == BallLine.WayDowntheLeg))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    ballThrow = candidateBall;
                    validBallGenerated = true;
                }
            }

            // If we couldn't generate a valid ball, force generate one that's not extreme
            if (!validBallGenerated)
            {
                Debug.LogWarning($"Could not generate valid ball after {attempts} attempts at ball {i + 1}, forcing safe ball generation");

                // Force generate a "safe" ball that doesn't violate constraints
                attempts = 0;
                do
                {
                    ballThrow = ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
                    attempts++;

                    if (attempts > 100)
                    {
                        Debug.LogError($"Critical: Cannot generate non-extreme balls. Check your GetRandomBallThrow probability distribution!");
                        // Create a default safe ball as last resort
                        // You'll need to adjust this based on your BallThrow structure
                        ballThrow = new BallThrow()
                        {
                            ballLength = BallLength.FullLength, // Or whatever your default safe length is
                            ballLine = BallLine.OffStump  // Or whatever your default safe line is
                        };
                        break;
                    }
                }
                while (ballThrow.ballLine == BallLine.WayOutsideOff ||
                    ballThrow.ballLine == BallLine.WayDowntheLeg ||
                    ballThrow.ballLength == BallLength.Yorker);
            }
            
            BallThrows.Add(ballThrow);
        }

        for(int i=0; i< BallThrows.Count; i++)
        {
            Debug.Log("ball with length " + BallThrows[i].ballLength + " and line " + BallThrows[i].ballLine);
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
                return BallThrows[ballCount % BallThrows.Count]; // Cycle through BallThrows based on current turn
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

    // Debug method to test smart card selection
    [ContextMenu("Test Smart Card Selection")]
    public void TestSmartCardSelection()
    {
        if (CurrentBallThrow == null)
        {
            Debug.LogError("No current ball throw to test with!");
            return;
        }
        
        float[] testRunRates = { 0.3f, 0.7f, 1.2f, 1.6f, 2.0f };
        
        foreach (float runRate in testRunRates)
        {
            Debug.Log($"\n===== Testing Run Rate: {runRate:F2} =====");
            debugRunRateOverride = runRate;
            
            List<AttackCardData> cards = CardSelectionLogic.GetSmartCardSelection(
                CurrentBallThrow,
                maxHandSize,
                runRate,
                CurrntTurn,
                ScoreManager.Instance.MaxBalls,
                ScoreManager.Instance.currentRuns
            );
            
            Debug.Log($"Cards selected for {CurrentBallThrow.ballLength} {CurrentBallThrow.ballLine}:");
            foreach (var card in cards)
            {
                Debug.Log($"  - {card.excelBattinStrategy}");
            }
        }
        
        debugRunRateOverride = -1; // Reset
    }
}