using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class AdsHandler : MonoBehaviour
{
    private string _adUnitId = "demo-rewarded-yandex";
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
        
        try
        {
            rewardedAd = await rewardedAdLoader.LoadAd(new AdRequest(_adUnitId));

            // Add events handlers for ad actions
            rewardedAd.OnAdClicked += HandleAdClicked;
            rewardedAd.OnAdShown += HandleAdShown;
            rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
            rewardedAd.OnAdImpression += HandleImpression;
            rewardedAd.OnAdDismissed += HandleAdDismissed;
            rewardedAd.OnRewarded += HandleRewarded;
        }
        catch (AdLoadingException e)
        {
            // Ad failed to load with {e.Message}
            // Attempting to load a new ad from catch block is strongly discouraged.
        }
    }

    private void ShowRewardedAd()
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
        // Called when the user can be rewarded with {args.type} and {args.amount}.
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