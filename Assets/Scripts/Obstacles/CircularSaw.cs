using UnityEngine;
using EzySlice;

public class CircularSaw : MonoBehaviour, IObstacle
{
    [SerializeField]
    private float _speed = 10;
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
        
        _crossSectionMaterial = _crossSectionLemmingMaterial;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            // Проверяем, что лемминг ещё жив
            if (lemmingView.IsDead) return;
            
            _bloodParticles.Play();
            
            _slicedObject = lemmingView.gameObject;
            SliceLemming();
            lemmingView.Kill();
        }
    }


    private void SliceLemming()
    {
        Vector3 lemmingPosition = _slicedObject.transform.position;
        // X от пилы (где лезвие), Y и Z от лемминга
        Vector3 slicePlanePosition = new Vector3(transform.position.x, lemmingPosition.y + 0.2f, lemmingPosition.z);
        
        _slicedObjects = Slice(slicePlanePosition, new Vector3(0, 0, 1), new TextureRegion());
        
        if (_slicedObjects == null || _slicedObjects.Length < 2)
        {
            Debug.LogError("Slice failed!");
            return;
        }
        
        Debug.Log($"Lemming pos: {lemmingPosition}, Part1 pos: {_slicedObjects[0].transform.position}, Part2 pos: {_slicedObjects[1].transform.position}");
        _slicedLemmingsHandler.HandleSlicedLemmings(_slicedObjects[0], _slicedObjects[1], _bloodParticles);
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
