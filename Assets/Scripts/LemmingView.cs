using System;
using UnityEngine;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    
    [SerializeField]
    private LemmingConfig _config;

    [SerializeField]
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
    public Transform RunningPlace;

    public Animator Animator;
    private void Start()
    {
        if(IsRun)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
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
        if (IsRun)
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
        if (IsLeader)
        {
            Rigidbody.linearVelocity = new Vector3(0,0,0);
        }
        Vector3 velocity = Rigidbody.linearVelocity;
        

        velocity.z = _forwardSpeed;
        
        if (IsMovingRight)
        {
            velocity.x = _sideSpeed;
        }
        else if (IsMovingLeft)
        {
            velocity.x = -_sideSpeed;
        }
        else
        {
            velocity.x = 0;
        }
        
        Rigidbody.linearVelocity = velocity;

        if (IsOnFire)
        {
            Rigidbody.linearVelocity = new Vector3(0,0,_onFireSpeed);
            
        }
    }

    public void SetFire()
    {
        _fireObject.SetActive(true);
        RunningPlace = null;
        IsOnFire = true;
        
        OnLemmingKilled?.Invoke(this);
        
        IsLeader = false;
        
        Destroy(gameObject, 2f);
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
        OnLemmingKilled?.Invoke(this);
        if(!IsOnFire)
            Destroy(gameObject);
        else 
            Destroy(gameObject, 2f);
    }

    private void OnDestroy()
    {
        
    }
}
