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
    // ✅ ADD THESE
    [SerializeField] GameObject playMatchesPanel; // Your 15 buttons panel
    [SerializeField] Button playMatchesPanelCloseButton;
    

    [Header("Campaign Button")]
    [SerializeField] Button startCampaignButton;
    [SerializeField] Sprite startCampaignSprite;      
[SerializeField] Sprite continueCampaignSprite;      
    [SerializeField] Button playMatchesButton;
    
    [SerializeField] TMP_InputField usernameInputField;

    [SerializeField] PlayMatchesPanelManager playMatchesPanelManager;



   
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
        // ✅ ADD THIS
        InitializePlayMatchesPanel();
        // Invoke(nameof(), 0.1f);
        PlayMatchesButtonActivated();//remove from inovke
        if (startCampaignButton != null)
            startCampaignButton.onClick.AddListener(OnStartCampaignClicked);
        
        if (playMatchesButton != null)
            playMatchesButton.onClick.AddListener(OnPlayMatchesClicked);
      
       Invoke(nameof(DisplayUsername), 0.5f);
        // Invoke(nameof(UpdateCampaignButtonImage), 0.2f);
        UpdateCampaignButtonImage();
        // Invoke(nameof(UpdateCampaignButtonAndShow), 0.01f);
        UpdateCampaignButtonAndShow();
        // ResetButtonModeState();
    }

    // ✅ ADD THIS METHOD
    private void InitializePlayMatchesPanel()
    {
        // Setup button listeners
        if (continueButton != null)
        {
            Button btn = continueButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnPlayMatchesButtonClicked);
        }
        
        if (playMatchesPanelCloseButton != null)
            playMatchesPanelCloseButton.onClick.AddListener(OnPlayMatchesPanelCloseClicked);

        // Panel starts hidden
        if (playMatchesPanel != null)
            playMatchesPanel.SetActive(false);
        
        Debug.Log("✅ Play Matches Panel initialized");
    }



    // ✅ ADD THIS METHOD
   void OnPlayMatchesButtonClicked()
{
    Debug.Log("🎮 Opening Play Matches panel...");
    playMatchesPanelManager.OpenPlayMatchesPanel(); // ✅ This refreshes stats
    DisableMainButtons();
}

    // ✅ ADD THIS METHOD
    private void OnPlayMatchesPanelCloseClicked()
    {
        Debug.Log("❌ Closing Play Matches panel...");
        if (playMatchesPanel != null)
            playMatchesPanel.SetActive(false);
        
        EnableMainButtons();
    }

    // ✅ ADD THIS METHOD
public void DisableMainButtons()
{
    if (continueButton != null)
        continueButton.SetActive(false);
    if (startCampaignButton != null)
        startCampaignButton.interactable = false;
    if (LeaderBoardsButtonpanel != null)
        LeaderBoardsButtonpanel.interactable = false;
    if (userNameButton != null)
        userNameButton.interactable = false;
}

    // ✅ ADD THIS METHOD
public void EnableMainButtons()
{
    CheckAndShowContinueButton();  // ✅ Instead of continueButton.SetActive(true)
    
    if (startCampaignButton != null)
        startCampaignButton.interactable = true;
    if (LeaderBoardsButtonpanel != null)
        LeaderBoardsButtonpanel.interactable = true;
    if (userNameButton != null)
        userNameButton.interactable = true;
}

   void OnStartCampaignClicked()
{
     if (!string.IsNullOrEmpty(GameFlowManager.savedCampaignDate))
    {
        GameManager.instance.currentSaveData.currentDate = GameFlowManager.savedCampaignDate;
        Debug.Log($"✅ Restored campaign date: {GameFlowManager.savedCampaignDate}");
    }
    
    // ✅ RESET all button mode state
    GameFlowManager.isButtonMode = false;
    GameFlowManager.savedCampaignDate = "";
    GameFlowManager.buttonYarnNode = "";
    GameFlowManager.nextGameplayName = "";
    GameFlowManager.nextGameplayDate = "";
    
    NewDayManager.currentEventIndex = 0;
    NewDayManager.isEvening = false;
    
    Debug.Log("🎮 Starting normal campaign - isButtonMode = false");
    
    // SET flag on first campaign start
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
    
    // Reset NewDayManager static state
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
    Debug.Log($"📌 Continue button hidden by default{GameManager.instance.currentSaveData.hasCompletedChapter1}");
    // ✅ Check saved flag OR current node
    if (GameManager.instance.currentSaveData.hasCompletedChapter1 ||
        DialogueScriptCommandHandler.currentNode == "Scene135_01_Ch1End")
    {
        // Save flag if reaching chapter end for first time
        if (!GameManager.instance.currentSaveData.hasCompletedChapter1)
        {
            GameManager.instance.currentSaveData.hasCompletedChapter1 = true;
            SaveSystem.SaveDataToFile();
        }
        
      CheckAndShowContinueButton();
        Debug.Log("✅ Continue button shown - condition met");
    }
}

    private void InitializeUsernamePanel()
    {
        userNameButton.onClick.AddListener(OnUserNameButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        submitButton.onClick.AddListener(OnSubmitButtonClicked);

        userNamePanel.SetActive(false);
        
        Debug.Log("✅ Username Panel initialized");
    }
    
 public void DisableButtons()
{
    if (userNameButton != null)
        userNameButton.gameObject.SetActive(false);
    if (LeaderboardsPanel != null)
        LeaderboardsPanel.SetActive(false);
    if (continueButton != null)
        continueButton.SetActive(false);
}


    public void DisplayUsername()
    {
        if(usernameInputField != null)
        {
            usernameInputField.text = GameManager.instance.currentSaveData.userName;
            Debug.Log($"✅ Displayed username: {GameManager.instance.currentSaveData.userName}");
        }
    }
    
 public void EnableButtons()
{
    if (userNameButton != null)
        userNameButton.gameObject.SetActive(true);

}

    public void OnUserNameButtonClicked()
    {
        Debug.Log("🔓 Opening Username Panel...");
        userNamePanel.SetActive(true);
        DisableButtons();
    }

    private void OnCloseButtonClicked()
    {
        Debug.Log("❌ Closing Username Panel...");
        userNamePanel.SetActive(false);
        EnableButtons();
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

        OnCloseButtonClicked();
    }
private void OnLeaderboardsButtonClicked()
{
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
    
    // Hide other main menu elements
    if (continueButton != null)
        continueButton.SetActive(false);
    if (startCampaignButton != null)
        startCampaignButton.interactable = false;
    if (userNameButton != null)
        userNameButton.interactable = false;
    if (LeaderBoardsButtonpanel != null)
        LeaderBoardsButtonpanel.interactable = false;
}

public void OnLeaderboardsPanelCloseClicked()
{
    Debug.Log("❌ Closing Leaderboards Panel...");
    if (LeaderboardsPanel != null)
        LeaderboardsPanel.SetActive(false);
     CheckAndShowContinueButton(); 
    
    if (startCampaignButton != null)
        startCampaignButton.interactable = true;
    if (userNameButton != null)
        userNameButton.interactable = true;
    if (LeaderBoardsButtonpanel != null)
        LeaderBoardsButtonpanel.interactable = true;
}


private void CheckAndShowContinueButton()
{
    if (continueButton == null) return;
    
    // Only show if Chapter 1 is complete
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
    if (LeaderBoardsButtonpanel != null) 
    LeaderBoardsButtonpanel.onClick.RemoveListener(OnLeaderboardsButtonClicked);
}
}