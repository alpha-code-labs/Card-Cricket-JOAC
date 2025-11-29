using UnityEngine;
using UnityEngine.UI;

public class LeaderboardsPanelController : MonoBehaviour
{
    [SerializeField] GameObject leaderboardsPanel;
    [SerializeField] Button openButton;
    [SerializeField] Button closeButton;
    
    [Header("Block Background Clicks")]
    [SerializeField] GameObject blockingPanel; // Full screen invisible panel to block clicks
    
    [Header("Optional - Disable World Interactions")]
    [SerializeField] GameObject worldInteractionsParent; // Parent of all GoTo objects (MapSprite)

    private void Start()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Panel starts hidden
        if (leaderboardsPanel != null)
            leaderboardsPanel.SetActive(false);
            
        if (blockingPanel != null)
            blockingPanel.SetActive(false);
    }

    public void OpenPanel()
    {
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
    }
}