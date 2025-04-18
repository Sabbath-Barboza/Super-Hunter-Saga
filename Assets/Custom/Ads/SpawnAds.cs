using System.Collections;
using TMPro;
using UnityEngine;

public class SpawnAds : MonoBehaviour
{
    public GameObject adTimerPanel; // Reference to the Ad Timer Panel
    public TMP_Text countdownText; // Reference to the Countdown Text
    public float panelDisplayDelay = 30f; // Time before the panel appears
    public float countdownDuration = 10f; // Countdown duration before the ad play

    private void Awake() => AdsManager.Adinstance.LoadBannerAD();
    
    private void Start()
    {
        // Initialize timers and hide the panel
        adTimerPanel.SetActive(false);
        StartCoroutine(ManageAdFlow());
    }

    IEnumerator ManageAdFlow()
    {
        AdsManager.Adinstance.LoadIntertitialAD();
        while (true) // Infinite loop to repeat ads
        {
            // Wait for the panel display delay
            yield return new WaitForSeconds(panelDisplayDelay);

            // Show the panel and start the countdown
            adTimerPanel.SetActive(true);
            yield return StartCoroutine(StartCountdown());

            // Hide the panel and play the ad
            adTimerPanel.SetActive(false);

            // Check if the interstitial ad is ready before showing
            if (AdsManager.Adinstance.IsIntertitialADReady())
            {
                AdsManager.Adinstance.ShowIntertitialAD();
            }
            else
            {
                Debug.Log("Ad not ready, reloading...");
                AdsManager.Adinstance.LoadIntertitialAD();
            }
            // Optionally, repeat this process
        }
    }

    IEnumerator StartCountdown()
    {
        float timer = countdownDuration;

        while (timer > 0)
        {
            countdownText.text = "Ad in: " + Mathf.CeilToInt(timer) + " Sec";
            timer -= Time.deltaTime;
            yield return null;
        }
        AdsManager.Adinstance.ShowIntertitialAD();
    }
}



