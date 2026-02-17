using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesSet : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _obstacles;

    public List<GameObject> Obstacles => _obstacles;

    /// <summary> Вызывается при добавлении препятствия/бонуса в набор (в т.ч. при спавне). </summary>
    public event Action<GameObject> OnObstacleAdded;

    public void AddObstacle(GameObject obj)
    {
        if (_obstacles == null)
            _obstacles = new List<GameObject>();
        _obstacles.Add(obj);
        OnObstacleAdded?.Invoke(obj);
    }
}
