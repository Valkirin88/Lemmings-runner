using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesSoundMediator : IDisposable
{
    private readonly ObstaclesSet _obstacles;
    private readonly SoundsHandler _soundHandler;
    private readonly List<Bird> _birds = new List<Bird>();

    public ObstaclesSoundMediator(SoundsHandler soundsHandler, ObstaclesSet obstacles)
    {
        _soundHandler = soundsHandler;
        _obstacles = obstacles;
        SubscribeOnBirds();
    }

    private void SubscribeOnBirds()
    {
        if (_obstacles == null || _obstacles.Obstacles == null) return;
        
        foreach (var obstacle in _obstacles.Obstacles)
        {
            // Проверяем на null (уничтоженные объекты при рестарте)
            if (obstacle == null) continue;
            
            if (obstacle.TryGetComponent<Bird>(out Bird bird))
            {
                bird.OnMadeSound += MadeSound;
                bird.OnDestroyed += UnSubscribeBird;
                _birds.Add(bird);
            }
        }
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
        foreach (var bird in _birds)
        {
            if (bird == null) continue;
            
            bird.OnMadeSound -= MadeSound;
            bird.OnDestroyed -= UnSubscribeBird;
        }
        _birds.Clear();
    }
}
