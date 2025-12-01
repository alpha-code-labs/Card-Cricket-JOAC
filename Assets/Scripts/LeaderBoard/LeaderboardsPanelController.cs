using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardsPanelController : MonoBehaviour
{
    [SerializeField] GameObject leaderboardsPanel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;
    
    [Header("Block Background Clicks")]
    [SerializeField] GameObject blockingPanel;
    
    [Header("Optional - Disable World Interactions")]
    [SerializeField] GameObject worldInteractionsParent;
    
    // ✅ NEW: No Internet Error Panel
    [Header("Error Handling UI")]
    [SerializeField] GameObject noInternetPanel;
    [SerializeField] TextMeshProUGUI noInternetMessageText;
    [SerializeField] Button noInternetCloseButton;

    private void Start()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        // ✅ NEW: No Internet close button
        if (noInternetCloseButton != null)
            noInternetCloseButton.onClick.AddListener(CloseNoInternetPanel);

        // Panel starts hidden
        if (leaderboardsPanel != null)
            leaderboardsPanel.SetActive(false);
            
        if (blockingPanel != null)
            blockingPanel.SetActive(false);
        
        // ✅ NEW: No Internet panel starts hidden
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
    }

    // ✅ NEW: Check Internet Connection
    private bool HasInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public void OpenPanel()
    {
        Debug.Log("🏆 Leaderboards button clicked...");
        
        // ✅ NEW: Check internet first
        if (!HasInternetConnection())
        {
            ShowNoInternetPanel("No internet connection.\nLeaderboards cannot be loaded.");
            return;
        }
        
        Debug.Log("🏆 Opening Leaderboards Panel...");
        
        // Enable blocking panel first
        if (blockingPanel != null)
            blockingPanel.SetActive(true);
            
        if (leaderboardsPanel != null)
            leaderboardsPanel.SetActive(true);
            
        // Disable world interactions
        DisableWorldInteractions();
    }

    public void ClosePanel()
    {
        Debug.Log("❌ Closing Leaderboards Panel...");
        
        if (leaderboardsPanel != null)
            leaderboardsPanel.SetActive(false);
            
        if (blockingPanel != null)
            blockingPanel.SetActive(false);
            
        // Re-enable world interactions
        EnableWorldInteractions();
    }
    
    // ✅ NEW: Show No Internet Panel
    private void ShowNoInternetPanel(string message)
    {
        Debug.Log($"📵 No Internet: {message}");
        
        // Enable blocking panel to prevent background clicks
        if (blockingPanel != null)
            blockingPanel.SetActive(true);
        
        if (noInternetPanel != null)
        {
            noInternetPanel.SetActive(true);
            
            if (noInternetMessageText != null)
                noInternetMessageText.text = message;
        }
        
        // Disable world interactions
        DisableWorldInteractions();
    }
    
    // ✅ NEW: Close No Internet Panel
    private void CloseNoInternetPanel()
    {
        Debug.Log("❌ Closing No Internet Panel...");
        
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
        
        if (blockingPanel != null)
            blockingPanel.SetActive(false);
        
        // Re-enable world interactions
        EnableWorldInteractions();
    }
    
    private void DisableWorldInteractions()
    {
        // Option 1: Disable parent GameObject
        if (worldInteractionsParent != null)
        {
            worldInteractionsParent.SetActive(false);
            return;
        }
        
        // Option 2: Disable all colliders on GoTo objects
        GoToLocationIntractionHandler[] handlers = FindObjectsOfType<GoToLocationIntractionHandler>();
        foreach (var handler in handlers)
        {
            Collider2D col = handler.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
        Debug.Log($"🔒 Disabled {handlers.Length} world interactions");
    }
    
    private void EnableWorldInteractions()
    {
        // Option 1: Enable parent GameObject
        if (worldInteractionsParent != null)
        {
            worldInteractionsParent.SetActive(true);
            return;
        }
        
        // Option 2: Enable all colliders on GoTo objects
        GoToLocationIntractionHandler[] handlers = FindObjectsOfType<GoToLocationIntractionHandler>();
        foreach (var handler in handlers)
        {
            Collider2D col = handler.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }
        Debug.Log($"🔓 Enabled {handlers.Length} world interactions");
    }

    private void OnDestroy()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
        
        // ✅ NEW: Remove no internet button listener
        if (noInternetCloseButton != null)
            noInternetCloseButton.onClick.RemoveListener(CloseNoInternetPanel);
    }
}