using GoogleMobileAds.Api;
using System;
using UnityEngine;


public class AdsManager : MonoBehaviour
{
    [SerializeField] private Ads_ID AdsID;
    public static AdsManager Adinstance;

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedInterstitialAd rewardedInterstitialAd;


    private void Awake()
    {
        if(Adinstance != null)
        {
            Destroy(this);
            return;
        }
        Adinstance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        MobileAds.Initialize(InitializationStatus => { });
        Debug.Log("The Ads are Initialize");

        LoadBannerAD();
        LoadIntertitialAD();
        LoadRewardedIntertitialAD();
    }

    #region Banner AD    
    private void CreateBannerAD()   // Created Banner AD Before it displays Banner Ads
    {
        Debug.Log("Creating Banner AD");   

        if(bannerView != null)  DestroyAd(); 

        bannerView = new(AdsID.BannerID, AdSize.IABBanner, AdPosition.Bottom); // Adding the Banner ID, Banner Size and Banner Position
    }

    public void LoadBannerAD()   // Display The Ads When it is Ready
    {
        if(bannerView == null) CreateBannerAD();

        var adRequest = new AdRequest();  

        Debug.Log("Loading Banner Ad");
        bannerView.LoadAd(adRequest);  // Requesting to load ad
    }

    private void DestroyAd()  // Destroy Banner Ads if it is Already displaying the banner ad
    {
        if (bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            bannerView.Destroy();
            bannerView = null;
        }
    }
    #endregion

    #region Interstitial AD
    private void LoadIntertitialAD()
    {
        Debug.Log("Loading the Interstitial Ad");

        // Create an AdRequest
        var adRequest = new AdRequest();

        // Load the interstitial ad
        InterstitialAd.Load(AdsID.IntertitialID, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load. Error: " + error.GetMessage());
                return;
            }

            if (ad == null)
            {
                Debug.LogError("Interstitial ad failed to load. Ad is null.");
                return;
            }

            Debug.Log("Interstitial ad loaded successfully with response: " + ad.GetResponseInfo());
            interstitialAd = ad;

            // Optionally, attach an event to handle when the ad is closed
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad closed.");
                interstitialAd = null; // Cleanup after the ad is closed
            };
        });
    }

    public void ShowIntertitialAD()
    {
        if(interstitialAd != null && interstitialAd.CanShowAd())
        {
            Debug.Log("Showing Intertitial Ad");
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Failed To Show The Intertitial Ad");
        }
        LoadIntertitialAD();
    }
    #endregion

    #region Rewarded Intertitial AD
    private void LoadRewardedIntertitialAD()
    {
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("Get Coins");

        RewardedInterstitialAd.Load(AdsID.RewardedIntertitialID, adRequest, (RewardedInterstitialAd ad, LoadAdError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load. Error: " + error.GetMessage());
                return;
            }

            if (ad == null)
            {
                Debug.LogError("Interstitial ad failed to load. Ad is null.");
                return;
            }

            Debug.Log("Interstitial ad loaded successfully with response: " + ad.GetResponseInfo());
            rewardedInterstitialAd = ad;

            // Optionally, attach an event to handle when the ad is closed
            rewardedInterstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad closed.");
                rewardedInterstitialAd = null; // Cleanup after the ad is closed
            };
        });
    }

    public void ShowRewardedIntertitialAd()
    {
        const string RewardMsg = "Player got the Reward";
        if(rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
              //CurrencySave.Instance.Deposit(50);
              //ScalingLabelBehavior.instance.SetAmount(50);
              Debug.Log("Rewarded the player with ");
              Debug.Log(String.Format(RewardMsg, reward.Type, reward.Amount));
            });
        }
    }
    #endregion
}
