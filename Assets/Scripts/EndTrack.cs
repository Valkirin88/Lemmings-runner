using UnityEngine;
using System;

public class EndTrack : MonoBehaviour
{
    public event Action OnFinished;
    
    private bool _isFinished;
    
    private void OnTriggerEnter(Collider other)
    {
        if (_isFinished) return;
        
        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            _isFinished = true;
            OnFinished?.Invoke();
        }
    }
}
