using System;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour, IObstacle
{
    public event Action<AudioClip> OnMadeSound;
    public event Action<GameObject> OnDestroyed;
    
    [Header("Movement")]
    [SerializeField] private Transform _firstEdge;
    [SerializeField] private Transform _secondEdge;
    [SerializeField] private float _moveSpeed = 1f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 360f;
    [SerializeField] private Vector3 _spinAxis = Vector3.forward;

    [Header("Catch")]
    [SerializeField] private Collider _catchCollider;
    [SerializeField] private float _deathDelay = 2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private BloodZone _bloodZone;

    public BloodZone BloodZone => _bloodZone;

    private readonly List<CaughtLemming> _caughtLemmings = new List<CaughtLemming>();

    private void Awake()
    {
        if (_bloodZone == null)
            _bloodZone = GetComponentInChildren<BloodZone>();
    }

    private struct CaughtLemming
    {
        public LemmingView View;
        public Vector3 LocalStickOffset;
        public Quaternion LocalStickRotation;
        public float DeathTimer;
    }

    private void Update()
    {
        MoveDrill();
        RotateDrill();
        UpdateCaughtLemmings();
    }

    private void MoveDrill()
    {
        if (_firstEdge == null || _secondEdge == null) return;

        float t = Mathf.PingPong(Time.time * _moveSpeed, 1f);
        transform.position = Vector3.Lerp(_firstEdge.position, _secondEdge.position, t);
    }

    private void RotateDrill()
    {
        transform.Rotate(_spinAxis.normalized, _rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void UpdateCaughtLemmings()
    {
        for (int i = _caughtLemmings.Count - 1; i >= 0; i--)
        {
            var state = _caughtLemmings[i];
            var lemming = state.View;

            if (lemming == null)
            {
                _caughtLemmings.RemoveAt(i);
                continue;
            }

            lemming.transform.localPosition = state.LocalStickOffset;
            lemming.transform.localRotation = state.LocalStickRotation;

            if (lemming.IsDead)
            {
                _caughtLemmings.RemoveAt(i);
                continue;
            }

            // Горящий лемминг умирает от огня (KillFromFireAfterDelay), не от сверла
            if (lemming.IsOnFire)
                continue;

            state.DeathTimer += Time.deltaTime;
            _caughtLemmings[i] = state;

            if (state.DeathTimer >= _deathDelay)
            {
                KillLemming(lemming);
                _caughtLemmings.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var lemming = other.GetComponentInParent<LemmingView>();
        if (lemming == null) return;

        if (!CanCatch(lemming)) return;
        if (IsAlreadyCaught(lemming)) return;

        CatchLemming(lemming, other);
    }

    private bool CanCatch(LemmingView lemming)
    {
        return (lemming.IsRun || lemming.IsOnFire)
               && !lemming.IsDead
               && !lemming.IsInvincible;
    }

    private bool IsAlreadyCaught(LemmingView lemming)
    {
        for (int i = 0; i < _caughtLemmings.Count; i++)
        {
            if (_caughtLemmings[i].View == lemming)
                return true;
        }
        return false;
    }

    private void CatchLemming(LemmingView lemming, Collider lemmingCollider)
    {
        StopLemmingMovement(lemming);

        Vector3 stickWorld = GetColliderContactPoint(lemmingCollider);
        lemming.transform.SetParent(transform, true);
        Vector3 localOffset = transform.InverseTransformPoint(stickWorld);

        if (!lemming.IsOnFire)
            lemming.ReportDanger();

        lemming.transform.localPosition = localOffset;
        Quaternion localRotation = lemming.transform.localRotation;

        PlayCatchEffects(lemming.transform.position);

        _caughtLemmings.Add(new CaughtLemming
        {
            View = lemming,
            LocalStickOffset = localOffset,
            LocalStickRotation = localRotation,
            DeathTimer = 0f
        });
    }

    private static void StopLemmingMovement(LemmingView lemming)
    {
        lemming.IsRun = false;
        lemming.IsScroll = false;
        lemming.IsPushed = false;
        lemming.RunningPlace = null;
        LemmingPlaceHandler.RepositionFormationIfActive();

        if (lemming.Animator != null)
            lemming.Animator.SetBool("IsRun", false);

        if (lemming.Rigidbody != null)
        {
            lemming.Rigidbody.linearVelocity = Vector3.zero;
            lemming.Rigidbody.angularVelocity = Vector3.zero;
            lemming.Rigidbody.isKinematic = true;
        }
    }

    private Vector3 GetColliderContactPoint(Collider lemmingCollider)
    {
        Collider drillCol = _catchCollider != null ? _catchCollider : GetComponent<Collider>();
        if (drillCol == null || lemmingCollider == null)
            return transform.position;

        Vector3 pointOnDrill = drillCol.ClosestPoint(lemmingCollider.bounds.center);
        Vector3 pointOnLemming = lemmingCollider.ClosestPoint(pointOnDrill);
        pointOnDrill = drillCol.ClosestPoint(pointOnLemming);
        return (pointOnDrill + pointOnLemming) * 0.5f;
    }

    private void KillLemming(LemmingView lemming)
    {
        if (lemming == null) return;

        if (lemming.transform.parent == transform)
            lemming.transform.SetParent(null, true);

        lemming.KillWithotBlood();
    }

    private void PlayCatchEffects(Vector3 worldPosition)
    {
        if (_particles != null)
        {
            _particles.transform.position = worldPosition;
            _particles.Play();
        }

        SpawnBloodAt(worldPosition);
    }

    public void SpawnBlood()
    {
        SpawnBloodAt(transform.position);
    }

    private void SpawnBloodAt(Vector3 worldPosition)
    {
        if (_bloodZone != null)
            _bloodZone.SpawnBloodAt(worldPosition);
    }

    public void MakeSound()
    {
    }

    public void OnDestroy()
    {
        for (int i = 0; i < _caughtLemmings.Count; i++)
        {
            var lemming = _caughtLemmings[i].View;
            if (lemming == null || lemming.IsDead) continue;

            if (lemming.transform.parent == transform)
                lemming.transform.SetParent(null, true);

            if (!lemming.IsOnFire)
                KillLemming(lemming);
        }
        _caughtLemmings.Clear();

        OnDestroyed?.Invoke(gameObject);
    }
}
