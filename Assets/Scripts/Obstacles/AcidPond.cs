using UnityEngine;

public class AcidPond : MonoBehaviour, IObstacle
{
    [SerializeField] 
    private ParticleSystem _bloodParticles;
    
    [SerializeField]
    private BloodZone _bloodZone;
    public BloodZone BloodZone => _bloodZone;
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
            _bloodParticles.Play();
            SpawnBlood();
            lemming.Kill();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
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
}
