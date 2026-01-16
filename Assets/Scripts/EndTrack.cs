using UnityEngine;
using System;

public class EndTrack : MonoBehaviour
{
    public event Action OnFinished;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            OnFinished?.Invoke();
        }
    }
}
