using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;


public class LemmingView : MonoBehaviour
{
    public event Action<LemmingView> OnLemmingCaught; 
    public event Action<LemmingView> OnLemmingKilled;
    
    [SerializeField] private float _sideSpeed;
    [SerializeField] private float _forwardSpeed;

    
    
    [SerializeField] private Rigidbody _rigidbody;

    public bool IsMovingLeft;
    public bool IsMovingRight;
    public bool IsRun;
    public bool IsLeader;

    private void FixedUpdate()
    {
        if (IsLeader)
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
