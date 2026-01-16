using System;
using UnityEngine;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    
    [SerializeField]
    private LemmingConfig _config;
    
    private float _sideSpeed;
    private float _forwardSpeed;
    private float _followSpeed;
    private float _stickDistance ;
    private float _stickSmoothing;
    
    public Rigidbody Rigidbody;

    public bool IsMovingLeft;
    public bool IsMovingRight;
    public bool IsRun;
    public bool IsLeader;
    public bool IsFinished;
    public Transform RunningPlace;

    public Animator Animator;
    private void Start()
    {
        if(IsRun)
        {
            Animator.SetBool("IsRun", true);
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }

        _sideSpeed = _config.SideSpeed;
        _forwardSpeed = _config.ForwardSpeed;
        _followSpeed = _config.FollowSpeed;
        _stickDistance = _config.StickDistance;
        _stickSmoothing = _config.StickSmoothing;
    }

    private void FixedUpdate()
    {
        if (IsLeader && IsRun)
        {
            UpdateMovement();
        }
        else if (RunningPlace != null)
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
                    lemmingView.Animator.SetBool("IsRun", true);
                    
                    // Разворачиваем лемминга вперед в направлении бега
                    lemmingView.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                    
                    OnLemmingCaught?.Invoke(lemmingView);
                }
            }
        }

        if (other.TryGetComponent(out EndTrack endTrack))
        {
            IsRun = false;
            IsFinished = true;
            Animator.SetBool("IsRun", false);
            
            
        }
    }

    private void UpdateMovement()
    {
        if (IsLeader)
        {
            Rigidbody.velocity = new Vector3(0,0,0);
        }
        Vector3 velocity = Rigidbody.velocity;
        

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
        
        Rigidbody.velocity = velocity;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnLemmingKilled?.Invoke(this);
    }
}
