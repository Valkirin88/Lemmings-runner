using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;
using YandexMobileAds.Common;

public class YandexMobileAdsRewardedAdDemoScript : MonoBehaviour
{
    private string _adUnitId = "YandexMobileAds";
    
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
        // ...
    }
    
    private async void RequestRewardedAd()
    {
        try
        {
            rewardedAd = await rewardedAdLoader.LoadAd(new AdRequest(_adUnitId));
        }
        catch (AdLoadingException e)
        {
            // Ad failed to load with {e.Message}
            // Attempting to load a new ad from catch block is strongly discouraged.
        }
    }
    
}
