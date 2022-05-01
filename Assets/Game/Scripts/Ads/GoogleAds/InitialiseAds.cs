using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Placement;
using UnityEngine;

public class InitialiseAds : MonoBehaviour
{
    private const string AndroidGoogleId = "ca-app-pub-7847363984248010~9115388794";
    
    private const string TestAndroidGoogleId = "ca-app-pub-3940256099942544/1033173712";
    private const string TestIosGoogleId = "ca-app-pub-3940256099942544/4411468910";

    public InterstitialAd interstitial;
    public static Action LoadLevel;
    public static Action LoadAd;
    
    private void Awake()
    {
        if (SaveLoadManager.GamePurchased)
            return;
        
        MapManager.LoadAd += ShowAd;
        MapManager.PrepareAd += GetAd;
    }

    private void Start()
    {
        if (SaveLoadManager.GamePurchased)
            return;
        
        fullscreenAd = MobileAds.Instance
            .GetAd<InterstitialAdGameObject>("InterstitialAd");

        MobileAds.Initialize((initStatus) => {
            Debug.Log("Initialized MobileAds");
        });
    }

    #region Manual Setup

    /*public void RequestInterstitial()
    {
        Debug.Log("Preparing Google Ad");
#if UNITY_ANDROID
        string adUnitId = TestAndroidGoogleId;
#elif UNITY_IPHONE
        string adUnitId = TestIosGoogleId;
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;
        
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public void DisplayFullScreenAd()
    {
        if (this.interstitial.IsLoaded()) 
        {
            Debug.Log("Ad loaded - showing");
            this.interstitial.Show();
        }
        else
        {
            Debug.Log("Ad hasn't loaded!");
        }
    }

    public void DestroyAd()
    {
        interstitial.Destroy();
    }*/

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
        LoadLevel?.Invoke();
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        fullscreenAd?.DestroyAd();
        LoadLevel?.Invoke();
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
        ContinueToLevel();
    }

    #endregion

    // ******** Placement ads ********** //

    InterstitialAdGameObject fullscreenAd;
    
    public void GetAd()
    {
        fullscreenAd?.LoadAd();
        if (fullscreenAd != null && fullscreenAd.InterstitialAd.IsLoaded())
        {
            Debug.Log("ad is loaded");
        }
        else
        {
            Debug.LogError("Ad errored out");
            // AdFailed();
            DestroyAd();
        }
    }
    
    public void ShowAd()
    {
        fullscreenAd?.ShowIfLoaded();
        Debug.Log("Ad 2 - show if loaded");
        if (fullscreenAd == null)
        {
            Debug.Log("Ad 3 - failed! Continuing to level");
            AdFailed();
        }
    }

    public void AdFailed()
    {
        Debug.LogError("Ad 4 - has failed to load");
        ContinueToLevel();
    }

    private void DestroyAd()
    {
        fullscreenAd?.DestroyAd();
        Debug.Log("Destroying ad. fullscreenAd null? " + fullscreenAd);
    }
    
    public void ContinueToLevel()
    {
        DestroyAd();
        
        LoadLevel?.Invoke();
    }

    private void OnDisable()
    {
        // MapManager.LoadAd -= ShowAd;
        // MapManager.PrepareAd -= GetAd;
    }

    private void OnDestroy()
    {
        MapManager.LoadAd -= ShowAd;
        MapManager.PrepareAd -= GetAd;
    }
}
