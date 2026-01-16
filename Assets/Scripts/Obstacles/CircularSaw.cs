using UnityEngine;
using EzySlice;

public class CircularSaw : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10;
    [SerializeField]
    private Material _crossSectionWoodMaterial;
    [SerializeField]
    private Material _crossSectionLemmingMaterial;
    [SerializeField]
    private ParticleSystem _bloodParticles;


    private Material _crossSectionMaterial;

    private Vector3 _sawRotation = new Vector3(0,1,0);
    private GameObject[] _slicedObjects;
    private GameObject _slicedObject;
    private SlicedLemmingsHandler _slicedLemmingsHandler;

    private void Start()
    {
        _slicedLemmingsHandler = new SlicedLemmingsHandler();
        _bloodParticles.transform.SetParent(null);
    }

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


    private void SliceLemming()
    {
        _slicedObjects = Slice(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), new Vector3(0, 0, 1), new TextureRegion());
        Destroy(_slicedObject);
        _slicedLemmingsHandler.HandleSlicedLemmings(_slicedObjects[0], _slicedObjects[1]);
    }

    public GameObject[] Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection, TextureRegion region)
    {
        return _slicedObject.SliceInstantiate(planeWorldPosition, planeWorldDirection, region, _crossSectionMaterial);
    }

    private void Update()
    {
        RotateSaw();
    }

    private void RotateSaw()
    {
        transform.Rotate(_speed * Time.deltaTime * _sawRotation);
    }
}
