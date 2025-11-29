using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonGameplayInitializer : MonoBehaviour
{
    public static ButtonGameplayInitializer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}