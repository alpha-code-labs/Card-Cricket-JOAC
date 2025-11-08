using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class DateShuffleEffectManager : MonoBehaviour
{
    TextMeshProUGUI dateText;
    // Start is called before the first frame update
    void Start()
    {
        dateText = GetComponentInChildren<TextMeshProUGUI>();
        dateText.text = "";
        if (NewDayManager.currentEventIndex == 0)
            SetFilmGrain(true);
        else
            SetFilmGrain(false);
    }
    public IEnumerator DisplayTextThenFade(string textToDisplay, float displayDuration = 1.5f, float fadeDuration = 1f)
    {
        dateText.text = textToDisplay;
        if (dateText.alpha != 1)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return dateText.DOFade(1f, fadeDuration).WaitForCompletion();
        }
        yield return new WaitForSeconds(displayDuration);
        yield return dateText.DOFade(0f, fadeDuration).WaitForCompletion();
    }

    public IEnumerator AnimateDateProgression(DateTime startDate, DateTime endDate)
    {
        // Calculate the total number of days between dates
        int totalDays = (endDate - startDate).Days;

        // Start with the previous date
        DateTime currentAnimatedDate = startDate;
        string currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");
        dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);

        // Make sure the text is visible
        if (dateText.alpha != 1f)
        {
            yield return dateText.DOFade(1f, 0.3f).WaitForCompletion();
        }

        // Hold at the starting date for 1 second
        yield return new WaitForSeconds(1f);

        // Start with 1 second delay and reduce by 20% each iteration
        float currentDelay = 1f;
        float currentTime = Time.time;
        // Animate through each day with progressively faster speed
        for (int i = 0; i < totalDays; i++)
        {
            const int SkipLength = 7;
            if (i % SkipLength == 0 && i >= SkipLength) // Skip weekends
            {
                i += SkipLength - 1;
                if (i >= totalDays)
                    i = totalDays - 1;
            }
            currentAnimatedDate = startDate.AddDays(i + 1);
            currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");

            // Use current delay, minimum 0.05 seconds
            float timeForThisDay = Mathf.Max(currentDelay, 0.05f);

            // Create a smooth transition effect
            float fadeTime = timeForThisDay * 0.3f;
            yield return dateText.DOFade(0.7f, fadeTime).WaitForCompletion();
            dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);
            AudioSFXManager.instance.PlayOneShotSFX(SFXType.ProjectorClick);
            yield return dateText.DOFade(1f, fadeTime).WaitForCompletion();

            // Wait for the remaining time for this day
            float remainingTime = timeForThisDay - (fadeTime * 2);
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }

            // Reduce delay by 20% for next iteration (multiply by 0.8)
            currentDelay *= 0.8f;
        }
        Debug.Log($"Animated from {startDate} to {endDate} took {Time.time - currentTime} seconds.");
        // Final format showing "from -> to" 
        yield return new WaitForSeconds(1f);
        yield return dateText.DOFade(0.5f, 1f).WaitForCompletion();
    }
    VideoPlayer videoPlayer;
    void SetFilmGrain(bool enable)
    {
        videoPlayer = Camera.main.GetComponent<VideoPlayer>();
        videoPlayer.enabled = enable;
    }
}
