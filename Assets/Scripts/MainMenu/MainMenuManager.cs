using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject continueButton;  
    [SerializeField] GameObject userNamePanel;
    [SerializeField] Button userNameButton;
    [SerializeField] Button closeButton;
    [SerializeField] Button submitButton;
    [SerializeField] Button LeaderboardsButton;
    [SerializeField] Button PlayerStatsButton;
    
    [SerializeField] Button LeaderBoardsButtonpanel;
    [SerializeField] GameObject LeaderboardsPanel;
    [SerializeField] GameObject playMatchesPanel;
    [SerializeField] Button playMatchesPanelCloseButton;
    
    [Header("Campaign Button")]
    [SerializeField] Button startCampaignButton;
    [SerializeField] Sprite startCampaignSprite;      
    [SerializeField] Sprite continueCampaignSprite;      
    [SerializeField] Button playMatchesButton;
    
    [SerializeField] TMP_InputField usernameInputField;

    [SerializeField] PlayMatchesPanelManager playMatchesPanelManager;

    [Header("No Internet UI")]
    [SerializeField] GameObject noInternetPanel;
    [SerializeField] TextMeshProUGUI noInternetMessageText;
    [SerializeField] Button noInternetCloseButton;

    private void Start()
    {
        if (startCampaignButton != null)
            startCampaignButton.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.SetActive(false);
        if (LeaderboardsPanel != null)
            LeaderboardsPanel.SetActive(false);
        if (LeaderBoardsButtonpanel != null)
            LeaderBoardsButtonpanel.onClick.AddListener(OnLeaderboardsButtonClicked);
        
        InitializeUsernamePanel();
        InitializePlayMatchesPanel();
        InitializeNoInternetPanel();
        
        PlayMatchesButtonActivated();
        
        if (startCampaignButton != null)
            startCampaignButton.onClick.AddListener(OnStartCampaignClicked);
        
        if (playMatchesButton != null)
            playMatchesButton.onClick.AddListener(OnPlayMatchesClicked);
      
        Invoke(nameof(DisplayUsername), 0.5f);
        UpdateCampaignButtonImage();
        UpdateCampaignButtonAndShow();
    }

    private void InitializeNoInternetPanel()
    {
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
        
        if (noInternetCloseButton != null)
            noInternetCloseButton.onClick.AddListener(OnNoInternetCloseClicked);
        
        Debug.Log("✅ No Internet Panel initialized");
    }

    private void ShowNoInternetPanel(string message)
    {
        if (noInternetPanel != null)
        {
            noInternetPanel.SetActive(true);
            
            if (noInternetMessageText != null)
                noInternetMessageText.text = message;
            
            Debug.Log($"📵 Showing No Internet Panel: {message}");
        }
        else
        {
            Debug.LogWarning("⚠️ No Internet Panel not assigned in Inspector");
        }
    }

    private void OnNoInternetCloseClicked()
    {
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
        
        EnableMainButtons();
        Debug.Log("❌ No Internet Panel closed");
    }

    private bool HasInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void InitializePlayMatchesPanel()
    {
        if (continueButton != null)
        {
            Button btn = continueButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnPlayMatchesButtonClicked);
        }
        
        if (playMatchesPanelCloseButton != null)
            playMatchesPanelCloseButton.onClick.AddListener(OnPlayMatchesPanelCloseClicked);

        if (playMatchesPanel != null)
            playMatchesPanel.SetActive(false);
        
        Debug.Log("✅ Play Matches Panel initialized");
    }

    void OnPlayMatchesButtonClicked()
    {
        Debug.Log("🎮 Play Matches button clicked...");
        
        if (!HasInternetConnection())
        {
            ShowNoInternetPanel("No internet connection.\nRanks cannot be loaded.");
            DisableMainButtons();
            return;
        }
        
        Debug.Log("🎮 Opening Play Matches panel...");
        playMatchesPanelManager.OpenPlayMatchesPanel();
        DisableMainButtons();
    }

    private void OnPlayMatchesPanelCloseClicked()
    {
        Debug.Log("❌ Closing Play Matches panel...");
        if (playMatchesPanel != null)
            playMatchesPanel.SetActive(false);
        
        EnableMainButtons();
    }

    // ═══════════════════════════════════════════════════════════════
    // ✅ FIXED: UNIFIED ENABLE/DISABLE METHODS
    // ═══════════════════════════════════════════════════════════════

    public void DisableMainButtons()
    {
        // Hide continueButton (it uses SetActive)
        if (continueButton != null)
            continueButton.SetActive(false);
        
        // Disable interactable buttons
        if (startCampaignButton != null)
            startCampaignButton.interactable = false;
        if (LeaderBoardsButtonpanel != null)
            LeaderBoardsButtonpanel.interactable = false;
        if (userNameButton != null)
            userNameButton.interactable = false;
        
        Debug.Log("🔒 Main buttons disabled");
    }

    public void EnableMainButtons()
    {
        // Show continueButton if condition met
        CheckAndShowContinueButton();
        
        // Enable interactable buttons
        if (startCampaignButton != null)
            startCampaignButton.interactable = true;
        if (LeaderBoardsButtonpanel != null)
            LeaderBoardsButtonpanel.interactable = true;
        if (userNameButton != null)
            userNameButton.interactable = true;
        
        Debug.Log("🔓 Main buttons enabled");
    }

    // ═══════════════════════════════════════════════════════════════
    // CAMPAIGN BUTTON
    // ═══════════════════════════════════════════════════════════════

    void OnStartCampaignClicked()
    {
        if (!string.IsNullOrEmpty(GameFlowManager.savedCampaignDate))
        {
            GameManager.instance.currentSaveData.currentDate = GameFlowManager.savedCampaignDate;
            Debug.Log($"✅ Restored campaign date: {GameFlowManager.savedCampaignDate}");
        }
        
        GameFlowManager.isButtonMode = false;
        GameFlowManager.savedCampaignDate = "";
        GameFlowManager.buttonYarnNode = "";
        GameFlowManager.nextGameplayName = "";
        GameFlowManager.nextGameplayDate = "";
        
        NewDayManager.currentEventIndex = 0;
        NewDayManager.isEvening = false;
        
        Debug.Log("🎮 Starting normal campaign - isButtonMode = false");
        
        if (!GameManager.instance.currentSaveData.hasCampaignStarted)
        {
            GameManager.instance.currentSaveData.hasCampaignStarted = true;
            SaveSystem.SaveDataToFile();
            Debug.Log("✅ First time campaign start - flag saved");
        }
        
        TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
    }

    private void ResetButtonModeState()
    {
        GameFlowManager.isButtonMode = false;
        GameFlowManager.buttonYarnNode = "";
        GameFlowManager.buttonSceneToLoad = "";
        GameFlowManager.nextGameplayName = "";
        GameFlowManager.nextGameplayDate = "";
        
        NewDayManager.currentEventIndex = 0;
        NewDayManager.isEvening = false;
        
        Debug.Log("🔄 Button mode state reset on Main Menu load");
    }

    private void UpdateCampaignButtonAndShow()
    {
        UpdateCampaignButtonImage();
        
        if (startCampaignButton != null)
            startCampaignButton.gameObject.SetActive(true);
    }

    private void UpdateCampaignButtonImage()
    {
        if (startCampaignButton == null) return;
        
        Image buttonImage = startCampaignButton.GetComponent<Image>();
        if (buttonImage == null) return;
        
        bool hasStarted = GameManager.instance.currentSaveData.hasCampaignStarted;
        
        buttonImage.sprite = hasStarted ? continueCampaignSprite : startCampaignSprite;
        
        Debug.Log(hasStarted ? "✅ Showing CONTINUE Campaign" : "✅ Showing START Campaign");
    }

    void OnPlayMatchesClicked()
    {
        Debug.Log("🎮 Play Matches clicked");
    }

    public void PlayMatchesButtonActivated()
    {
        Debug.Log("✅ Checking if Continue button should be shown...");
        continueButton.SetActive(false);
        Debug.Log($"📌 Continue button hidden by default - hasCompletedChapter1: {GameManager.instance.currentSaveData.hasCompletedChapter1}");
        
        if (GameManager.instance.currentSaveData.hasCompletedChapter1 ||
            DialogueScriptCommandHandler.currentNode == "Scene135_01_Ch1End")
        {
            if (!GameManager.instance.currentSaveData.hasCompletedChapter1)
            {
                GameManager.instance.currentSaveData.hasCompletedChapter1 = true;
                SaveSystem.SaveDataToFile();
            }
            
            CheckAndShowContinueButton();
            Debug.Log("✅ Continue button shown - condition met");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ✅ FIXED: USERNAME PANEL
    // ═══════════════════════════════════════════════════════════════

    private void InitializeUsernamePanel()
    {
        userNameButton.onClick.AddListener(OnUserNameButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        submitButton.onClick.AddListener(OnSubmitButtonClicked);

        userNamePanel.SetActive(false);
        
        Debug.Log("✅ Username Panel initialized");
    }

    public void OnUserNameButtonClicked()
    {
        Debug.Log("🔓 Opening Username Panel...");
        userNamePanel.SetActive(true);
        DisableMainButtons();  // ✅ FIXED: Use DisableMainButtons()
    }

    private void OnCloseButtonClicked()
    {
        Debug.Log("❌ Closing Username Panel...");
        userNamePanel.SetActive(false);
        EnableMainButtons();  // ✅ FIXED: Use EnableMainButtons()
    }

    private void OnSubmitButtonClicked()
    {
        string newUsername = usernameInputField.text.Trim();

        if (string.IsNullOrEmpty(newUsername))
        {
            Debug.LogWarning("⚠️ Username cannot be empty!");
            return;
        }

        if (newUsername.Length < 3)
        {
            Debug.LogWarning("⚠️ Username must be at least 3 characters!");
            return;
        }

        Debug.Log($"💾 Submitting new username: {newUsername}");

        GameManager.instance.currentSaveData.userName = newUsername;
        SaveSystem.SaveUsernameOnly();

        FirestoreManager firestoreManager = FirestoreManager.GetInstance();
        if (firestoreManager != null)
        {
            firestoreManager.SaveUsernameToFirestore(newUsername);
        }

        OnCloseButtonClicked();  // ✅ This now calls EnableMainButtons()
    }

    public void DisplayUsername()
    {
        if(usernameInputField != null)
        {
            usernameInputField.text = GameManager.instance.currentSaveData.userName;
            Debug.Log($"✅ Displayed username: {GameManager.instance.currentSaveData.userName}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ✅ FIXED: LEADERBOARDS PANEL
    // ═══════════════════════════════════════════════════════════════

    private void OnLeaderboardsButtonClicked()
    {
        Debug.Log("🏆 Leaderboards button clicked...");
        
        if (!HasInternetConnection())
        {
            ShowNoInternetPanel("No internet connection.\nLeaderboards cannot be loaded.");
            DisableMainButtons();
            return;
        }
        
        Debug.Log("🏆 Opening Leaderboards Panel...");
        if (LeaderboardsPanel != null)
        {
            LeaderboardsPanel.SetActive(true);
            Debug.Log("✅ LeaderboardsPanel is now active");
        }
        else
        {
            Debug.LogError("❌ LeaderboardsPanel is NULL! Assign it in Inspector.");
        }
        
        DisableMainButtons();  // ✅ FIXED: Use unified method
    }

    public void OnLeaderboardsPanelCloseClicked()
    {
        Debug.Log("❌ Closing Leaderboards Panel...");
        if (LeaderboardsPanel != null)
            LeaderboardsPanel.SetActive(false);
        
        EnableMainButtons();  // ✅ FIXED: Use unified method
    }

    // ═══════════════════════════════════════════════════════════════
    // CONTINUE BUTTON LOGIC
    // ═══════════════════════════════════════════════════════════════

    private void CheckAndShowContinueButton()
    {
        if (continueButton == null) return;
        
        if (GameManager.instance.currentSaveData.hasCompletedChapter1 ||
            DialogueScriptCommandHandler.currentNode == "Scene135_01_Ch1End")
        {
            continueButton.SetActive(true);
            Debug.Log("✅ Continue button shown - condition met");
        }
        else
        {
            continueButton.SetActive(false);
            Debug.Log("❌ Continue button hidden - condition NOT met");
        }
    }

    private void OnDestroy()
    {
        if (userNameButton != null) userNameButton.onClick.RemoveListener(OnUserNameButtonClicked);
        if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        if (startCampaignButton != null) startCampaignButton.onClick.RemoveListener(OnStartCampaignClicked);
        if (playMatchesButton != null) playMatchesButton.onClick.RemoveListener(OnPlayMatchesClicked);
        if (continueButton != null) continueButton.GetComponent<Button>().onClick.RemoveListener(OnPlayMatchesButtonClicked);
        if (playMatchesPanelCloseButton != null) playMatchesPanelCloseButton.onClick.RemoveListener(OnPlayMatchesPanelCloseClicked);
        if (LeaderBoardsButtonpanel != null) LeaderBoardsButtonpanel.onClick.RemoveListener(OnLeaderboardsButtonClicked);
        if (noInternetCloseButton != null) noInternetCloseButton.onClick.RemoveListener(OnNoInternetCloseClicked);
    }
}