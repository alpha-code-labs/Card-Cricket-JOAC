using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[System.Serializable]
public class SpriteNameValidator : EditorWindow
{
    private GameObject handlerObject;
    private List<string> errors = new List<string>();
    private List<string> warnings = new List<string>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Validate Sprite Names")]
    public static void ShowWindow()
    {
        GetWindow<SpriteNameValidator>("Sprite Validator");
    }

    void OnGUI()
    {
        GUILayout.Label("Sprite Name Validation Tool", EditorStyles.boldLabel);
        
        handlerObject = (GameObject)EditorGUILayout.ObjectField(
            "Handler GameObject", 
            handlerObject, 
            typeof(GameObject), 
            true
        );

        if (GUILayout.Button("Run Validation"))
        {
            ValidateSprites();
        }

        if (errors.Count > 0 || warnings.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (errors.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"ERRORS ({errors.Count})", EditorStyles.boldLabel);
                foreach (string error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }

            if (warnings.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"WARNINGS ({warnings.Count})", EditorStyles.boldLabel);
                foreach (string warning in warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    void ValidateSprites()
    {
        errors.Clear();
        warnings.Clear();

        if (handlerObject == null)
        {
            errors.Add("Please assign a GameObject with DialogueScriptCommandHandler!");
            return;
        }

        // Find the component by name using reflection
        var handler = handlerObject.GetComponent("DialogueScriptCommandHandler");
        
        if (handler == null)
        {
            errors.Add("DialogueScriptCommandHandler component not found on this GameObject!");
            return;
        }

        // Get the allSprites field using reflection
        var handlerType = handler.GetType();
        var spritesField = handlerType.GetField("allSprites", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);

        if (spritesField == null)
        {
            errors.Add("Could not access allSprites field!");
            return;
        }

        List<Sprite> allSprites = spritesField.GetValue(handler) as List<Sprite>;

        if (allSprites == null || allSprites.Count == 0)
        {
            errors.Add("No sprites found in the handler!");
            return;
        }

        // Find enum types using multiple methods
        System.Type charactersEnum = FindEnumType("Characters");
        System.Type emotionsEnum = FindEnumType("EmotionType");

        if (charactersEnum == null)
        {
            errors.Add("Could not find 'Characters' enum! Make sure it's defined in your project.");
            Debug.LogError("Searched assemblies for Characters enum but couldn't find it.");
            return;
        }

        if (emotionsEnum == null)
        {
            errors.Add("Could not find 'EmotionType' enum! Make sure it's defined in your project.");
            Debug.LogError("Searched assemblies for EmotionType enum but couldn't find it.");
            return;
        }

        string[] validCharacters = System.Enum.GetNames(charactersEnum);
        string[] validEmotions = System.Enum.GetNames(emotionsEnum);

        Debug.Log($"Found {validCharacters.Length} characters: {string.Join(", ", validCharacters)}");
        Debug.Log($"Found {validEmotions.Length} emotions: {string.Join(", ", validEmotions)}");

        HashSet<string> characterSet = new HashSet<string>(validCharacters);
        HashSet<string> emotionSet = new HashSet<string>(validEmotions);

        // Track found sprites
        Dictionary<string, List<string>> characterEmotions = new Dictionary<string, List<string>>();
        List<string> backgroundSprites = new List<string>();
        List<string> unknownSprites = new List<string>();

        for (int i = 0; i < allSprites.Count; i++)
        {
            if (allSprites[i] == null)
            {
                warnings.Add($"Index {i}: Sprite slot is null/empty");
                continue;
            }

            string spriteName = allSprites[i].name;
            bool isCharacterSprite = false;

            // Check if it's a character sprite
            foreach (string character in validCharacters)
            {
                // Normalize both sprite name and character name for comparison
                string normalizedSpriteName = NormalizeName(spriteName);
                string normalizedCharacter = NormalizeName(character);
                
                if (normalizedSpriteName.StartsWith(normalizedCharacter))
                {
                    // Get the remaining part after character name
                    int charLength = character.Length;
                    string remainingName = spriteName.Substring(charLength);
                    
                    // Remove underscores from remaining name for emotion check
                    string normalizedRemaining = NormalizeName(remainingName);
                    
                    // Check if the remaining part matches any emotion (case-insensitive, no underscores)
                    bool foundEmotion = false;
                    string matchedEmotion = "";
                    
                    foreach (string emotion in validEmotions)
                    {
                        if (NormalizeName(emotion) == normalizedRemaining)
                        {
                            foundEmotion = true;
                            matchedEmotion = emotion;
                            break;
                        }
                    }
                    
                    if (foundEmotion)
                    {
                        isCharacterSprite = true;
                        
                        if (!characterEmotions.ContainsKey(character))
                        {
                            characterEmotions[character] = new List<string>();
                        }
                        characterEmotions[character].Add(matchedEmotion);
                        
                        Debug.Log($"✓ Valid character sprite: {spriteName} (Index: {i}) → Matched as {character}{matchedEmotion}");
                    }
                    else if (!string.IsNullOrEmpty(remainingName))
                    {
                        errors.Add($"Index {i}: '{spriteName}' - Invalid emotion '{remainingName}'. " +
                                 $"Valid emotions: {string.Join(", ", validEmotions)}");
                    }
                    break;
                }
            }

            if (!isCharacterSprite)
            {
                // Check if it's a background sprite
                if (spriteName.ToLower().Contains("bg") || 
                    spriteName.ToLower().Contains("background") ||
                    spriteName.ToLower().Contains("scene"))
                {
                    backgroundSprites.Add(spriteName);
                    Debug.Log($"✓ Background sprite: {spriteName} (Index: {i})");
                }
                else
                {
                    unknownSprites.Add($"{spriteName} (Index: {i})");
                }
            }
        }

        // Report missing emotions for each character
        foreach (string character in validCharacters)
        {
            if (!characterEmotions.ContainsKey(character))
            {
                warnings.Add($"Character '{character}' has no sprite expressions defined");
            }
            else
            {
                var missingEmotions = validEmotions.Except(characterEmotions[character]).ToArray();
                if (missingEmotions.Length > 0)
                {
                    warnings.Add($"Character '{character}' is missing emotions: {string.Join(", ", missingEmotions)}");
                }
            }
        }

        // Report unknown sprites
        if (unknownSprites.Count > 0)
        {
            warnings.Add($"Unknown sprite types found:\n{string.Join("\n", unknownSprites)}");
        }

        // Summary
        if (errors.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("✓ ALL SPRITES VALIDATED SUCCESSFULLY!");
        }
        else
        {
            Debug.Log($"Validation complete: {errors.Count} errors, {warnings.Count} warnings");
        }
    }

    // Helper method to find enum type across all assemblies
    private System.Type FindEnumType(string enumName)
    {
        // Try direct lookup first
        System.Type type = System.Type.GetType(enumName);
        if (type != null && type.IsEnum)
            return type;

        // Search all loaded assemblies
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in assembly.GetTypes())
            {
                if (t.IsEnum && t.Name == enumName)
                {
                    Debug.Log($"Found {enumName} in assembly: {assembly.GetName().Name}");
                    return t;
                }
            }
        }

        return null;
    }

    // Helper method to normalize names (remove underscores, spaces, lowercase)
    private string NormalizeName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "";
        
        return name.Replace("_", "").Replace(" ", "").ToLower();
    }
}
#endif