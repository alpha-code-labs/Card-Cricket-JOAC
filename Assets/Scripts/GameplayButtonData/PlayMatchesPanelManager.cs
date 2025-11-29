using UnityEngine;
using UnityEngine.UI;

public class PlayMatchesPanelManager : MonoBehaviour
{
    [SerializeField] GameObject playMatchesPanel;
    [SerializeField] Button closeButton;
    [SerializeField] GameplayStatsDisplay gameplayStatsDisplay; // ✅ ADD THIS

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
        
        EnsurePlayerStatsTrackerExists();
    }

    void EnsurePlayerStatsTrackerExists()
    {
        if (PlayerStatsTracker.Instance == null)
        {
            Debug.Log("⚠️ PlayerStatsTracker not found - creating one...");
            GameObject trackerGO = new GameObject("PlayerStatsTracker");
            trackerGO.AddComponent<PlayerStatsTracker>();
            Debug.Log("✅ Created PlayerStatsTracker");
        }
    }

    public void OpenPlayMatchesPanel()
    {
        Debug.Log("🎮 Opening Play Matches panel...");
        playMatchesPanel.SetActive(true);
        
        EnsurePlayerStatsTrackerExists();
        ReloadDataFromJSON();
        
        // ✅ Refresh all stats with one call
        gameplayStatsDisplay.RefreshAllStats();
    }

    void ReloadDataFromJSON()
    {
        Debug.Log("📂 Reloading stats from JSON...");
        
        if (PlayerStatsTracker.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerStatsTracker not ready");
            return;
        }

        var playerStats = PlayerStatsTracker.Instance.GetPlayerStats();
        if (playerStats == null)
        {
            Debug.LogError("❌ PlayerStats is null");
            return;
        }

        var savedData = PlayerStatsSaveSystem.LoadStats();
        PlayerStatsSaveSystem.LoadIntoPlayerStats(playerStats, savedData);
        
        Debug.Log($"✅ Reloaded from JSON");
    }

    void OnCloseClicked()
    {
        Debug.Log("❌ Closing Play Matches panel...");
        playMatchesPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }
}