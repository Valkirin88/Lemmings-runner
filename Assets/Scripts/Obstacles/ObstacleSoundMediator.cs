using System;
using System.Collections.Generic;
using UnityEngine;

public class EventsSoundMediator : IDisposable
{
    private readonly ObstaclesSet _obstacles;
    private readonly SoundsHandler _soundHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly List<Bird> _birds = new List<Bird>();

    public EventsSoundMediator(SoundsHandler soundsHandler, ObstaclesSet obstacles, LemmingsEventsHandler lemmingsEventsHandler)
    {
        _soundHandler = soundsHandler;
        _obstacles = obstacles;
        _lemmingsEventsHandler = lemmingsEventsHandler;
        
        SubscribeOnBirds();
        if (_obstacles != null)
            _obstacles.OnObstacleAdded += OnObstacleAdded;

        _lemmingsEventsHandler.OnCurrencyGot += PlayBonusSound;
        _lemmingsEventsHandler.OnLemmingCountAdd += PlayNewLemming;
    }

    private void PlayNewLemming(LemmingView obj)
    {
        _soundHandler.PlayAddLemming();
    }

    private void PlayBonusSound(int score)
    {
        _soundHandler.PlayBonusGot();
    }

    private void OnObstacleAdded(GameObject obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent<Bird>(out Bird bird))
            SubscribeBird(bird);
    }

    private void SubscribeOnBirds()
    {
        if (_obstacles == null || _obstacles.Obstacles == null) return;

        foreach (var obstacle in _obstacles.Obstacles)
        {
            if (obstacle == null) continue;
            if (obstacle.TryGetComponent<Bird>(out Bird bird))
                SubscribeBird(bird);
        }
    }

    private void SubscribeBird(Bird bird)
    {
        if (bird == null) return;
        bird.OnMadeSound += MadeSound;
        bird.OnDestroyed += UnSubscribeBird;
        _birds.Add(bird);
    }

    private void UnSubscribeBird(GameObject birdObject)
    {
        if (birdObject == null) return;
        
        var bird = birdObject.GetComponent<Bird>();
        if (bird == null) return;
        
        bird.OnMadeSound -= MadeSound;
        bird.OnDestroyed -= UnSubscribeBird;
        _birds.Remove(bird);
    }

    private void MadeSound(AudioClip clip)
    {
        _soundHandler.PlaySound(clip);
    }

    public void Dispose()
    {
        if (_obstacles != null)
            _obstacles.OnObstacleAdded -= OnObstacleAdded;
        foreach (var bird in _birds)
        {
            if (bird == null) continue;
            bird.OnMadeSound -= MadeSound;
            bird.OnDestroyed -= UnSubscribeBird;
        }
        _birds.Clear();
        
        _lemmingsEventsHandler.OnCurrencyGot -= PlayBonusSound;
        _lemmingsEventsHandler.OnLemmingCountAdd -= PlayNewLemming;
    }
}
