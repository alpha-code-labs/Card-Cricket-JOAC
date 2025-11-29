using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class LeaderboardBattingAverageManager : MonoBehaviour
{
    private static LeaderboardBattingAverageManager instance;
    private FirebaseFirestore db;
    
    [SerializeField] private Transform leaderboardContent; // ScrollView Content
    [SerializeField] private GameObject leaderboardStripPrefab; // LeaderboardStrip prefab
    [SerializeField] private TextMeshProUGUI loadingText; // Optional: Show "Loading..." message
    [SerializeField] private GameObject leaderboardPanel; // Your leaderboard panel
    
    private List<LeaderboardEntry> leaderboardData = new List<LeaderboardEntry>();
    private string battingAveragesCollection = "battingAverages";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        // Data will load when button is clicked, not automatically
    }

    // ✅ ADD DUMMY DATA FOR TESTING
    public void AddDummyData()
    {
        leaderboardData.Clear();

        // Add dummy entries
        leaderboardData.Add(new LeaderboardEntry { userName = "Mukesh", battingAverage = 45.5f });
        leaderboardData.Add(new LeaderboardEntry { userName = "Raju", battingAverage = 78.3f });
        leaderboardData.Add(new LeaderboardEntry { userName = "Arjun", battingAverage = 62.1f });
        leaderboardData.Add(new LeaderboardEntry { userName = "Virat", battingAverage = 89.7f });
        leaderboardData.Add(new LeaderboardEntry { userName = "Rohit", battingAverage = 55.4f });

        // Sort in descending order
        leaderboardData = leaderboardData.OrderByDescending(x => x.battingAverage).ToList();

        // Display leaderboard
        DisplayLeaderboard();

        Debug.Log($"✅ Dummy data loaded - {leaderboardData.Count} entries");
    }

    // ✅ FETCH LEADERBOARD DATA FROM FIRESTORE
    public void FetchAndDisplayLeaderboard()
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        if (loadingText != null)
            loadingText.text = "Loading Leaderboard...";

        leaderboardData.Clear();

        db.Collection(battingAveragesCollection)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    QuerySnapshot snapshot = task.Result;

                    // ✅ EXTRACT DATA FROM FIRESTORE
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        try
                        {
                            string userName = document.GetValue<string>("userName");
                            float battingAverage = document.GetValue<double>("battingAverage").ToString() == "" ? 0 : (float)document.GetValue<double>("battingAverage");

                            leaderboardData.Add(new LeaderboardEntry
                            {
                                userName = userName,
                                battingAverage = battingAverage
                            });

                            Debug.Log($"✅ Loaded: {userName} - {battingAverage:F1}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"⚠️ Error parsing document: {e.Message}");
                        }
                    }

                    // ✅ SORT IN DESCENDING ORDER (Highest batting average = Rank 1)
                    leaderboardData = leaderboardData.OrderByDescending(x => x.battingAverage).ToList();

                    // ✅ DISPLAY LEADERBOARD
                    DisplayLeaderboard();

                    if (loadingText != null)
                        loadingText.text = "";
                }
                else
                {
                    Debug.LogError($"❌ Error fetching leaderboard: {task.Exception}");
                    if (loadingText != null)
                        loadingText.text = "Error Loading Leaderboard";
                }
            });
    }

    // ✅ DISPLAY LEADERBOARD IN UI
    private void DisplayLeaderboard()
    {
        // Clear existing entries
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // ✅ SET CONTENT HEIGHT DYNAMICALLY BASED ON ENTRIES (BEFORE LOOP)
        RectTransform contentRect = leaderboardContent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            float totalHeight = (leaderboardData.Count * 133) + 200;
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
            Debug.Log($"✅ Content height set to: {totalHeight}");
        }

        // ✅ CREATE LEADERBOARD ENTRIES WITH RANK (1, 2, 3...)
        for (int i = 0; i < leaderboardData.Count; i++)
        {
            int rank = i + 1; // Rank starts from 1
            
            GameObject stripInstance = Instantiate(leaderboardStripPrefab, leaderboardContent);
            
            // ✅ SET Y POSITION MANUALLY FOR SPACING (133 pixels each)
            RectTransform stripRect = stripInstance.GetComponent<RectTransform>();
            if (stripRect != null)
            {
                float yPosition = -88 - (i * 133); // First entry at -140, then -273, -406, etc.
                stripRect.anchoredPosition = new Vector2(stripRect.anchoredPosition.x, yPosition);
            }
            
            // ✅ FIND TEXTMESHPRO COMPONENTS BY NAME
            TextMeshProUGUI rankText = FindTextMeshProByName(stripInstance, "RankText");
            TextMeshProUGUI userNameText = FindTextMeshProByName(stripInstance, "UserName");
            TextMeshProUGUI battingAverageText = FindTextMeshProByName(stripInstance, "BattingAverage");
            
            if (rankText != null && userNameText != null && battingAverageText != null)
            {
                rankText.text = rank.ToString();
                userNameText.text = leaderboardData[i].userName;
                battingAverageText.text = leaderboardData[i].battingAverage.ToString("F1");
                
                Debug.Log($"✅ Displayed Rank {rank}: {leaderboardData[i].userName} - Batting Average: {leaderboardData[i].battingAverage:F1} at Y: {stripRect.anchoredPosition.y}");
            }
            else
            {
                // Debug: Show what TextMeshPro components actually exist
                TextMeshProUGUI[] allTexts = stripInstance.GetComponentsInChildren<TextMeshProUGUI>();
                Debug.LogError($"❌ Could not find all TextMeshPro components. Found {allTexts.Length} components:");
                for (int j = 0; j < allTexts.Length; j++)
                {
                    Debug.LogError($"   TextMeshPro {j}: Name = '{allTexts[j].gameObject.name}', Text = '{allTexts[j].text}'");
                }
                Debug.LogError($"Looking for: 'RankText', 'UserName', 'BattingAverage'");
            }
        }

        Debug.Log($"✅ Leaderboard displayed with {leaderboardData.Count} total entries");
    }

    // ✅ HELPER METHOD TO FIND TEXTMESHPRO BY NAME
    private TextMeshProUGUI FindTextMeshProByName(GameObject parent, string textName)
    {
        TextMeshProUGUI[] allTexts = parent.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.gameObject.name == textName)
            {
                return text;
            }
        }
        return null;
    }

    public static LeaderboardBattingAverageManager GetInstance()
    {
        return instance;
    }

    // ✅ OPEN LEADERBOARD PANEL AND FETCH DATA
    public void OpenLeaderboardPanel()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            Debug.Log("✅ Leaderboard panel opened");
        }
        FetchAndDisplayLeaderboard();
    }

    // ✅ CLOSE LEADERBOARD PANEL
    public void CloseLeaderboardPanel()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
            Debug.Log("✅ Leaderboard panel closed");
        }
    }
}