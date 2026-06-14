using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    
    private Vector3 _offset;
    
    private void Start()
    {
        if (_target != null)
        {
            _offset = transform.position - _target.position;
        }
    }
    
    private void LateUpdate()
    {
        if (_target == null)
            return;
        
        transform.position = _target.position + _offset;
    }
}
