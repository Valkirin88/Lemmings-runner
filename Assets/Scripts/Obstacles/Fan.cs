using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Fan : MonoBehaviour, IObstacle
{
    [Header("Wind Settings")]
    [SerializeField] private float _windForce = 5f;
    [SerializeField] private Vector3 _windDirection = Vector3.right;
    [SerializeField] private bool _useLocalDirection = true;
    [SerializeField] private BloodZone _bloodZone;
    
    public event Action<AudioClip> OnMadeSound;
    public event Action OnDestroyed;
    
    private Vector3 _actualWindDirection;
    private LemmingPlaceView _currentTarget;
    
    // Кэшированные значения для оптимизации
    private Quaternion _cachedRotation;
    private Vector3 _cachedWindDirection;
    private bool _cachedUseLocalDirection;
    
    private void Awake()
    {
        // Убеждаемся что коллайдер - триггер
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
    
    private void Start()
    {
        ForceUpdateWindDirection();
    }
    
    private void FixedUpdate()
    {
        // Пересчитываем только если что-то изменилось
        if (NeedsDirectionUpdate())
        {
            ForceUpdateWindDirection();
        }
        
        // Применяем силу пока цель в зоне
        if (_currentTarget != null)
        {
            Vector3 windForce = _actualWindDirection * _windForce;
            _currentTarget.AddExternalForce(windForce);
        }
    }
    
    private bool NeedsDirectionUpdate()
    {
        return _cachedUseLocalDirection != _useLocalDirection ||
               _cachedWindDirection != _windDirection ||
               (_useLocalDirection && _cachedRotation != transform.rotation);
    }
    
    private void ForceUpdateWindDirection()
    {
        _cachedRotation = transform.rotation;
        _cachedWindDirection = _windDirection;
        _cachedUseLocalDirection = _useLocalDirection;
        
        if (_useLocalDirection)
        {
            _actualWindDirection = transform.TransformDirection(_windDirection.normalized);
        }
        else
        {
            _actualWindDirection = _windDirection.normalized;
        }
    }
    
    // Для обратной совместимости (вызывается из DrawGizmos)
    private void UpdateWindDirection()
    {
        ForceUpdateWindDirection();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var placeView = other.GetComponentInParent<LemmingPlaceView>();
        if (placeView == null)
            placeView = other.GetComponent<LemmingPlaceView>();
            
        if (placeView != null)
        {
            _currentTarget = placeView;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        var placeView = other.GetComponentInParent<LemmingPlaceView>();
        if (placeView == null)
            placeView = other.GetComponent<LemmingPlaceView>();
            
        if (placeView != null && placeView == _currentTarget)
        {
            _currentTarget = null;
        }
    }

    public void MakeSound()
    {
        
    }

    public void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    // Визуализация
    private void OnDrawGizmos()
    {
        DrawGizmos(false);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawGizmos(true);
    }
    
    private void DrawGizmos(bool isSelected)
    {
        UpdateWindDirection();
        
        // Стрелка направления ветра
        Gizmos.color = isSelected ? new Color(0.2f, 0.7f, 1f, 0.9f) : new Color(0.2f, 0.7f, 1f, 0.5f);
        Vector3 arrowEnd = transform.position + _actualWindDirection * 2f;
        Gizmos.DrawLine(transform.position, arrowEnd);
        
        // Наконечник
        Vector3 right = Vector3.Cross(_actualWindDirection, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.Cross(_actualWindDirection, Vector3.right).normalized;
        Gizmos.DrawLine(arrowEnd, arrowEnd - _actualWindDirection * 0.4f + right * 0.2f);
        Gizmos.DrawLine(arrowEnd, arrowEnd - _actualWindDirection * 0.4f - right * 0.2f);
        
        // Box Collider
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = isSelected ? new Color(1f, 1f, 0f, 0.4f) : new Color(1f, 1f, 0f, 0.15f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
            if (isSelected)
            {
                Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.1f);
                Gizmos.DrawCube(box.center, box.size);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
        
        #if UNITY_EDITOR
        if (isSelected)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up, $"Force: {_windForce}");
        }
        #endif
    }

    public BloodZone BloodZone { get; }
    public void SpawnBlood()
    {
        if (_bloodZone != null)
        {
            _bloodZone.SpawnBlood();
        }
    }
}
