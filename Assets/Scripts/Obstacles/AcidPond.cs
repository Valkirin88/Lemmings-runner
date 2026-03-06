using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class AcidPond : MonoBehaviour, IObstacle
{
    [SerializeField] 
    private ParticleSystem _bloodParticles;
    
    [SerializeField]
    private BloodZone _bloodZone;
    public BloodZone BloodZone => _bloodZone;

    [Header("Acid sphere throw")]
    [SerializeField] private GameObject _acidSpherePrefab;
    [SerializeField] private float _spawnIntervalMin = 2f;
    [SerializeField] private float _spawnIntervalMax = 4f;
    [SerializeField] private float _throwForce = 8f;
    [SerializeField] private float _upwardBias = 1.5f;
    [SerializeField] private Transform _spawnPoint;

    private Coroutine _throwRoutine;
    private ObstaclesSet _obstaclesSet;

    /// <summary> Вызывается при создании пруда (например RandomSpawner) — прокидывает ObstaclesSet из Entry Point. </summary>
    public void SetObstaclesSet(ObstaclesSet set) => _obstaclesSet = set;

    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;

    private void Start()
    {
        if (_acidSpherePrefab != null)
            _throwRoutine = StartCoroutine(ThrowAcidSpheresRoutine());
    }

    private void OnDisable()
    {
        if (_throwRoutine != null)
            StopCoroutine(_throwRoutine);
    }

    private IEnumerator ThrowAcidSpheresRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_spawnIntervalMin, _spawnIntervalMax));
            ThrowAcidSphere();
        }
    }

    private void ThrowAcidSphere()
    {
        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
        GameObject sphere = Instantiate(_acidSpherePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Случайное направление вверх и в стороны (равномерно по горизонтали)
            Vector2 horizontal = Random.insideUnitCircle;
            Vector3 direction = (Vector3.up * _upwardBias + new Vector3(horizontal.x, 0f, horizontal.y)).normalized;
            float scrollSpeed = ScrollSpeedProvider.CurrentSpeed;
            // Сразу задаём и бросок, и смещение в -Z как у всех препятствий
            rb.linearVelocity = direction * _throwForce + new Vector3(0f, 0f, -scrollSpeed);
        }

        if (_obstaclesSet != null)
            _obstaclesSet.Obstacles.Add(sphere);
    }

    private void OnTriggerEnter(Collider collision)
    {
        var lemming = collision.gameObject.GetComponent<LemmingView>();
        if (lemming == null) return;
        if (!lemming.IsRun && !lemming.IsOnFire && lemming.IsInvincible) return;
        _bloodParticles.Play();
        SpawnBlood();
        lemming.Kill(destroyImmediately: true);
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
