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
        
        var hearts = Instantiate(_heartsParticles,transform);
        hearts.gameObject.SetActive(true);
 
        Destroy(hearts.gameObject, 5f);
    }

    private void OnDestroy()
    {
        if (_increaseLemmingsNumber != null)
            _increaseLemmingsNumber.OnActivated -= ShowHearts;
    }
}
