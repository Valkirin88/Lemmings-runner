using System;
using UnityEngine;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    public event Action OnLemmingOnFire;
    
    [SerializeField]
    private LemmingConfig _config;

    private GameObject _fireObject;
    
    private float _followSpeed;
    private float _stickDistance ;
    private float _stickSmoothing;
    private float _onFireSpeed;
    private float _jumpForce;
    
    private bool _isJumping;
    
    public Rigidbody Rigidbody;

    // Внешние силы (ветер, и т.д.)
    private Vector3 _externalForce;

   public bool IsRun;
    public bool IsOnFire;
    public bool IsDead;
    public bool IsSliced;
    public Transform RunningPlace;

    public Animator Animator;
    private void Start()
    {
        if(IsRun)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }

        _followSpeed = _config.FollowSpeed;
        _stickDistance = _config.StickDistance;
        _stickSmoothing = _config.StickSmoothing;
        _onFireSpeed = _config.OnFireSpeed;
        _jumpForce = _config.JumpForce;
    }

    private void Update()
    {
        if (IsRun)
        {
            Animator.SetBool("IsRun", true);
        }
        else
        {
            Animator.SetBool("IsRun", false);
            // Не разворачиваем если лемминг захвачен (isKinematic)
            if (!Rigidbody.isKinematic)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.back);
            }
        }
    }

    private void FixedUpdate()
    {
        if (RunningPlace != null && IsRun)
        {
            
            Vector3 currentPos = transform.position;
            Vector3 targetPos = RunningPlace.position;
            
            float deltaX = targetPos.x - currentPos.x;
            float deltaZ = targetPos.z - currentPos.z;
            float distanceXZ = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            
            
            float speed = distanceXZ > _stickDistance ? _followSpeed : _stickSmoothing;
            
            
            Vector3 directionXZ = new Vector3(deltaX, 0, deltaZ).normalized;
            Vector3 velocityXZ = directionXZ * Mathf.Min(distanceXZ * speed, _followSpeed);
            
            // Добавляем внешние силы (ветер и т.д.) - напрямую к скорости
            velocityXZ.x += _externalForce.x;
            velocityXZ.z += _externalForce.z;
            
            // Сохраняем вертикальную скорость (гравитация/прыжок) + внешняя вертикальная сила
            float yVelocity = Rigidbody.linearVelocity.y + _externalForce.y;
            
            // Применяем скорость: X и Z к цели, Y от физики
            Rigidbody.linearVelocity = new Vector3(velocityXZ.x, yVelocity, velocityXZ.z);
        }
        else if (_externalForce.sqrMagnitude > 0.01f)
        {
            // Если лемминг не бежит, но есть внешняя сила - применяем её
            Vector3 vel = Rigidbody.linearVelocity;
            Rigidbody.linearVelocity = new Vector3(
                vel.x + _externalForce.x,
                vel.y + _externalForce.y,
                vel.z + _externalForce.z
            );
        }

        if (IsRun && IsOnFire)
        {
            UpdateMovement();
        }
        
        // Сбрасываем внешнюю силу
        _externalForce = Vector3.zero;
    }
    
    /// <summary>
    /// Добавить внешнюю силу (ветер, взрывы и т.д.)
    /// </summary>
    public void AddExternalForce(Vector3 force)
    {
        _externalForce += force;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (IsRun && !IsOnFire)
        {
            if (other.TryGetComponent(out LemmingView lemmingView))
            {
                if (!lemmingView.IsRun)
                {
                    lemmingView.IsRun = true;
                    
                    // Разворачиваем лемминга вперед в направлении бега
                    lemmingView.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                    
                    OnLemmingCaught?.Invoke(lemmingView);
                }
            }
        }

        if (other.TryGetComponent(out EndTrack endTrack))
        {
            IsRun = false;
        }
    }

    private void UpdateMovement()
    {
        float yVelocity = Rigidbody.linearVelocity.y;
        
        if (IsOnFire)
        {
            Rigidbody.linearVelocity = new Vector3(0, yVelocity, _onFireSpeed);
            return;
        }
    }

    public void Jump()
    {
        if (IsRun && !IsDead && !_isJumping)
        {
            _isJumping = true;
            Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем приземление
        foreach (var contact in collision.contacts)
        {
            // Если нормаль направлена вверх — это земля
            if (contact.normal.y > 0.5f)
            {
                _isJumping = false;
                break;
            }
        }
    }

    public void SetFire(GameObject fireObject)
    {
        _fireObject = fireObject;
        _fireObject.transform.SetParent(transform);
        _fireObject.transform.localPosition = Vector3.zero;
        _fireObject.SetActive(true);
        OnLemmingOnFire?.Invoke();
        RunningPlace = null;
        IsOnFire = true;
        
        Kill();
        
    }

    public void Kill()
    {
        if (IsDead) return;
        
        IsDead = true;
        OnLemmingKilled?.Invoke(this);
        
        // Добавляем пятна крови на экран (только если не горит)
        if (!IsOnFire && BloodSplatterManager.Instance != null)
        {
            BloodSplatterManager.Instance.AddSplattersOnKill();
        }
        
        // Пятна на поверхности создаются через BloodZone на препятствиях
        
        if (!IsOnFire)
        {
            Destroy(gameObject);
        }
        else 
        {
            Destroy(gameObject, 2f);
        }
    }

    public void KillWithotBlood()
    {
        if (IsDead) return;
        
        IsDead = true;
        OnLemmingKilled?.Invoke(this);
        
        if (!IsOnFire)
        {
            Destroy(gameObject);
        }
        else 
        {
            Destroy(gameObject, 2f);
        }
    }

    private void OnDestroy()
    {
        
    }
}
