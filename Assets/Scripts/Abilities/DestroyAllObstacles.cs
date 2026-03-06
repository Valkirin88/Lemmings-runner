using System;
using Object = UnityEngine.Object;

public class DestroyAllObstacles
{
    public event Action OnDeactivated;
    
    private readonly ObstaclesSet _obstaclesSet;
    
    public DestroyAllObstacles(ObstaclesSet obstaclesSet)
    {
        _obstaclesSet = obstaclesSet;
    }

    public void Activate()
    {
        foreach (var obsacle in _obstaclesSet.Obstacles)
        {
            Object.Destroy(obsacle);
        }
    }

    public void Deactivate()
    {
        OnDeactivated?.Invoke();
    }
}
