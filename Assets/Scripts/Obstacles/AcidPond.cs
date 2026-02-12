using System;
using UnityEngine;

public class AcidPond : MonoBehaviour, IObstacle
{
    [SerializeField] 
    private ParticleSystem _bloodParticles;
    
    [SerializeField]
    private BloodZone _bloodZone;
    public BloodZone BloodZone => _bloodZone;
    
    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
            if (!lemming.IsRun && !lemming.IsOnFire) return;
            _bloodParticles.Play();
            SpawnBlood();
            lemming.Kill();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
            if (!lemming.IsRun && !lemming.IsOnFire) return;
            _bloodParticles.Play();
            lemming.Kill();
        }
    }
    
    public void SpawnBlood()
    {
        if (_bloodZone != null)
        {
            _bloodZone.SpawnBlood();
        }
    }

    public void MakeSound()
    {
        
    }

    public void OnDestroy()
    {
        OnDestroyed?.Invoke(gameObject);
    }
}
