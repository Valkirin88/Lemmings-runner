using System;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

public class DestroyAllObstacles : IAbility
{
    private const float ShrinkDuration = 0.25f;
    private const float SoundVolume = 0.55f;

    public event Action OnDeactivated;
    
    private readonly ObstaclesSet _obstaclesSet;
    private readonly AudioClip _destroyAllSound;
    
    public DestroyAllObstacles(ObstaclesSet obstaclesSet, AudioClip destroyAllSound)
    {
        _obstaclesSet = obstaclesSet;
        _destroyAllSound = destroyAllSound;
    }

    public void Activate()
    {
        PlayDestroyAllSound(_destroyAllSound);

        foreach (var obsacle in _obstaclesSet.Obstacles)
        {
            if (obsacle == null) continue;
            if (obsacle.GetComponentInChildren<IObstacle>() == null) continue;

            // Отключаем взаимодействие, пока объект "схлопывается".
            var colliders = obsacle.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            var rigidbodies = obsacle.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rigidbodies)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            obsacle.transform.DOScale(Vector3.zero, ShrinkDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => Object.Destroy(obsacle));
        }
    }

    public void Update()
    {
        
    }

    public void Deactivate()
    {
        OnDeactivated?.Invoke();
    }

    private static void PlayDestroyAllSound(AudioClip clip)
    {
        var listener = Object.FindFirstObjectByType<AudioListener>();
        if (listener == null || clip == null) return;

        AudioSource.PlayClipAtPoint(clip, listener.transform.position, SoundVolume);
    }
}
