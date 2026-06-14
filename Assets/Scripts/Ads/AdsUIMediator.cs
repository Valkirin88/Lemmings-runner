using System;
using UnityEngine;

public class AdsUIMediator : IDisposable
{
    private readonly AdsHandler _adsHandler;
    private readonly UIHandler _uiHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly RandomSpawner _randomSpawner;
    private readonly GameStatesUIMediator _gameStatesUIMediator;
    private readonly ObstaclesSet _obstaclesSet;
    private readonly AudioClip _destroyAllObstaclesSound;
    private readonly LemmingPlaceView _lemmingPlaceView;

    private readonly int _continueLemmingsCount;

    private readonly Vector3 _leaderStartPosition;
    private readonly bool _hasLeaderStart;
    private readonly Vector3 _placeViewStartPosition;

    public AdsUIMediator(
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
        _adsHandler = adsHandler;
        _uiHandler = uiHandler;
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _randomSpawner = randomSpawner;
        _gameStatesUIMediator = gameStatesUIMediator;
        _obstaclesSet = obstaclesSet;
        _destroyAllObstaclesSound = destroyAllObstaclesSound;
        _lemmingPlaceView = lemmingPlaceView;
        _continueLemmingsCount = Mathf.Max(1, continueLemmingsCount);

        if (leaderLemmingView != null)
        {
            _leaderStartPosition = leaderLemmingView.transform.position;
            _hasLeaderStart = true;
        }
        if (_lemmingPlaceView != null)
            _placeViewStartPosition = _lemmingPlaceView.transform.position;

        _uiHandler.OnResumedAfterGameOver += ShowAds;
        _uiHandler.OnContinueGameplay += ContinueGameplay;
        _adsHandler.OnAdsLoaded += RefreshContinueButton;
        _adsHandler.OnAdsLoadFailed += RefreshContinueButton;
        _adsHandler.OnAdsUnloaded += RefreshContinueButton;
        _adsHandler.OnAdsRewarded += GrantReward;

        _uiHandler.IsAdReadyCheck = () => _adsHandler != null && _adsHandler.IsAdReady;

        RefreshContinueButton();
    }

    private void ShowAds()
    {
        _adsHandler.ShowRewardedAd();
    }

    private void RefreshContinueButton()
    {
        _uiHandler.RefreshGameOverContinueButton();
    }

    private void GrantReward()
    {
        _uiHandler.IsAdsNeeded = false;
        _uiHandler.OnAdsRewardGranted();
        RefreshContinueButton();
    }

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
        _adsHandler.OnAdsLoaded -= RefreshContinueButton;
        _adsHandler.OnAdsLoadFailed -= RefreshContinueButton;
        _adsHandler.OnAdsUnloaded -= RefreshContinueButton;
        _adsHandler.OnAdsRewarded -= GrantReward;
    }
}
