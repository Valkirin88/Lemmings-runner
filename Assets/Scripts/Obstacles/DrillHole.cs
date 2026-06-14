using System;
using UnityEngine;

public class DrillHole : MonoBehaviour, IObstacle 
{
    [SerializeField]
    private BloodZone _bloodZone;
    public event Action<GameObject> OnDestroyed;
    
    public BloodZone BloodZone => _bloodZone;
    public void SpawnBlood()
    {
        if (_bloodZone != null)
            _bloodZone.SpawnBlood();
    }

    public void MakeSound()
    {
        throw new System.NotImplementedException();
    }

    public void OnDestroy()
    {
        OnDestroyed?.Invoke(gameObject);
    }
}
