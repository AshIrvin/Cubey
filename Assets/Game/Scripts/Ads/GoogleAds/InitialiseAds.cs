using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Placement;
using UnityEngine;

public class InitialiseAds : MonoBehaviour
{
    /// <summary>
    /// Half of this is Googles code, the other half is me trying to get it to work...
    /// </summary>
    
    public InterstitialAd interstitial;
    public static Action LoadLevel;
    public static Action LoadAd;

    private InterstitialAdGameObject fullscreenAd;
    public BannerAdGameObject bannerAd;

    private void Awake()
    {
        enabled = false;

        if (bannerAd == null)
        {
            bannerAd = transform.GetComponentInChildren<BannerAdGameObject>();
        }

        MobileAds.Initialize((initStatus) => {
            Debug.Log("Initialized MobileAds");
        });
    }

    private void OnEnable()
    {
        if (SaveLoadManager.GamePurchased)
        {
            // bannerAd.gameObject.SetActive(false);
            DestroyTopBannerAd();
            return;
        }
        
        MapManager.LoadAd += ShowAd;
        MapManager.MapOpened += GetAd;
        
        bannerAd.gameObject.SetActive(true);
        bannerAd.enabled = true;
        bannerAd?.LoadAd();
    }

    private void Start()
    {
        if (SaveLoadManager.GamePurchased)
        {
            return;
        }
        
        fullscreenAd = MobileAds.Instance.GetAd<InterstitialAdGameObject>("InterstitialAd");
        bannerAd = MobileAds.Instance.GetAd<BannerAdGameObject>("TopBannerAd");
        bannerAd.gameObject.SetActive(true);
        bannerAd?.LoadAd();
    }

    #region Manual Setup

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
    public void GetAd()
    {
        fullscreenAd?.LoadAd();
        
        StartCoroutine(WaitToGetAd());
    }

    private IEnumerator WaitToGetAd()
    {
        yield return new WaitUntil(() => fullscreenAd != null && 
                                         fullscreenAd.InterstitialAd != null &&
                                         fullscreenAd.InterstitialAd.IsLoaded()); // crashed here
        
        if (fullscreenAd != null && fullscreenAd.InterstitialAd.IsLoaded())
        {
            Debug.Log("ad is loaded");
        }
        else
        {
            Debug.LogError("Ad errored out");
            DestroyAd();
        }
    }
    
    public void ShowAd()
    {
        if (fullscreenAd != null && fullscreenAd.InterstitialAd != null && fullscreenAd.InterstitialAd.IsLoaded())
        {
            fullscreenAd.ShowIfLoaded(); // crashed here
            Debug.Log("Ad 2 - show if loaded: " + fullscreenAd.name);
        }
        
        StartCoroutine(DelayToCheckAd());
    }

    IEnumerator DelayToCheckAd()
    {
        yield return new WaitForSeconds(0.5f);
        if (fullscreenAd == null || fullscreenAd.InterstitialAd == null)
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

    public void DestroyTopBannerAd()
    {
        bannerAd.gameObject.SetActive(false);
        bannerAd?.DestroyAd();
        bannerAd.enabled = false;
    }

    public void LoadTopBannerAd()
    {
        bannerAd?.LoadAd();
    }
    
    private void OnDisable()
    {
        MapManager.LoadAd -= ShowAd;
        MapManager.MapOpened -= GetAd;
    }

    private void OnDestroy()
    {
        MapManager.LoadAd -= ShowAd;
        MapManager.MapOpened -= GetAd;
    }
}
