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
    
    // Внешние силы (ветер и т.д.)
    private Vector3 _externalForce;

    private void FixedUpdate()
    {
        UpdateMovement();
        
        // Сбрасываем внешнюю силу после применения
        _externalForce = Vector3.zero;
    }
    
    /// <summary>
    /// Добавить внешнюю силу (ветер, и т.д.)
    /// </summary>
    public void AddExternalForce(Vector3 force)
    {
        _externalForce += force;
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

            // Добавляем внешние силы (ветер)
            xVelocity += _externalForce.x;
            
            float currentForwardSpeed = ForwardSpeed * _currentSpeedMultiplier;
            Rigidbody.linearVelocity = new Vector3(xVelocity, yVelocity + _externalForce.y, currentForwardSpeed + _externalForce.z);
        }
        else
        {
            // Полностью останавливаем - делаем кинематическим
            if (!Rigidbody.isKinematic)
            {
                Rigidbody.linearVelocity = Vector3.zero;
                Rigidbody.isKinematic = true;
            }
        }
    }
}
