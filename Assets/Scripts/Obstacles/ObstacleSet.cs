using System.Collections.Generic;
using UnityEngine;

public class ObstaclesSet : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _obstacles;

    public List<GameObject> Obstacles => _obstacles;
}
