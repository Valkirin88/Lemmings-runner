using System;
using UnityEngine;

public class Bird : MonoBehaviour, IObstacle
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField] private float _patrolSpeed = 5f;
    
    [Header("Hunt Settings")]
    [SerializeField] private float _huntSpeed = 10f;
    [SerializeField] private float _catchDistance = 0.3f;
    [SerializeField] private float _carryUpSpeed = 8f;
    [SerializeField] private float _killHeight = 10f;
    
    [Header("Gizmos")]
    [SerializeField] private Color _patrolLineColor = Color.yellow;
    
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 5f;
    [Header("Sounds")]
    [SerializeField]
    private AudioClip _birdClip;
    
    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;
    
    // IObstacle implementation (птица не оставляет крови)
    public BloodZone BloodZone => null;
    
    private LemmingView _targetLemming;
    private bool _isHunting;
    private bool _isCarrying;
    private bool _movingToB = true;
    private Vector3 _startCarryPosition;
    
    // Кэшированные мировые позиции точек патрулирования
    private Vector3 _pointAPosition;
    private Vector3 _pointBPosition;
    
    // Направление птицы в момент захвата лемминга
    private Vector3 _carryDirection;
    
    private void Start()
    {
        // Сохраняем мировые позиции точек патрулирования
        if (_pointA != null) _pointAPosition = _pointA.position;
        if (_pointB != null) _pointBPosition = _pointB.position;
    }
    
    private void Update()
    {
        if (_isCarrying)
        {
            CarryLemming();
        }
        else if (_isHunting && _targetLemming != null)
        {
            HuntLemming();
        }
        else
        {
            Patrol();
        }
    }
    
    private void Patrol()
    {
        Vector3 targetPoint = _movingToB ? _pointBPosition : _pointAPosition;
        Vector3 direction = (targetPoint - transform.position);
        
        // Проверяем достижение точки
        if (direction.magnitude < 0.1f)
        {
            _movingToB = !_movingToB;
            return;
        }
        
        // Плавно поворачиваем птицу в направлении движения
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        
        // Двигаемся вперёд (в направлении взгляда птицы)
        transform.position += transform.forward * _patrolSpeed * Time.deltaTime;
    }
    
    private void HuntLemming()
    {
        if (_targetLemming == null || _targetLemming.IsDead)
        {
            StopHunting();
            return;
        }
        
        Vector3 targetPosition = _targetLemming.transform.position;
        Vector3 direction = (targetPosition - transform.position);
        float distance = direction.magnitude;
        
        // Проверяем дистанцию для захвата
        if (distance <= _catchDistance)
        {
            CatchLemming();
            return;
        }
        
        // Плавно поворачиваем птицу к лемминга
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * 2f * Time.deltaTime);
        
        // Двигаемся к лемминга
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, _huntSpeed * Time.deltaTime);
    }
    
    private void CatchLemming()
    {
        if (_targetLemming == null) return;
        
        // Проверяем что лемминг ещё жив и бежит
        if (_targetLemming.IsDead || !_targetLemming.IsRun)
        {
            StopHunting();
            return;
        }
        
        // Лемминг захвачен птицей
        _targetLemming.CaptureByBird();
        
        // Привязываем лемминга к птице
        _targetLemming.transform.SetParent(transform);
        
        _isCarrying = true;
        _isHunting = false;
        _startCarryPosition = transform.position;
        
        // Запоминаем направление птицы для взлёта
        _carryDirection = transform.forward;
        _targetLemming.CauughtByBird();
    }
    
    private void CarryLemming()
    {
        // Направление полёта - вперёд и вверх (под крутым углом ~70 градусов)
        Vector3 flyDirection = (_carryDirection * 0.4f + Vector3.up).normalized;
        
        // Плавно поворачиваем птицу в направлении полёта
        Quaternion targetRotation = Quaternion.LookRotation(flyDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        
        // Летим в направлении взгляда птицы
        transform.position += transform.forward * _carryUpSpeed * Time.deltaTime;
        
        // Проверяем высоту для убийства
        float heightTraveled = transform.position.y - _startCarryPosition.y;
        if (heightTraveled >= _killHeight)
        {
            KillCarriedLemming();
        }
    }
    
    private void KillCarriedLemming()
    {
        if (_targetLemming != null)
        {
            _targetLemming.transform.SetParent(null);
            _targetLemming.KillWithotBlood();
        }
        
        _targetLemming = null;
        _isCarrying = false;
        
        // Уничтожаем птицу после убийства (улетела)
        Destroy(gameObject);
    }
    
    private void StopHunting()
    {
        _targetLemming = null;
        _isHunting = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Если уже охотимся или несём - игнорируем
        if (_isHunting || _isCarrying) return;
        
        if (other.TryGetComponent(out LemmingView lemmingView))
        {
            // Проверяем что лемминг жив и бежит
            if (!lemmingView.IsDead && lemmingView.IsRun)
            {
                _targetLemming = lemmingView;
                _isHunting = true;
                MakeSound(); // Крик при начале охоты
            }
        }
    }
    
    public void SpawnBlood()
    {
        // Птица не оставляет крови
    }

    public void MakeSound()
    {
        OnMadeSound?.Invoke(_birdClip);
    }

    public void OnDestroy()
    {
        OnDestroyed?.Invoke(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (_pointA == null || _pointB == null) return;
        
        Gizmos.color = _patrolLineColor;
        Gizmos.DrawLine(_pointA.position, _pointB.position);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_pointA == null || _pointB == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_pointA.position, _pointB.position);
    }
}
