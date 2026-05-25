using System;
using UnityEngine;

public class WoodLog : MonoBehaviour, IObstacle
{
    [SerializeField]
    private ParticleSystem _bloodParticles;
    
    [SerializeField]
    private BloodZone _bloodZone;
    public BloodZone BloodZone => _bloodZone;
    
    [SerializeField]
    private float _rotationSpeed = 200f;
    [SerializeField]
    private Rigidbody _rigidbody;
    
    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;
    
    private Quaternion _bloodZoneInitialRotation;

    private void Start()
    {
        _rigidbody.isKinematic = true;
        if (_bloodZone != null)
            _bloodZoneInitialRotation = _bloodZone.transform.rotation;
    }

    private void Update()
    {
        transform.Rotate(Vector3.right, -_rotationSpeed * Time.deltaTime, Space.World);
    }

    private void LateUpdate()
    {
        if (_bloodZone != null)
            _bloodZone.transform.rotation = _bloodZoneInitialRotation;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Горящий лемминг: коллайдер может быть у дочернего объекта (огонь), ищем лемминга в родителях
        var lemmingView = collision.gameObject.GetComponent<LemmingView>();
        if (lemmingView == null) return;

        if ((lemmingView.IsRun || lemmingView.IsOnFire) && !lemmingView.IsInvincible)
        {
            lemmingView.Kill(destroyImmediately: true);
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
        Destroy(_bloodParticles.gameObject, 2f);
        SpawnBlood();
    }
    
    public void SpawnBlood()
    {
        if (_bloodZone != null)
        {
            _bloodZone.SpawnBlood();
        }
    }

    public void MakeSound()
    {
        
    }

    public void OnDestroy()
    {
        OnDestroyed?.Invoke(gameObject);
    }
}
