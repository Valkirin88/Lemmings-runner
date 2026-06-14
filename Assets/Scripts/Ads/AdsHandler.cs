using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class AdsHandler : MonoBehaviour
{
    public static AdsHandler Instance { get; private set; }

    public event Action OnAdsLoadFailed;
    public event Action OnAdsLoaded;
    public event Action OnAdsUnloaded;
    public event Action OnAdsRewarded;

    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;
    private bool _isLoading;
    private Coroutine _retryCoroutine;
    private const float RetryDelaySeconds = 5f;

    public bool IsAdReady => rewardedAd != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            YandexAds.SetUserConsent(true);
        }
        catch (Exception)
        {
        }

        SetupLoader();
        RequestRewardedAd();
    }

    private void OnDestroy()
    {
        if (_retryCoroutine != null)
        {
            StopCoroutine(_retryCoroutine);
            _retryCoroutine = null;
        }

        if (Instance == this)
            Instance = null;
    }

    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
    }

    private void RequestRewardedAd()
    {
        if (_isLoading)
            return;

        _isLoading = true;
        AdRequest adRequest = new AdRequest(GameInfo.AdsHandlerName);
        rewardedAdLoader.LoadAd(
            adRequest: adRequest,
            onLoaded: HandleAdLoaded,
            onFailed: HandleAdFailedToLoad);
    }

    private void ForceReloadRewardedAd()
    {
        _isLoading = false;
        rewardedAdLoader?.CancelLoading();
        SetupLoader();
        RequestRewardedAd();
    }

    public void HandleAdLoaded(RewardedAd loadedAd)
    {
        _isLoading = false;
        if (_retryCoroutine != null)
        {
            StopCoroutine(_retryCoroutine);
            _retryCoroutine = null;
        }

        rewardedAd = loadedAd;

        rewardedAd.OnAdClicked += HandleAdClicked;
        rewardedAd.OnAdShown += HandleAdShown;
        rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
        rewardedAd.OnAdImpression += HandleImpression;
        rewardedAd.OnAdDismissed += HandleAdDismissed;
        rewardedAd.OnRewarded += HandleRewarded;

        OnAdsLoaded?.Invoke();
    }

    public void HandleAdFailedToLoad(AdFailedToLoadEventArgs args)
    {
        _isLoading = false;
        OnAdsLoadFailed?.Invoke();
        ScheduleRetry();
    }

    private void ScheduleRetry()
    {
        if (_retryCoroutine != null)
            return;

        _retryCoroutine = StartCoroutine(RetryLoadAfterDelay());
    }

    private IEnumerator RetryLoadAfterDelay()
    {
        yield return new WaitForSecondsRealtime(RetryDelaySeconds);
        _retryCoroutine = null;
        ForceReloadRewardedAd();
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show();
            return;
        }

        ForceReloadRewardedAd();
    }

    public void HandleAdDismissed(object sender, EventArgs args)
    {
        DestroyRewardedAd();
        RequestRewardedAd();
    }

    public void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        DestroyRewardedAd();
        RequestRewardedAd();
    }

    public void HandleAdClicked(object sender, EventArgs args)
    {
    }

    public void HandleAdShown(object sender, EventArgs args)
    {
    }

    public void HandleImpression(object sender, ImpressionData impressionData)
    {
    }

    public void HandleRewarded(object sender, Reward args)
    {
        OnAdsRewarded?.Invoke();
        DestroyRewardedAd();
        RequestRewardedAd();
    }

    public void DestroyRewardedAd()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.Destroy();
        rewardedAd = null;
        OnAdsUnloaded?.Invoke();
    }
}
