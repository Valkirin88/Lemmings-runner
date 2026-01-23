using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LemmingPlaceView : MonoBehaviour
{
    public float SideSpeed;
    
    public float ForwardSpeed;
    
    public bool IsMovingLeft;
    public bool IsMovingRight;
    
    public Rigidbody Rigidbody;

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
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
}
