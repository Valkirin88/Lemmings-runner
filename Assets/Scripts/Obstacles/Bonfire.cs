using System.Collections.Generic;
using UnityEngine;

public class Bonfire : MonoBehaviour, IObstacle
{
    [SerializeField]
    private GameObject _firePrefab;
    
    private List<LemmingView> _lemmingsInZone = new List<LemmingView>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            _lemmingsInZone.Add(lemmingView);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            _lemmingsInZone.Remove(lemmingView);
        }
    }

    private void Update()
    {
        for (int i = _lemmingsInZone.Count - 1; i >= 0; i--)
        {
            var lemming = _lemmingsInZone[i];
            
            if (lemming == null)
            {
                _lemmingsInZone.RemoveAt(i);
                continue;
            }
            
            if (!lemming.IsOnFire)
            {
                GameObject fireObject = Instantiate(_firePrefab);
                lemming.SetFire(fireObject);
            }
        }
    }
}
