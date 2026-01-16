using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    
    [SerializeField]
    private float _sideSpeed;
    [SerializeField]
    private float _forwardSpeed;
    [SerializeField]
    private float _followSpeed = 5f;
    [SerializeField]
    private float _stickDistance = 0.1f;
    [SerializeField]
    private float _stickSmoothing = 10f;
    
    [SerializeField]
    private Rigidbody _rigidbody;

    public bool IsMovingLeft;
    public bool IsMovingRight;
    public bool IsRun;
    public bool IsLeader;
    public Transform RunningPlace;

    public Animator Animator;
    private void Start()
    {
        if(IsRun)
        {
            Animator.SetBool("IsRun", true);
            // Разворачиваем вперед, если уже бежит
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }

    private void FixedUpdate()
    {
        if (IsLeader)
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
                _rigidbody.MovePosition(targetPosition);
            }
            else
            {
                // Держимся на месте - плавно следуем с инерцией
                Vector3 smoothedPosition = Vector3.Lerp(
                    transform.position,
                    RunningPlace.position,
                    _stickSmoothing * Time.fixedDeltaTime
                );
                _rigidbody.MovePosition(smoothedPosition);
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
                    Debug.Log("Collided");
                    lemmingView.IsRun = true;
                    lemmingView.Animator.SetBool("IsRun", true);
                    
                    // Разворачиваем лемминга вперед в направлении бега
                    lemmingView.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                    
                    OnLemmingCaught?.Invoke(lemmingView);
                }
            }
        }
    }

    private void UpdateMovement()
    {
        Vector3 velocity = _rigidbody.velocity;
        

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
        
        _rigidbody.velocity = velocity;
    }

    private void OnDestroy()
    {
        OnLemmingKilled?.Invoke(this);
    }
}
