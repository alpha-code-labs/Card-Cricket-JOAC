using UnityEngine;
using System.Linq;
using Yarn.Unity;

/// <summary>
/// Attach this script to a GameObject in your scene (like a GameManager)
/// It will automatically disable any persistent Dialogue Systems from previous scenes
/// This prevents input conflicts between multiple dialogue systems
/// </summary>
public class DialogueSystemConflictResolver : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool disableOnAwake = true;
    [SerializeField] private bool debugMode = true;
    
    private static GameObject persistentDialogueSystem;
    
    void Awake()
    {
        if (disableOnAwake)
        {
            DisablePersistentDialogueSystems();
        }
    }
    
    void Start()
    {
        // Double-check in Start in case something was missed in Awake
        if (!disableOnAwake)
        {
            DisablePersistentDialogueSystems();
        }
    }
    
    /// <summary>
    /// Finds and disables any Dialogue Systems that are from a different scene (DontDestroyOnLoad)
    /// </summary>
    public void DisablePersistentDialogueSystems()
    {
        // Get all GameObjects named "Dialogue System"
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System" || go.name.Contains("DialogueSystem"))
            .ToArray();

        if (debugMode)
        {
            Debug.Log($"[DialogueSystemConflictResolver] Found {dialogueSystems.Length} Dialogue System(s) in scene");
        }

        int disabledCount = 0;
        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                // Store reference before disabling
                persistentDialogueSystem = dialogueSystem;
                
                // Method 1: Disable the entire GameObject
                dialogueSystem.SetActive(false);
                disabledCount++;
                
                if (debugMode)
                {
                    Debug.Log($"[DialogueSystemConflictResolver] Disabled persistent Dialogue System from scene: {dialogueSystem.scene.name}");
                }
            }
            else if (debugMode)
            {
                Debug.Log($"[DialogueSystemConflictResolver] Found local Dialogue System in current scene - keeping it active");
            }
        }
        
        if (debugMode && disabledCount == 0 && dialogueSystems.Length > 1)
        {
            Debug.LogWarning("[DialogueSystemConflictResolver] Multiple Dialogue Systems found but none were from a different scene. This might still cause conflicts!");
        }
    }
    
    /// <summary>
    /// Re-enables the persistent dialogue system when needed (e.g., when leaving this scene)
    /// </summary>
    public void EnablePersistentDialogueSystem()
    {
        if (persistentDialogueSystem != null)
        {
            persistentDialogueSystem.SetActive(true);
            
            if (debugMode)
            {
                Debug.Log("[DialogueSystemConflictResolver] Re-enabled persistent Dialogue System");
            }
        }
    }
    
    /// <summary>
    /// Call this before changing scenes if you want to re-enable the persistent dialogue system
    /// </summary>
    void OnDestroy()
    {
        // Optionally re-enable when this scene is destroyed
        // Uncomment if you want automatic re-enabling
        EnablePersistentDialogueSystem();
    }
    
    /// <summary>
    /// Alternative approach: Only disable input components instead of the entire GameObject
    /// Use this if you need the persistent dialogue system to remain active but not receive input
    /// </summary>
    
}