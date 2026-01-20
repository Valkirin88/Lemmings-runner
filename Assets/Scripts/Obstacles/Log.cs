using UnityEngine;

public class Log : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public float LifeTime;

    public bool IsDangerous;
    public bool IsPileDestroyed;
    public float Velocity;
    
    
    private void FixedUpdate()
    {
        SetDanger();
    }

    private void Update()
    {
        LifeTime += Time.deltaTime;
        if(IsPileDestroyed && Rigidbody.isKinematic)
        {
            Rigidbody.isKinematic = false;
            gameObject.transform.SetParent(null);
        }
    }

    private void SetDanger()
    {
        if (Rigidbody.linearVelocity.magnitude < 1 && IsPileDestroyed)
        {
            IsDangerous = false;
        }
        else if (Rigidbody.linearVelocity.magnitude > 1)
        {
            IsDangerous = true;
        }
        Velocity = Rigidbody.linearVelocity.magnitude;
    }

    public void StartMoving()
    {
        IsPileDestroyed = true;
    }
}
