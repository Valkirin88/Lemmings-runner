using System;
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

    private void FixedUpdate()
    {
        
            UpdateMovement();
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

            Rigidbody.linearVelocity = new Vector3(xVelocity, yVelocity, ForwardSpeed);
        }
        else
        {
            // Полностью останавливаем - делаем кинематическим
            Rigidbody.linearVelocity = Vector3.zero;
            Rigidbody.isKinematic = true;
        }
    }
}
