using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    public Button pauseButton;
    public GameObject handCards;
    public GameObject ballerCard;
    public GameObject redrawButton;
    public TMP_Text puaseInstructionText;
    public GameObject blackOverlayPanel;
    private bool isPaused = false;
    

    void Start()
    {
        // Attach Resume() function to the pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(Resume);
            pauseButton.gameObject.SetActive(false); // Hide pause button initially
        }

        if (puaseInstructionText != null)
            puaseInstructionText.text = "Press 'Esc' to Pause";

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

    void Pause()
    {
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        //hide handCards, baller card and redraw button
        if (handCards != null)
            handCards.SetActive(false);
        if (ballerCard != null)
            ballerCard.SetActive(false);
        if (redrawButton != null)
            redrawButton.SetActive(false);

        if (blackOverlayPanel != null)
            blackOverlayPanel.SetActive(true);
        
        if (puaseInstructionText != null)
            puaseInstructionText.text = "Press 'Esc' to Resume";

        Time.timeScale = 0f;  // Freeze time
        isPaused = true;
        AudioListener.pause = true; 
    }

    void Resume()
    {
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        //Unhide handCards, ballerCard and redraw button
        if (handCards != null)
            handCards.SetActive(true);
        if (ballerCard != null)
            ballerCard.SetActive(true);
        if (redrawButton != null)
            redrawButton.SetActive(true);

        if (blackOverlayPanel != null)
            blackOverlayPanel.SetActive(false);
        
        if (puaseInstructionText != null)
            puaseInstructionText.text = "Press 'Esc' to Pause";

        Time.timeScale = 1f;  // Resume time
        isPaused = false;
        AudioListener.pause = false; 
    }
}
