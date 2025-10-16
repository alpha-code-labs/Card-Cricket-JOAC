using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System;

public class DialogueScriptCommandHandler : MonoBehaviour
{
    public static DialogueScriptCommandHandler Instance;

    [Header("All Sprites - Characters and Backgrounds")]
    List<Sprite> allSprites;

    [Header("Character Display Images")]
    [SerializeField] Image centerCharacterImage;

    [Header("UI Components")]
    [SerializeField] Image currentBGSprite;

    [Header("Dialogue Settings")]
    public static string currentNode;

    [Header("Audio Settings")]
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] List<AudioClip> backgroundMusicClips;
    [SerializeField] float musicFadeDuration = 1f;

    // Dictionary for sprite name to index mapping
    private Dictionary<string, int> spriteNameToIndex;

    private Dictionary<String, AudioClip> musicDictionary;
    private Characters currentActiveCharacter = Characters.Ramu; // Track active character
    void Awake()
    {
        Instance = this;
        InitializeMusicDictionary();
        InitializeSpriteMapping();
    }

    void Start()
    {
        if (currentNode == null)//This should only happen if we directly load the scene for testing
        {
            currentNode = "scene_1";//Default Starting Node
            Debug.LogWarning("Testing?: currentNode was null, defaulting to 'scene_1'");
        }

        Debug.Log($"Starting Dialogue at node: {currentNode}");
        HideAllCharacters();
        YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(currentNode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (YarnDialogSystemSingleTonMaker.instance.dialogueRunner != null)
            {
                YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(currentNode);
            }
            else
            {
                Debug.LogError("DialogueRunner not assigned!");
            }
        }
    }

    private void InitializeSpriteMapping()
    {
        // Initialize sprite name to index mapping
        spriteNameToIndex = new Dictionary<string, int>();

        // Load Characters and Locations separately for clarity and predictable organization
        List<Sprite> characterSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Textures/Characters"));
        List<Sprite> locationSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Textures/Locations"));

        allSprites = new List<Sprite>(characterSprites.Count + locationSprites.Count);
        allSprites.AddRange(locationSprites);
        allSprites.AddRange(characterSprites);
        Debug.Log($"Sprite load: Characters={characterSprites.Count}, Locations={locationSprites.Count}, Total={allSprites.Count}");
        // Auto-populate based on sprite names in the list
        for (int i = 0; i < allSprites.Count; i++)
        {
            if (allSprites[i] != null)
            {
                string actualSpriteName = allSprites[i].name;
                string normalizedKey = NormalizeName(actualSpriteName);
                if (!spriteNameToIndex.ContainsKey(normalizedKey))
                {
                    spriteNameToIndex[normalizedKey] = i;
                }
                else
                {
                    Debug.LogWarning($"Duplicate sprite name detected: '{actualSpriteName}'. Keeping the first occurrence and ignoring later duplicates.");
                }

                // Debug to see the mapping
                //Debug.Log($"Mapped '{actualSpriteName}' to normalized key '{normalizedKey}' at index {i}");
            }
        }
    }

    private void InitializeMusicDictionary()
    {
        musicDictionary = new Dictionary<string, AudioClip>();

        // Map each AudioClip by its actual name, not by index or enum value
        for (int i = 0; i < backgroundMusicClips.Count; i++)
        {
            if (backgroundMusicClips[i] != null)
            {
                AudioClip clip = backgroundMusicClips[i];
                string clipName = clip.name; // Get the actual name of the AudioClip

                // Add the clip with its exact name
                musicDictionary[clipName] = clip;

                // Also add normalized version for flexibility
                string normalizedName = NormalizeName(clipName);
                if (normalizedName != clipName)
                {
                    musicDictionary[normalizedName] = clip;
                }

                // Debug to verify mapping
                Debug.Log($"Mapped AudioClip '{clipName}' (at index {i}) to dictionary");
            }
        }

        Debug.Log($"Initialized music dictionary with {musicDictionary.Count} entries");
    }

    // Helper method to normalize sprite names for lookup
    private static string NormalizeName(string name)
    {
        // Remove spaces, underscores, and convert to lowercase
        return name.Replace(" ", "").Replace("_", "").ToLower();
    }

    // MASTER CHARACTER EXPRESSION METHOD - Now callable from Yarn
    [YarnCommand("SetCharacterExpression")]
    public static void SetCharacterExpression(string characterName, string emotion)
    {
        if (!System.Enum.TryParse<Characters>(characterName, true, out Characters character))
        {
            Debug.LogError($"Unknown character name: {characterName}");
            return;
        }

        SetCharacterExpressionInternal(character, emotion);
    }

    // Internal method that does the actual work
    private static void SetCharacterExpressionInternal(Characters character, string emotion)
    {
        // Create sprite name: CharacterEmotion (e.g., "RamuExcited", "RajuSerious")
        string spriteName = character.ToString() + emotion;
        string normalizedKey = NormalizeName(spriteName);

        if (!Instance.spriteNameToIndex.ContainsKey(normalizedKey))
        {
            Debug.LogError($"Sprite '{spriteName}' (normalized: '{normalizedKey}') not found in sprite list!");
            return;
        }

        int spriteIndex = Instance.spriteNameToIndex[normalizedKey];
        if (spriteIndex >= 0 && spriteIndex < Instance.allSprites.Count)
        {
            Sprite targetSprite = Instance.allSprites[spriteIndex];
            if (targetSprite != null)
            {
                Instance.centerCharacterImage.gameObject.SetActive(false);

                // If different character, switch sides
                if (Instance.currentActiveCharacter != character)
                {
                    Instance.currentActiveCharacter = character;
                }
                Instance.centerCharacterImage.sprite = targetSprite;
                Instance.centerCharacterImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Sprite at index {spriteIndex} is null!");
            }
        }
        else
        {
            Debug.LogError($"Sprite index {spriteIndex} out of range!");
        }
    }

    // BACKGROUND COMMANDS
    [YarnCommand("SetBGSprite")]
    public static void SetBGSprite(string backgroundName)
    {
        string normalizedKey = NormalizeName(backgroundName);

        if (!Instance.spriteNameToIndex.ContainsKey(normalizedKey))
        {
            Debug.LogError($"Background sprite '{backgroundName}' (normalized: '{normalizedKey}') not found in sprite list!");
            return;
        }

        int spriteIndex = Instance.spriteNameToIndex[normalizedKey];
        if (spriteIndex >= 0 && spriteIndex < Instance.allSprites.Count)
        {
            Instance.currentBGSprite.sprite = Instance.allSprites[spriteIndex];
        }
        else
        {
            Debug.LogError($"Background sprite index {spriteIndex} out of range!");
        }
    }

    // UTILITY COMMANDS
    [YarnCommand("HideAllCharacters")]
    public static void HideAllCharacters()
    {
        Instance.centerCharacterImage.gameObject.SetActive(false);
    }

    // AUDIO COMMANDS
    [YarnCommand("PlayBackgroundMusic")]
    public static void PlayBackgroundMusic(string musicName)
    {
        // Try to find music in dictionary
        if (Instance.musicDictionary.ContainsKey(musicName))
        {
            AudioClip musicClip = Instance.musicDictionary[musicName];
            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(musicClip));
        }
        // Try normalized name
        else if (Instance.musicDictionary.ContainsKey(NormalizeName(musicName)))
        {
            AudioClip musicClip = Instance.musicDictionary[NormalizeName(musicName)];
            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(musicClip));
        }
        else
        {
            Debug.LogError($"Music '{musicName}' not found in dictionary!");
        }
    }
    [YarnCommand("StopBackgroundMusic")]
    public static void StopBackgroundMusic()
    {
        Instance.StartCoroutine(Instance.StopBackgroundMusicCoroutine());
    }

    // AUDIO COROUTINES
    private IEnumerator PlayBackgroundMusicCoroutine(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            // If same music is already playing, exit immediately
            if (musicAudioSource.clip == musicClip && musicAudioSource.isPlaying)
            {
                yield break; // Exit immediately
            }

            // Handle music switching
            if (musicAudioSource.isPlaying && musicAudioSource.clip != musicClip)
            {
                // Don't yield - start fade-out independently
                StartCoroutine(FadeOutMusic());
            }

            musicAudioSource.clip = musicClip;
            musicAudioSource.loop = true;
            musicAudioSource.volume = 0f;
            musicAudioSource.Play();

            // Don't yield - start fade-in independently
            StartCoroutine(FadeInMusic());
        }

        // Coroutine ends immediately
        yield break;
    }

    private IEnumerator StopBackgroundMusicCoroutine()
    {
        if (musicAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
            musicAudioSource.Stop();
        }
    }

    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;
        while (elapsed < musicFadeDuration)
        {
            musicAudioSource.volume = elapsed / musicFadeDuration;
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicAudioSource.volume = 1f;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            musicAudioSource.volume = startVolume * (1f - (elapsed / musicFadeDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicAudioSource.volume = 0f;
    }
}

public enum EmotionType
{
    Angry,
    Anxious,
    Astonished,
    BattingPose,
    Bored,
    Confident,
    Confused,
    Crying,
    Curious,
    Determined,
    Disappointed,
    Disapproving,
    Disgusted,
    Embarrassed,
    Excited,
    ExcitedWithBat,
    Furious,
    Irritated,
    Loving,
    Mischievous,
    Neutral,
    Overjoyed,
    Pained,
    Proud,
    Quizzical,
    Relaxed,
    Relieved,
    Sad,
    Serious,
    SeriousWithBat,
    Shocked,
    Skeptical,
    Smiling,
    Terrified,
    Tired,
    Worried,
}

public enum MusicType
{
    Heartbeat = 0,
    Exciting_1 = 1,
    Disappointing_1 = 2,
    Light_1 = 3,
    Emotional_1 = 4,
    Angry_1 = 5,

    Spiritual_1 = 6,

    Light_3 = 7,

    Emotional_2 = 8,


}


