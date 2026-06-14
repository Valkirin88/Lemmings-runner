using System;
using UnityEngine;

public class AbilitiesFX : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _heartsParticles;

    private IncreaseLemmingsNumber _increaseLemmingsNumber;
    
    public void Initialize(IncreaseLemmingsNumber increaseLemmingsNumber)
    {
        if (_increaseLemmingsNumber != null)
            _increaseLemmingsNumber.OnActivated -= ShowHearts;

        _increaseLemmingsNumber = increaseLemmingsNumber;
        if (_increaseLemmingsNumber != null)
            _increaseLemmingsNumber.OnActivated += ShowHearts;
    }

    private void ShowHearts()
    {
        if (_heartsParticles == null) return;
        var hearts = Instantiate(_heartsParticles, transform.position, transform.rotation, transform);
        var main = hearts.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        hearts.Play();
    }

    private void OnDestroy()
    {
        if (_increaseLemmingsNumber != null)
            _increaseLemmingsNumber.OnActivated -= ShowHearts;
    }
}
