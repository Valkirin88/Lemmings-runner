using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class LemmingPlaceView : MonoBehaviour
{
    public float SideSpeed;
    
    public float ForwardSpeed;
    
    public bool IsMovingLeft;
    public bool IsMovingRight;
    public bool IsMoving;
    
    public Rigidbody Rigidbody;
    
    public float AccelerateDuration;
    public float AccelerateMultiplier;
    
    private float _currentSpeedMultiplier = 1f;
    private bool _isAccelerating = false;

    private void FixedUpdate()
    {
        
        UpdateMovement();
    }

    public void Accelerate()
    {
        if (!_isAccelerating && IsMoving)
        {
            StartCoroutine(AccelerateCoroutine());
        }
    }
    
    private IEnumerator AccelerateCoroutine()
    {
        _isAccelerating = true;
        _currentSpeedMultiplier = AccelerateMultiplier;
        
        yield return new WaitForSeconds(AccelerateDuration);
        
        _currentSpeedMultiplier = 1f;
        _isAccelerating = false;
    }
    
    private void UpdateMovement()
    {
        if (IsMoving)
        {
            // Включаем физику если была выключена
            if (Rigidbody.isKinematic)
                Rigidbody.isKinematic = false;
            
            float yVelocity = Rigidbody.linearVelocity.y;

            float xVelocity = 0;

            if (IsMovingRight)
            {
                xVelocity = SideSpeed;
            }
            else if (IsMovingLeft)
            {
                xVelocity = -SideSpeed;
            }

            float currentForwardSpeed = ForwardSpeed * _currentSpeedMultiplier;
            Rigidbody.linearVelocity = new Vector3(xVelocity, yVelocity, currentForwardSpeed);
        }
        else
        {
            // Полностью останавливаем - делаем кинематическим
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.isKinematic = true;
        }
    }
}
