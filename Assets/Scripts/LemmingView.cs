using System;
using System.Collections;
using UnityEngine;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    public event Action<LemmingView> OnLemmingCapturedByBird;
    public event Action OnLemmingOnDanger;
    public event Action<AppleCurrency> OnScoreBonusGot;
    
    [SerializeField]
    private LemmingConfig _config;

    [SerializeField] private GameObject _auraObject;

    [Header("Highlight Settings")]
    [SerializeField]
    private Outline _outline; // Компонент обводки

    private bool _wasPickedUp; // Был ли лемминг когда-либо подобран — пока false, аура включена

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

   /// <summary>
    /// true = подобран другими леммингами. Препятствия проверяют IsRun — убивают только при true.
    /// </summary>
    public bool IsRun;
    /// <summary>
    /// true = движется в -Z вместе с миром (ожидающий лемминг), false = не двигается скроллом.
    /// </summary>
    public bool IsScroll;
    public bool IsOnFire;
    public bool IsDead;
    public bool IsSliced;
    public bool IsInvincible;
    /// <summary>
    /// true = лемминга подбросило препятствие (например JumpTrap).
    /// </summary>
    public bool IsPushed;
    
    public Transform RunningPlace;

    public Animator Animator;
    
    private void Awake()
    {
        // Инициализируем подсветку в Awake, до Start
        InitializeHighlight();
        // По умолчанию аура включена (лемминг ждёт, чтобы его подобрали)
        SetAuraActive(true);
    }
    
    private void Start()
    {
        if(IsRun)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
            _wasPickedUp = true; // Лидер уже "подобран"
            DisableHighlight(); // Лидер без подсветки
            SetAuraActive(false); // Лидеру аура не нужна
        }

        _followSpeed = _config.FollowSpeed;
        _stickDistance = _config.StickDistance;
        _stickSmoothing = _config.StickSmoothing;
        _onFireSpeed = _config.OnFireSpeed;
        _jumpForce = _config.JumpForce;
    }
    
    private void InitializeHighlight()
    {
        // Если компонент Outline не назначен - пробуем найти.
        // Состояние enabled не трогаем — берём из инспектора.
        if (_outline == null)
        {
            _outline = GetComponent<Outline>();
        }
    }
    
    private void DisableHighlight()
    {
        if (_outline != null)
        {
            _outline.enabled = false;
        }
    }

    private void SetAuraActive(bool isActive)
    {
        if (_auraObject != null)
        {
            _auraObject.SetActive(isActive);
        }
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
        // Не управляем velocity если Rigidbody кинематический (схвачен птицей)
        if (Rigidbody.isKinematic)
        {
            _externalForce = Vector3.zero;
            return;
        }
        // Подброшенный лемминг — не перезаписываем скорость (импульс от JumpTrap и т.д.)
        if (IsPushed)
        {
            _externalForce = Vector3.zero;
            return;
        }
        
        if (RunningPlace != null && IsRun)
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPos = RunningPlace.position;
            
            float deltaX = targetPos.x - currentPos.x;
            float deltaZ = targetPos.z - currentPos.z;
            float distanceXZ = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            
            // Цель впереди — догоняем на полной скорости; иначе при приближении — плавное следование
            bool targetAhead = deltaZ > 0.1f;
            float speed = (distanceXZ > _stickDistance || targetAhead) ? _followSpeed : _stickSmoothing;
            
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
        else if (IsScroll && !IsDead)
        {
            // Ожидающий лемминг — движется в -Z вместе с миром (Obstacles).
            float scrollSpeed = ScrollSpeedProvider.CurrentSpeed;
            Vector3 vel = Rigidbody.linearVelocity;
            vel.z = -scrollSpeed + _externalForce.z;
            vel.x += _externalForce.x;
            vel.y += _externalForce.y;
            Rigidbody.linearVelocity = vel;
        }
        else if (_externalForce.sqrMagnitude > 0.01f)
        {
            // Внешняя сила (птица отпустила и т.д.)
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
                // Подбираем только если лемминг ещё не был подобран другим леммингом
                if (!lemmingView.IsRun && !lemmingView._wasPickedUp)
                {
                    lemmingView.PickUp();
                    
                    OnLemmingCaught?.Invoke(lemmingView);
                }
            }
        }
        
        
        if (other.TryGetComponent(out AppleCurrency scoreBonus))
        {
            OnScoreBonusGot?.Invoke(scoreBonus);
            Destroy(scoreBonus.gameObject);
        }
    }
    
    public void PickUp()
    {
        if (_wasPickedUp) return;
        
        _wasPickedUp = true;
        IsRun = true;
        IsScroll = false;
        
        // Разворачиваем лемминга вперед в направлении бега
        transform.rotation = Quaternion.LookRotation(Vector3.forward);
        
        // Выключаем подсветку и ауру — лемминга уже подобрали
        DisableHighlight();
        SetAuraActive(false);
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

    public void CaughtByBird()
    {
        OnLemmingOnDanger?.Invoke();
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

    [SerializeField]
    private float _fireDeathDelay = 2f;

    public void SetFire(GameObject fireObject)
    {
        _fireObject = fireObject;
        _fireObject.transform.SetParent(transform);
        _fireObject.transform.localPosition = Vector3.zero;
        _fireObject.SetActive(true);
        OnLemmingOnDanger?.Invoke();
        RunningPlace = null;
        IsOnFire = true;
        
       StartCoroutine(KillFromFireAfterDelay());
    }

    private IEnumerator KillFromFireAfterDelay()
    {
        yield return new WaitForSeconds(_fireDeathDelay);
        if (!IsDead)
            KillWithotBlood();
    }

    /// <summary>
    /// Вызывается когда птица захватывает лемминга
    /// </summary>
    public void CaptureByBird()
    {
        IsRun = false;
        IsScroll = false;
        
        // Делаем кинематическим
        Rigidbody.isKinematic = true;
        
        // Уведомляем о захвате (для перестроения)
        OnLemmingCapturedByBird?.Invoke(this);
    }
    
    /// <param name="destroyImmediately">true — убрать объект сразу (например при убийстве препятствием), иначе горящий исчезнет через 2 сек</param>
    public void Kill(bool destroyImmediately = false)
    {
        if (IsDead)
        {
            if (destroyImmediately)
                Destroy(gameObject);
            return;
        }

        IsDead = true;

        OnLemmingKilled?.Invoke(this);

        // Добавляем пятна крови на экран (только если не горит)
        if (!IsOnFire && BloodSplatterManager.Instance != null)
        {
            BloodSplatterManager.Instance.AddSplattersOnKill();
        }

        // Пятна на поверхности создаются через BloodZone на препятствиях
        if (!IsOnFire || destroyImmediately)
            Destroy(gameObject);
        else
            Destroy(gameObject, 2f);
    }

    public void KillWithotBlood()
    {
        if (IsDead) return;
        
        IsDead = true;
        
        OnLemmingKilled?.Invoke(this);

            Destroy(gameObject);
     }

    private void OnDestroy()
    {
        
    }
}
