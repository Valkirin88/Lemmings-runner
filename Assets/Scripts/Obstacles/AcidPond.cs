using UnityEngine;

public class AcidPond : MonoBehaviour
{
    [SerializeField] 
    private ParticleSystem _bloodParticles;
    
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemming))
        {
            _bloodParticles.Play();
            lemming.Kill();
            // lemming.gameObject.transform.position = new Vector3(lemming.gameObject.transform.position.x, lemming.gameObject.transform.position.y, transform.position.z);
            // _slicedObject = lemming.gameObject;
            // _crossSectionMaterial = _crossSectionLemmingMaterial;
            // SliceLemming();
        }
    }
}
