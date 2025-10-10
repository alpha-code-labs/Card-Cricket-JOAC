using UnityEngine;


class TutorialArrowManager : MonoBehaviour
{
    public static TutorialArrowManager Instance;


    public GameObject arrow_score;
    public GameObject arrow_wickets;
    public GameObject arrow_remainingBalls;
    public GameObject arrow_redrawButton;
    public GameObject arrow_timing;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void HideAllArrows()
    {
        arrow_score.SetActive(false);
        arrow_wickets.SetActive(false);
        arrow_remainingBalls.SetActive(false);
        arrow_redrawButton.SetActive(false);
        arrow_timing.SetActive(false);
    }

    public void ShowArrow(string arrowName)
    {
        HideAllArrows();
        switch (arrowName)
        {
            case "score":
                arrow_score.SetActive(true);
                break;
            case "wickets":
                arrow_wickets.SetActive(true);
                break;
            case "remainingBalls":
                arrow_remainingBalls.SetActive(true);
                break;
            case "redrawButton":
                arrow_redrawButton.SetActive(true);
                break;
            case "timing":
                arrow_timing.SetActive(true);
                break;
            default:
                Debug.LogWarning($"No arrow found with name: {arrowName}");
                break;
        }
    }
    
    public void ShowAllArrows()
    {
        HideAllArrows();
        arrow_score.SetActive(true);
        arrow_wickets.SetActive(true);
        arrow_remainingBalls.SetActive(true);
        arrow_redrawButton.SetActive(true);
        arrow_timing.SetActive(true);
    }

}