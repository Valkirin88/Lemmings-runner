using System;
using UnityEngine;

public class AdsUIMediator : IDisposable
{
    private readonly LemmingPlaceHandler _lemmingPlaceHandler;
    private readonly AdsHandler _adsHandler;
    private readonly UIHandler _uiHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly RandomSpawner _randomSpawner;
    private readonly GameStatesUIMediator _gameStatesUIMediator;
    private readonly ObstaclesSet _obstaclesSet;
    private readonly AudioClip _destroyAllObstaclesSound;

    private readonly int _continueLemmingsCount;
    private int _playAttempts;

    public AdsUIMediator(
        LemmingPlaceHandler lemmingPlaceHandler,
        AdsHandler adsHandler,
        UIHandler uiHandler,
        LemmingsEventsHandler lemmingsEventsHandler,
        RandomSpawner randomSpawner,
        GameStatesUIMediator gameStatesUIMediator,
        ObstaclesSet obstaclesSet,
        AudioClip destroyAllObstaclesSound,
        int continueLemmingsCount = 1)
    {
        _lemmingPlaceHandler = lemmingPlaceHandler;
        _adsHandler = adsHandler;
        _uiHandler = uiHandler;
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _randomSpawner = randomSpawner;
        _gameStatesUIMediator = gameStatesUIMediator;
        _obstaclesSet = obstaclesSet;
        _destroyAllObstaclesSound = destroyAllObstaclesSound;
        _continueLemmingsCount = Mathf.Max(1, continueLemmingsCount);

        _playAttempts = GameInfo.PlayAttempts;

        _uiHandler.OnResumedAfterGameOver += ShowAds;
        _uiHandler.OnContinueGameplay += ContinueGameplay;
        _adsHandler.OnAdsLoaded += SetAdsStatus;
        _adsHandler.OnAdsLoadFailed += ResetAdsStatus;
        _adsHandler.OnAdsRewarded += GrantReward;

        if (GameInfo.IsAdsOn)
            _uiHandler.IsAdsNeeded = true;
    }

    private void ShowAds()
    {
        _adsHandler.ShowRewardedAd();
    }

    private void SetAdsStatus()
    {
        _uiHandler.IsAdsAvailable = true;
    }

    private void ResetAdsStatus()
    {
        _uiHandler.IsAdsAvailable = false;
    }

    /// <summary>
    /// Награда получена: реклама больше не нужна, прячем картинку и тратим попытку.
    /// Кнопка «Продолжить» остаётся — следующее нажатие возобновит игру.
    /// </summary>
    private void GrantReward()
    {
        _uiHandler.IsAdsNeeded = false;
        _uiHandler.OnAdsRewardGranted();
    }

    /// <summary>
    /// Второе нажатие «Продолжить» после награды: оживляем леммингов и возвращаем игру в Game.
    /// </summary>
    private void ContinueGameplay()
    {
        ClearAllObstacles();
        ReviveLemmings(_continueLemmingsCount);
        _gameStatesUIMediator.ResumeAfterGameOver();
    }

    private void ClearAllObstacles()
    {
        if (_obstaclesSet == null)
            return;

        new DestroyAllObstacles(_obstaclesSet, _destroyAllObstaclesSound).Activate();
    }

    private void ReviveLemmings(int count)
    {
        if (count <= 0)
            return;

        var reviver = new IncreaseLemmingsNumber(_randomSpawner, _lemmingPlaceHandler, _lemmingsEventsHandler);
        for (int i = 0; i < count; i++)
            reviver.Activate();
    }

    public void Dispose()
    {
        _uiHandler.OnResumedAfterGameOver -= ShowAds;
        _uiHandler.OnContinueGameplay -= ContinueGameplay;
        _adsHandler.OnAdsLoaded -= SetAdsStatus;
        _adsHandler.OnAdsLoadFailed -= ResetAdsStatus;
        _adsHandler.OnAdsRewarded -= GrantReward;
    }
}
