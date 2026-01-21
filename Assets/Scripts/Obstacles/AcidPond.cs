using UnityEngine;

public class AcidPond : MonoBehaviour, IObstacle
{
    [SerializeField] 
    private ParticleSystem _bloodParticles;
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
            _bloodParticles.Play();
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
}
