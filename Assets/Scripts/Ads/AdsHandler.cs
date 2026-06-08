using System;
using UnityEngine;
using UnityEngine.UI;
using YandexMobileAds;
using YandexMobileAds.Base;

public class AdsHandler : MonoBehaviour
{
    public event Action OnAdsLoadFailed;
    public event Action OnAdsLoaded;
    public event Action OnAdsRewarded;
    
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;


    private void Awake()
    {
        SetupLoader();
        RequestRewardedAd();
        DontDestroyOnLoad(gameObject);
    }

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
    }

    private async void RequestRewardedAd()
    {
        string adUnitId = "demo-rewarded-yandex"; // замените на "R-M-XXXXXX-Y"
        AdRequest adRequest = new AdRequest(adUnitId);
        rewardedAdLoader.LoadAd(
            adRequest: adRequest,
            onLoaded: HandleAdLoaded,
            onFailed: HandleAdFailedToLoad);
    }
    
    public void HandleAdLoaded(RewardedAd rewardedAd)
    {
        OnAdsLoaded?.Invoke();
        // The ad was loaded successfully. Now you can handle it.
        this.rewardedAd = rewardedAd;

        // Add events handlers for ad actions
        this.rewardedAd.OnAdClicked += HandleAdClicked;
        this.rewardedAd.OnAdShown += HandleAdShown;
        this.rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
        this.rewardedAd.OnAdImpression += HandleImpression;
        this.rewardedAd.OnAdDismissed += HandleAdDismissed;
        this.rewardedAd.OnRewarded += HandleRewarded;
    }
    
    public void HandleAdFailedToLoad(AdFailedToLoadEventArgs args)
    {
        OnAdsLoadFailed?.Invoke();
    }


    public void ShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show();
        }
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        // Called when an ad is dismissed.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        // Called when rewarded ad failed to show.

        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
        // Called when a click is recorded for an ad.
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
        // Called when an ad is shown.
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
        // Called when an impression is recorded for an ad.
    }

    public void HandleRewarded(object sender, Reward args)
    {
        OnAdsRewarded?.Invoke();
        // Clear resources after an ad dismissed.
        DestroyRewardedAd();

        // Now you can preload the next rewarded ad.
        RequestRewardedAd();
    }

    public void DestroyRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }
}