using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Placement;
using UnityEngine;
using System.Threading.Tasks;

public class InitialiseAds : MonoBehaviour
{
    [SerializeField] private InterstitialAd interstitial;

    internal static Action LoadLevel;
    internal static Action LoadAd;

    private InterstitialAdGameObject fullscreenAd;
    private BannerAdGameObject bannerAd;

    private void Awake()
    {
        enabled = false;

        if (bannerAd == null)
        {
            bannerAd = transform.GetComponentInChildren<BannerAdGameObject>();
        }
    }

    private void Start()
    {
        if (ShopManager.GamePurchased)
        {
            Logger.Instance.ShowDebugLog("MobileAds. Game purchased. No ads required.");
            return;
        }

        LoadAd += ShowAd;
        LoadLevel += LevelManager.Instance.PrepareToLoadLevelFromAd;

        MobileAds.Initialize((initStatus) => {
            Logger.Instance.ShowDebugLog("Initialized MobileAds");
        });

        fullscreenAd = MobileAds.Instance.GetAd<InterstitialAdGameObject>("InterstitialAd");
        bannerAd = MobileAds.Instance.GetAd<BannerAdGameObject>("TopBannerAd");
        bannerAd.gameObject.SetActive(true);
        bannerAd.LoadAd();

        print("Initialise Ads Start");
    }

    private void OnEnable()
    {
        if (ShopManager.GamePurchased)
        {
            DestroyTopBannerAd();
            Destroy(this);
            return;
        }
        
        //AdSettings.Instance.LoadAd += ShowAd;

        MapManager.OnMapLoad += GetAd;
        LoadAd += ShowAd;

        bannerAd.gameObject.SetActive(true);
        bannerAd.enabled = true;
        bannerAd.LoadAd();

        print("Initialise Ads Enable");
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
        ContinueToLevel();
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        fullscreenAd.DestroyAd();
        ContinueToLevel();
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
        ContinueToLevel();
    }

    #endregion

    // ******** Placement ads ********** //
    internal void GetAd()
    {
        if (ShopManager.GamePurchased) return;

        fullscreenAd.LoadAd();
        
        StartCoroutine(WaitToGetAd());
    }

    private IEnumerator WaitToGetAd()
    {
        yield return new WaitUntil(() => fullscreenAd != null && 
                                         fullscreenAd.InterstitialAd != null &&
                                         fullscreenAd.InterstitialAd.IsLoaded()); // crashed here
        
        if (fullscreenAd != null && fullscreenAd.InterstitialAd.IsLoaded())
        {
            Logger.Instance.ShowDebugLog("ad is loaded");
        }
        else
        {
            Logger.Instance.ShowDebugError("Ad errored out");
            DestroyAd();
        }

        LoadTopBannerAd();
    }
    
    public void ShowAd()
    {
        print("Show ad if not null");
        if (fullscreenAd != null && fullscreenAd.InterstitialAd != null && fullscreenAd.InterstitialAd.IsLoaded())
        {
            fullscreenAd.ShowIfLoaded(); // TODO - fix: crashed here
            Logger.Instance.ShowDebugLog("Ad 2 - show if loaded: " + fullscreenAd.name);
        }
        
        DelayToCheckAd();
    }

    private async void DelayToCheckAd()
    {
        await Task.Delay(500);

        if (fullscreenAd == null || fullscreenAd.InterstitialAd == null)
        {
            Logger.Instance.ShowDebugLog("Ad 3 - failed! Continuing to level");
            AdFailed();
        }
    }

    private void AdFailed()
    {
        Logger.Instance.ShowDebugError("Ad 4 - has failed to load");
        ContinueToLevel();
    }

    private void DestroyAd()
    {
        fullscreenAd.DestroyAd();
        
        Logger.Instance.ShowDebugLog("Destroying ad. fullscreenAd null? " + fullscreenAd);
    }
    
    private void ContinueToLevel()
    {
        DestroyAd();
        
        LoadLevel?.Invoke();
    }

    internal void DestroyTopBannerAd()
    {
        if (bannerAd == null)
            Logger.Instance.ShowDebugLog("Can't find bannerAd");

        bannerAd.gameObject.SetActive(false);
        bannerAd.DestroyAd();
        bannerAd.enabled = false;
    }

    private void LoadTopBannerAd()
    {
        bannerAd.LoadAd();
    }
    
    private void OnDisable()
    {
        //LoadAd -= ShowAd;
        MapManager.OnMapLoad -= GetAd;
    }

    private void OnDestroy()
    {
        //LoadAd -= ShowAd;
        MapManager.OnMapLoad -= GetAd;
    }
}
