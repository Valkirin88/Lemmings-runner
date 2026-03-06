using System;
using UnityEngine;
using System.Collections.Generic;

public class Chipper : MonoBehaviour, IObstacle
{
    [Header("Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed = 5f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem _particles;
    
    [SerializeField] private BloodZone _bloodZone;
   
    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;
    
    
    public BloodZone BloodZone => _bloodZone;
    
    private List<LemmingView> _caughtLemmings = new List<LemmingView>();

    private void Start()
    {
        transform.rotation *= Quaternion.Euler(-90f, 180f, 0f);
    }

    private void Update()
    {
        for (int i = _caughtLemmings.Count - 1; i >= 0; i--)
        {
            var lemming = _caughtLemmings[i];
            
            if (lemming == null)
            {
                _caughtLemmings.RemoveAt(i);
                continue;
            }
            
            // Перемещаем лемминга к цели
            Vector3 direction = (_target.position - lemming.transform.position).normalized;
            float distance = Vector3.Distance(lemming.transform.position, _target.position);
            
            if (distance > 0.1f)
            {
                lemming.transform.position += direction * _speed * Time.deltaTime;
                // Сохраняем направление вперёд, чтобы LemmingView не разворачивал
                lemming.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            }
            else
            {
                _particles.Play();
                SpawnBlood();
                lemming.Kill(destroyImmediately: true);
                _caughtLemmings.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var lemming = other.GetComponentInParent<LemmingView>();
        if (lemming != null)
        {
            if ((lemming.IsRun || lemming.IsOnFire) && !lemming.IsDead && !_caughtLemmings.Contains(lemming) && !lemming.IsInvincible)
            {
                // Отключаем стандартное движение лемминга
                lemming.IsRun = false;
                lemming.IsScroll = false;
                lemming.RunningPlace = null;
                
                // Останавливаем физику
                if (lemming.Rigidbody != null)
                {
                    lemming.Rigidbody.linearVelocity = Vector3.zero;
                    lemming.Rigidbody.isKinematic = true;
                }
                
                _caughtLemmings.Add(lemming);
            }
        }
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
