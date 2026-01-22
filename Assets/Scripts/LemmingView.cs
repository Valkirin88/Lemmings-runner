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
    
    private float _sideSpeed;
    private float _forwardSpeed;
    private float _followSpeed;
    private float _stickDistance ;
    private float _stickSmoothing;
    private float _onFireSpeed;
    
    public Rigidbody Rigidbody;

    public bool IsMovingLeft;
    public bool IsMovingRight;
    public bool IsRun;
    public bool IsLeader;
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

        // Включаем интерполяцию для плавного движения между кадрами физики
        if (Rigidbody != null)
        {
            Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        _sideSpeed = _config.SideSpeed;
        _forwardSpeed = _config.ForwardSpeed;
        _followSpeed = _config.FollowSpeed;
        _stickDistance = _config.StickDistance;
        _stickSmoothing = _config.StickSmoothing;
        _onFireSpeed = _config.OnFireSpeed;
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
            transform.rotation = Quaternion.LookRotation(Vector3.back);
        }

        CheckIfOnPlate();
    }

    private void FixedUpdate()
    {
        if (IsLeader && IsRun)
        {
            UpdateMovement();
        }
        else if (RunningPlace != null && IsRun)
        {
            float distance = Vector3.Distance(transform.position, RunningPlace.position);

            if (distance > _stickDistance)
            {
                // Быстро догоняем
                Vector3 targetPosition = Vector3.MoveTowards(
                    transform.position,
                    RunningPlace.position,
                    _followSpeed * Time.fixedDeltaTime
                );
                Rigidbody.MovePosition(targetPosition);
            }
            else
            {
                // Держимся на месте - плавно следуем с инерцией
                Vector3 smoothedPosition = Vector3.Lerp(
                    transform.position,
                    RunningPlace.position,
                    _stickSmoothing * Time.fixedDeltaTime
                );
                Rigidbody.MovePosition(smoothedPosition);
            }
        }

        if (IsRun && IsOnFire)
        {
            UpdateMovement();
        }
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

        float xVelocity = 0;
        
        if (IsMovingRight)
        {
            xVelocity = _sideSpeed;
        }
        else if (IsMovingLeft)
        {
            xVelocity = -_sideSpeed;
        }
        
        Rigidbody.linearVelocity = new Vector3(xVelocity, yVelocity, _forwardSpeed);
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
        
        IsLeader = false;
    }

    private void CheckIfOnPlate()
    {
        if (RunningPlace != null)
        {
            float distance = Vector3.Distance(transform.position, RunningPlace.position);
            if (distance > 2f)
            {
                Kill();
            }
        }
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
