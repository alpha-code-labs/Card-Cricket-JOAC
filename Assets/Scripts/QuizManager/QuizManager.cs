using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Data")]
    public QuizData quizData;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public Image[] crossImages;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;
    public GameObject countdownContainer;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI totalQuestionsText; // Display total questions
    public TextMeshProUGUI attemptingText; // Display current question number

    [Header("Retry Panel UI")]
    public GameObject retryPanel;
    public GameObject topRightPanel;
    public Image characterImage;
    public TextMeshProUGUI dialogText;
    public Button retryButton;

    [Header("Winning Panel UI")]
    public GameObject winningPanel;
    public Slider progressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI congratulationsText;
    public Button nextButton; // Next button for progression
    public AudioClip excitingMusic;
    public GameObject confettiPrefab; // UI Image confetti piece prefab
    public Transform confettiParent; // Parent transform for spawning confetti

    [Header("Audio")]
    public AudioSource backgroundMusicSource;

    [Header("Background")]
    public Image backgroundImage;

    [Header("Retry Panel Settings")]
    public string dialogMessage = "This is non negotiable Captain. Try Again";
    public float typewriterSpeed = 0.05f;

    [Header("Testing/Debug")]
    public bool enableTestingMode = false;
    public int testCorrectAnswers = 50; // Set how many you want correct for testing
    public KeyCode testWinningKey = KeyCode.W; // Press W to trigger winning
    public KeyCode testLosingKey = KeyCode.L; // Press L to trigger losing

    // Game State Variables
    private int currentQuestionIndex = 0;
    private int wrongAnswersCount = 0;
    private int correctAnswersCount = 0;
    private float currentTimer;
    private bool isAnswering = false;
    private bool gameStarted = false;

    // Retry Panel Variables
    private bool isTyping = false;
    private bool dialogComplete = false;

    // Timer Configuration
    private readonly float[] questionTimers = { 20f, 18f, 15f, 12f, 10f };

    void Start()
    {
        InitializeQuiz();
    }

    void InitializeQuiz()
    {
        // Setup background music
        if (quizData.backgroundMusic != null && backgroundMusicSource != null)
        {
            backgroundMusicSource.clip = quizData.backgroundMusic;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }

        // Setup background image
        if (quizData.backgroundImage != null && backgroundImage != null)
        {
            backgroundImage.sprite = quizData.backgroundImage;
        }

        // Hide retry panel initially
        if (retryPanel != null)
            retryPanel.SetActive(false);

        // Hide winning panel initially
        if (winningPanel != null)
            winningPanel.SetActive(false);

        // Hide quiz UI initially
        SetQuizUIActive(false);

        // Start countdown
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownContainer.SetActive(true);

        // Show 3, 2, 1, GO!
        string[] countdownTexts = { "3", "2", "1", "GO!" };

        for (int i = 0; i < countdownTexts.Length; i++)
        {
            countdownText.text = countdownTexts[i];
            yield return new WaitForSeconds(1f);
        }

        // Hide countdown and start quiz
        countdownContainer.SetActive(false);
        SetQuizUIActive(true);
        gameStarted = true;

        StartQuiz();
    }

    void StartQuiz()
    {
        currentQuestionIndex = 0;
        wrongAnswersCount = 0;
        correctAnswersCount = 0;

        // Setup answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int buttonIndex = i; // Capture for closure
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }

        LoadQuestion();
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= quizData.questions.Length)
        {
            EndQuiz();
            return;
        }

        QuizData.Question question = quizData.questions[currentQuestionIndex];

        // Display question
        questionText.text = question.questionText;

        // Update question counters
        UpdateQuestionCounters();

        // Display answer options
        for (int i = 0; i < answerButtons.Length && i < question.options.Length; i++)
        {
            answerTexts[i].text = question.options[i];
            answerButtons[i].interactable = true;
        }

        // Set timer based on question number
        SetTimerForQuestion();

        // Start question timer
        isAnswering = true;
        StartCoroutine(QuestionTimer());
    }

    void UpdateQuestionCounters()
    {
        // Update total questions display
        if (totalQuestionsText != null)
        {
            totalQuestionsText.text = $"{quizData.totalQuestions}";
        }

        // Update current question attempt display
        if (attemptingText != null)
        {
            attemptingText.text = $"{currentQuestionIndex + 1}";
        }
    }

    void SetTimerForQuestion()
    {
        int timerIndex = 0;

        if (currentQuestionIndex >= 40) timerIndex = 4; // Questions 41-50: 10 seconds
        else if (currentQuestionIndex >= 30) timerIndex = 3; // Questions 31-40: 12 seconds  
        else if (currentQuestionIndex >= 20) timerIndex = 2; // Questions 21-30: 15 seconds
        else if (currentQuestionIndex >= 10) timerIndex = 1; // Questions 11-20: 18 seconds
        else timerIndex = 0; // Questions 1-10: 20 seconds

        currentTimer = questionTimers[timerIndex];
    }

    IEnumerator QuestionTimer()
    {
        while (currentTimer > 0 && isAnswering)
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString();
            currentTimer -= Time.deltaTime;
            yield return null;
        }

        if (isAnswering)
        {
            // Time up - treat as wrong answer
            OnTimeUp();
        }
    }

    void OnTimeUp()
    {
        isAnswering = false;
        ProcessWrongAnswer();
    }

    public void OnAnswerSelected(int selectedIndex)
    {
        if (!isAnswering) return;

        isAnswering = false;

        // Disable all buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        QuizData.Question currentQuestion = quizData.questions[currentQuestionIndex];

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            // Correct answer
            correctAnswersCount++;
            StartCoroutine(NextQuestionDelay());
        }
        else
        {
            // Wrong answer
            ProcessWrongAnswer();
        }
    }

    void ProcessWrongAnswer()
    {
        wrongAnswersCount++;
        UpdateCrossDisplay();

        if (wrongAnswersCount >= quizData.maxWrongAnswers)
        {
            // Game over - show retry panel
            Debug.Log("Game Over - No percentage saved (user lost)");
            ShowRetryPanel();
        }
        else
        {
            StartCoroutine(NextQuestionDelay());
        }
    }

    void UpdateCrossDisplay()
    {
        // Highlight/show the cross for wrong answer
        if (wrongAnswersCount <= crossImages.Length)
        {
            // Make the cross red/highlighted to show it's used
            crossImages[wrongAnswersCount - 1].color = Color.red;
        }
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(1f); // Brief pause before next question
        currentQuestionIndex++;
        LoadQuestion();
    }

    void EndQuiz()
    {
        // User completed all 50 questions without 5 wrong answers = WIN
        Debug.Log("=== QUIZ WON! ===");
        Debug.Log($"Correct Answers: {correctAnswersCount}/50");
        Debug.Log($"Wrong Answers: {wrongAnswersCount}/5");

        // Calculate winning percentage
        float percentage = CalculateWinningPercentage(correctAnswersCount);

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("QuizPercentage", percentage);
        PlayerPrefs.Save();

        Debug.Log($"Percentage Saved: {percentage}%");
        Debug.Log($"Saved to PlayerPrefs with key: 'QuizPercentage'");
        Debug.Log("================");

        // Show winning panel
        ShowWinningPanel(percentage);
    }

    float CalculateWinningPercentage(int correctAnswers)
    {
        float percentage = 0f;

        switch (correctAnswers)
        {
            case 45: percentage = 90f; break;
            case 46: percentage = 92f; break;
            case 47: percentage = 94f; break;
            case 48: percentage = 96f; break;
            case 49: percentage = 98f; break;
            case 50: percentage = 100f; break;
            default:
                // Fallback calculation
                percentage = ((float)correctAnswers / 50f) * 100f;
                break;
        }

        Debug.Log($"Correct: {correctAnswers} → Percentage: {percentage}%");
        return percentage;
    }

    void ShowWinningPanel(float finalPercentage)
    {
        // Hide quiz UI
        SetQuizUIActive(false);

        // Show winning panel
        if (winningPanel != null)
            winningPanel.SetActive(true);

        // Start exciting music
        if (excitingMusic != null && backgroundMusicSource != null)
        {
            backgroundMusicSource.loop = false;
            backgroundMusicSource.clip = excitingMusic;
            backgroundMusicSource.Play();
        }

        // Hide final elements initially
        if (percentageText != null)
            percentageText.gameObject.SetActive(false);
        if (congratulationsText != null)
            congratulationsText.gameObject.SetActive(false);
        if (nextButton != null)
            nextButton.gameObject.SetActive(false); // Hide next button initially

        // Start loading animation
        StartCoroutine(WinningSequence(finalPercentage));
    }

    IEnumerator WinningSequence(float targetPercentage)
    {
        Debug.Log("Starting winning sequence...");

        // Step 1: Show loading text
        if (loadingText != null)
            loadingText.text = "Calculating Percentage...";

        // Step 2: Animate progress bar
        if (progressBar != null)
        {
            progressBar.value = 0f;
            float currentValue = 0f;

            while (currentValue < targetPercentage)
            {
                currentValue += Time.deltaTime * 25f; // Animation speed
                progressBar.value = currentValue / 100f; // Slider expects 0-1

                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(currentValue)}%";

                yield return null;
            }

            // Final values
            progressBar.value = targetPercentage / 100f;
            if (loadingText != null)
                loadingText.text = $"Complete! {targetPercentage:F0}%";
        }

        yield return new WaitForSeconds(1f);

        // Step 3: Hide loading, show percentage
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
        if (progressBar != null)
            progressBar.gameObject.SetActive(false);

        if (percentageText != null)
        {
            percentageText.gameObject.SetActive(true);
            percentageText.text = $"Your Percentage: {targetPercentage:F0}%";
        }

        yield return new WaitForSeconds(1f);

        // Step 4: Show congratulations with confetti
        if (congratulationsText != null)
        {
            congratulationsText.gameObject.SetActive(true);
            congratulationsText.text = "Congratulations!";
        }

        // Trigger UI confetti effect
        if (confettiPrefab != null && confettiParent != null)
        {
            StartCoroutine(SpawnUIConfetti());
        }
        else
        {
            Debug.Log("Confetti prefab or parent not assigned!");
        }

        yield return new WaitForSeconds(2f);

        // Step 5: Show Next button after celebration
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);

            // Setup next button click
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonPressed134);

            // Animate next button appearance
            StartCoroutine(AnimateNextButtonAppear());
        }

        Debug.Log("Winning sequence complete - Next button ready");
        // TODO: Transition to next Day Wise sequence
    }
    IEnumerator AnimateNextButtonAppear()
    {
        if (nextButton != null)
        {
            nextButton.transform.localScale = Vector3.zero;

            float timer = 0f;
            float duration = 0.3f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, timer / duration);
                nextButton.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            nextButton.transform.localScale = Vector3.one;
        }
    }

    public void OnNextButtonPressed134()
    {
        Debug.Log("Next button pressed - Transitioning to next Day Wise sequence");
      
    }
    void ShowRetryPanel()
    {
        // Hide quiz UI
        SetQuizUIActive(false);

        // Show retry panel
        if (retryPanel != null)
            retryPanel.SetActive(true);

        // Setup retry button
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryPressed);
            retryButton.gameObject.SetActive(false); // Hide initially
        }

        // Make sure top right panel is visible
        if (topRightPanel != null)
            topRightPanel.SetActive(true);

        // Clear dialog text initially
        if (dialogText != null)
            dialogText.text = "";

        // Start typewriter effect
        StartCoroutine(TypewriterEffect());
    }

    IEnumerator TypewriterEffect()
    {
        isTyping = true;
        if (dialogText != null)
            dialogText.text = "";

        // Type each character one by one
        for (int i = 0; i < dialogMessage.Length; i++)
        {
            if (dialogText != null)
                dialogText.text += dialogMessage[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        dialogComplete = true;

        // Show retry button after dialog is complete
        yield return new WaitForSeconds(0.5f);
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
            StartCoroutine(AnimateButtonAppear());
        }
    }

    IEnumerator AnimateButtonAppear()
    {
        if (retryButton != null)
        {
            retryButton.transform.localScale = Vector3.zero;

            float timer = 0f;
            float duration = 0.3f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1f, timer / duration);
                retryButton.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            retryButton.transform.localScale = Vector3.one;
        }
    }

    public void OnRetryPressed()
    {
        if (!dialogComplete) return;

        // Hide retry panel
        if (retryPanel != null)
            retryPanel.SetActive(false);

        // Reset game state
        currentQuestionIndex = 0;
        wrongAnswersCount = 0;
        correctAnswersCount = 0;
        isTyping = false;
        dialogComplete = false;

        // Reset cross images to normal
        for (int i = 0; i < crossImages.Length; i++)
        {
            crossImages[i].color = Color.white;
        }

        // Restart quiz
        SetQuizUIActive(true);
        StartQuiz();
    }

    void SetQuizUIActive(bool active)
    {
        timerText.transform.parent.gameObject.SetActive(active);
        crossImages[0].transform.parent.gameObject.SetActive(active);
        questionText.transform.parent.gameObject.SetActive(active);
        answerButtons[0].transform.parent.gameObject.SetActive(active);
    }

    void SetAnswerButtonsActive(bool active)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(active);
        }
    }

    void Update()
    {
        // Testing shortcuts during gameplay
        if (enableTestingMode && gameStarted)
        {
            // Press W to instantly win with specified correct answers
            if (Input.GetKeyDown(testWinningKey))
            {
                TestWinningScenario();
            }

            // Press L to instantly lose (trigger retry panel)
            if (Input.GetKeyDown(testLosingKey))
            {
                TestLosingScenario();
            }

            // Press number keys to set specific correct answers
            if (Input.GetKeyDown(KeyCode.Alpha1)) TestWinWithCorrect(45); // 90%
            if (Input.GetKeyDown(KeyCode.Alpha2)) TestWinWithCorrect(46); // 92%
            if (Input.GetKeyDown(KeyCode.Alpha3)) TestWinWithCorrect(47); // 94%
            if (Input.GetKeyDown(KeyCode.Alpha4)) TestWinWithCorrect(48); // 96%
            if (Input.GetKeyDown(KeyCode.Alpha5)) TestWinWithCorrect(49); // 98%
            if (Input.GetKeyDown(KeyCode.Alpha6)) TestWinWithCorrect(50); // 100%
        }

        // Skip typewriter effect on click during retry panel
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            // Skip typewriter effect and show full text immediately
            StopAllCoroutines();
            if (dialogText != null)
                dialogText.text = dialogMessage;
            isTyping = false;
            dialogComplete = true;
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(true);
                retryButton.transform.localScale = Vector3.one;
            }
        }
    }

 

    void TestWinningScenario()
    {
        Debug.Log($"=== TESTING: Triggering WIN with {testCorrectAnswers} correct answers ===");
        correctAnswersCount = testCorrectAnswers;
        wrongAnswersCount = 50 - testCorrectAnswers;
        EndQuiz();
    }

    void TestLosingScenario()
    {
        Debug.Log("=== TESTING: Triggering LOSE (5 wrong answers) ===");
        wrongAnswersCount = 5;
        ShowRetryPanel();
    }

    void TestWinWithCorrect(int correctCount)
    {
        Debug.Log($"=== TESTING: Triggering WIN with exactly {correctCount} correct answers ===");
        correctAnswersCount = correctCount;
        wrongAnswersCount = 50 - correctCount;
        EndQuiz();
    }

    // Public methods for debugging or external calls
    public int GetCurrentQuestionNumber()
    {
        return currentQuestionIndex + 1;
    }

    public int GetCorrectAnswers()
    {
        return correctAnswersCount;
    }

    public int GetWrongAnswers()
    {
        return wrongAnswersCount;
    }

    public float GetCurrentTimer()
    {
        return currentTimer;
    }

    IEnumerator SpawnUIConfetti()
    {
        if (confettiPrefab == null || confettiParent == null)
        {
            Debug.Log("Confetti prefab or parent not assigned!");
            yield break;
        }

        Debug.Log("Spawning UI confetti...");

        // Spawn multiple confetti pieces
        for (int i = 0; i < 20; i++)
        {
            // Create confetti piece
            GameObject confetti = Instantiate(confettiPrefab);

            // Set parent after instantiation
            confetti.transform.SetParent(confettiParent);
            confetti.transform.localScale = Vector3.one;

            // Get RectTransform and set position
            RectTransform rectTransform = confetti.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(
                    Random.Range(-400f, 400f), // Random X position
                    600f // Start at top
                );

                // Animate falling
                StartCoroutine(AnimateConfettiPiece(rectTransform));
            }

            yield return new WaitForSeconds(0.1f); // Small delay between spawns
        }
    }

    IEnumerator AnimateConfettiPiece(RectTransform confettiRect)
    {
        float fallSpeed = Random.Range(200f, 400f);
        float rotationSpeed = Random.Range(90f, 180f);
        float sidewaysDrift = Random.Range(-50f, 50f);

        Vector2 startPos = confettiRect.anchoredPosition;
        float rotation = 0f;

        // Fall animation
        while (confettiRect.anchoredPosition.y > -700f)
        {
            // Move down with sideways drift
            Vector2 currentPos = confettiRect.anchoredPosition;
            currentPos.y -= fallSpeed * Time.deltaTime;
            currentPos.x += sidewaysDrift * Time.deltaTime;
            confettiRect.anchoredPosition = currentPos;

            // Rotate
            rotation += rotationSpeed * Time.deltaTime;
            confettiRect.rotation = Quaternion.Euler(0, 0, rotation);

            yield return null;
        }

        // Destroy after falling
        Destroy(confettiRect.gameObject);
    }
}