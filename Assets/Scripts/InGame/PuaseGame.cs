using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    public Button resumeButton;
    public Button pauseButton;
    public TextMeshProUGUI pauseText;
    public GameObject handCards;
    public GameObject ballerCard;
    public GameObject redrawButton;
    public GameObject blackOverlayPanel;
    private bool isPaused = false;
    

    void Start()
    {
        // Attach Resume() function to the pause button
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
            resumeButton.gameObject.SetActive(false); // Hide pause button initially
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(Pause);
        }

        if (blackOverlayPanel != null)
            blackOverlayPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(true);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
        if (pauseText != null)
        {
           pauseText.gameObject.SetActive(false);
        }

        //hide handCards, baller card and redraw button
        if (handCards != null)
            handCards.SetActive(false);
        if (ballerCard != null)
            ballerCard.SetActive(false);
        if (redrawButton != null)
            redrawButton.SetActive(false);

        if (blackOverlayPanel != null)
            blackOverlayPanel.SetActive(true);
        
        Time.timeScale = 0f;  // Freeze time
        isPaused = true;
        AudioListener.pause = true; 
    }

    public void Resume()
    {
        if (resumeButton != null)
            resumeButton.gameObject.SetActive(false);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);
        if (pauseText != null)
        {
            pauseText.gameObject.SetActive(true);
        }
        

        //Unhide handCards, ballerCard and redraw button
        if (handCards != null)
            handCards.SetActive(true);
        if (ballerCard != null)
            ballerCard.SetActive(true);
        if (redrawButton != null)
            redrawButton.SetActive(true);

        if (blackOverlayPanel != null)
            blackOverlayPanel.SetActive(false);

        Time.timeScale = 1f;  // Resume time
        isPaused = false;
        AudioListener.pause = false; 
    }
}
