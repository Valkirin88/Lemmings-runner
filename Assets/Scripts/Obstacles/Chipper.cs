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

            if (lemming.IsInvincible)
            {
                ReleaseLemming(lemming);
                _caughtLemmings.RemoveAt(i);
                continue;
            }

            if (_target == null)
            {
                // Цель пропала (например при пересборке) — добиваем лемминга, чтобы не завис.
                KillLemming(lemming);
                _caughtLemmings.RemoveAt(i);
                continue;
            }

            // Перемещаем лемминга к цели. Поскольку лемминг теперь является дочкой Chipper,
            // он автоматически едет вместе со скроллом, а здесь мы делаем «дотяжку» к таргету.
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
                KillLemming(lemming);
                _caughtLemmings.RemoveAt(i);
            }
        }
    }

    private void ReleaseLemming(LemmingView lemming)
    {
        if (lemming == null) return;

        if (lemming.transform.parent == transform)
            lemming.transform.SetParent(null, true);

        if (lemming.Rigidbody != null)
            lemming.Rigidbody.isKinematic = false;

        lemming.IsRun = true;
        lemming.IsScroll = false;
        LemmingPlaceHandler.RepositionFormationIfActive();
    }

    private void KillLemming(LemmingView lemming)
    {
        if (lemming == null || lemming.IsInvincible) return;
        // Снимаем с родителя, иначе при Destroy(gameObject) у Chipper-а ребёнок может уйти вместе с ним.
        if (lemming.transform.parent == transform)
        {
            lemming.transform.SetParent(null, true);
        }
        lemming.Kill(destroyImmediately: true);
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
                LemmingPlaceHandler.RepositionFormationIfActive();

                // Останавливаем физику
                if (lemming.Rigidbody != null)
                {
                    lemming.Rigidbody.linearVelocity = Vector3.zero;
                    lemming.Rigidbody.isKinematic = true;
                }

                // Парентим к чипперу — теперь он едет вместе со скроллом, и относительная скорость
                // в Update всегда равна _speed (чиппер не «убегает» от лемминга).
                lemming.transform.SetParent(transform, true);

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
        // Если чиппер уезжает за экран и уничтожается, добиваем всех пойманных леммингов,
        // чтобы они не зависали в воздухе кинематичными «трупами».
        for (int i = 0; i < _caughtLemmings.Count; i++)
        {
            var lemming = _caughtLemmings[i];
            if (lemming != null && !lemming.IsDead)
            {
                KillLemming(lemming);
            }
        }
        _caughtLemmings.Clear();

        OnDestroyed?.Invoke(gameObject);
    }
}
