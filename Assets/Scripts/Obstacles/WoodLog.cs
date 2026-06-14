using System;
using UnityEngine;

public class WoodLog : MonoBehaviour, IObstacle
{
    [SerializeField]
    private ParticleSystem _bloodParticles;
    
    [SerializeField]
    private float _rollSpeed = 5f;
    
    [SerializeField]
    private float _rotationSpeed = 200f;
    [SerializeField]
    private Rigidbody _rigidbody;
    
    
    private bool _isMoving = false;
    
    
    private void Awake()
    {
        
        _rigidbody.isKinematic = true;
    }
    
    private void Update()
    {
        if (_isMoving)
        {
            // Движение в направлении -Z
            transform.position += Vector3.back * _rollSpeed * Time.deltaTime;
            
            // Вращение бревна вокруг оси X (имитация качения)
            transform.Rotate(Vector3.right, _rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            if (lemmingView.IsRun)
            {
                lemmingView.Kill();
                SpawnBloodAtCollision(collision);
            }
            else
            {
                // Игнорируем столкновение с леммингами, которые не бегут
                Collider logCollider = collision.contacts[0].thisCollider;
                Collider lemmingCollider = collision.collider;
                Physics.IgnoreCollision(logCollider, lemmingCollider);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            if (lemmingView.IsRun)
            {
                StartMoving();
            }
        }
    }

    private void StartMoving()
    {
        _isMoving = true;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
    }
    
    private void SpawnBloodAtCollision(Collision collision)
    {
        // Получаем точку столкновения
        ContactPoint contact = collision.GetContact(0);
        Vector3 collisionPoint = contact.point;
        
        // Перемещаем систему частиц в точку столкновения
        _bloodParticles.transform.position = collisionPoint;
        
        // Устанавливаем направление частиц строго в -Z (независимо от вращения бревна)
        _bloodParticles.transform.rotation = Quaternion.LookRotation(Vector3.back);
        
        _bloodParticles.Play();
    }
}
