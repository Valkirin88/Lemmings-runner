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
    private readonly LemmingPlaceView _lemmingPlaceView;

    private readonly int _continueLemmingsCount;
    private int _playAttempts;

    // Стартовые позиции, зафиксированные в начале игры — чтобы вернуть отряд на безопасное место после рекламы
    private readonly Vector3 _leaderStartPosition;
    private readonly bool _hasLeaderStart;
    private readonly Vector3 _placeViewStartPosition;

    public AdsUIMediator(
        LemmingPlaceHandler lemmingPlaceHandler,
        AdsHandler adsHandler,
        UIHandler uiHandler,
        LemmingsEventsHandler lemmingsEventsHandler,
        RandomSpawner randomSpawner,
        GameStatesUIMediator gameStatesUIMediator,
        ObstaclesSet obstaclesSet,
        AudioClip destroyAllObstaclesSound,
        LemmingView leaderLemmingView,
        LemmingPlaceView lemmingPlaceView,
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
        _lemmingPlaceView = lemmingPlaceView;
        _continueLemmingsCount = Mathf.Max(1, continueLemmingsCount);

        // Запоминаем самое начальное место лемминга и управляемого объекта
        if (leaderLemmingView != null)
        {
            _leaderStartPosition = leaderLemmingView.transform.position;
            _hasLeaderStart = true;
        }
        if (_lemmingPlaceView != null)
            _placeViewStartPosition = _lemmingPlaceView.transform.position;

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
        ResetPlaceViewToStart();
        ReviveLemmings(_continueLemmingsCount);
        _gameStatesUIMediator.ResumeAfterGameOver();
    }

    private void ClearAllObstacles()
    {
        if (_obstaclesSet == null)
            return;

        new DestroyAllObstacles(_obstaclesSet, _destroyAllObstaclesSound).Activate();
    }

    /// <summary>
    /// Возвращает управляемый объект (а вместе с ним формацию) на стартовое место — чтобы отряд не оказался над обрывом.
    /// </summary>
    private void ResetPlaceViewToStart()
    {
        if (_lemmingPlaceView == null)
            return;

        _lemmingPlaceView.transform.position = _placeViewStartPosition;
        _lemmingPlaceView.IsMovingLeft = false;
        _lemmingPlaceView.IsMovingRight = false;

        if (_lemmingPlaceView.Rigidbody != null)
        {
            _lemmingPlaceView.Rigidbody.linearVelocity = Vector3.zero;
            _lemmingPlaceView.Rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void ReviveLemmings(int count)
    {
        if (count <= 0)
            return;

        var prefabs = _randomSpawner.LemmingPrefabs;
        if (prefabs == null || prefabs.Count == 0)
            return;

        for (int i = 0; i < count; i++)
        {
            var prefab = prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
            if (prefab == null)
                continue;

            var instance = UnityEngine.Object.Instantiate(prefab);
            var lemming = instance.GetComponentInChildren<LemmingView>();
            if (lemming == null)
            {
                UnityEngine.Object.Destroy(instance);
                continue;
            }

            lemming.PickUp();
            _lemmingsEventsHandler.AddLemming(lemming);

            // Первого ставим точно на сохранённое стартовое место, остальных — на их места в строю
            if (i == 0 && _hasLeaderStart)
                lemming.transform.position = _leaderStartPosition;
            else if (lemming.RunningPlace != null)
                lemming.transform.position = lemming.RunningPlace.position;
        }
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
