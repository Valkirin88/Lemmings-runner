using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTrap : MonoBehaviour, IObstacle
{
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _pushForceUp = 12f;
    [SerializeField] private float _pushForceForward = 6f;
    [SerializeField] private float _pushForceSide = 4f;
    [SerializeField] private float _tumbleSpeed = 6f;
    [SerializeField] private Animator _animator;

    private bool _isActivated;
    private bool _collecting;
    private readonly List<LemmingView> _lemmingsToPush = new List<LemmingView>();

    private void OnTriggerEnter(Collider other)
    {
        var lemmingView = other.GetComponent<LemmingView>() ?? other.GetComponentInParent<LemmingView>();
        if (lemmingView == null || !lemmingView.IsRun || lemmingView.IsDead) return;

        if (!_isActivated)
        {
            _isActivated = true;
            _collecting = true;
            
            _lemmingsToPush.Add(lemmingView);
            StartCoroutine(WaitAndPush());
        }
        else if (_collecting && !_lemmingsToPush.Contains(lemmingView))
        {
            _lemmingsToPush.Add(lemmingView);
        }
    }

    private IEnumerator WaitAndPush()
    {
        yield return new WaitForSeconds(_delay);
        _collecting = false;

        _animator.SetTrigger("Activate");
        foreach (var lemming in _lemmingsToPush)
        {
            if (lemming == null || lemming.IsDead) continue;
            lemming.RunningPlace = null;
            lemming.IsPushed = true;
            if (lemming.Animator != null)
                lemming.Animator.applyRootMotion = false;
            lemming.Rigidbody.constraints &= ~(RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ);
            float side = UnityEngine.Random.Range(-1f, 1f);
            Vector3 velocity = Vector3.up * _pushForceUp
                + Vector3.forward * _pushForceForward
                + Vector3.right * (side * _pushForceSide);
            lemming.Rigidbody.linearVelocity = velocity;
            lemming.Rigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * _tumbleSpeed;
        }
        _lemmingsToPush.Clear();
    }

    public BloodZone BloodZone { get; }
    public void SpawnBlood() { }
    public void MakeSound() { }
    public void OnDestroy() { }
}
