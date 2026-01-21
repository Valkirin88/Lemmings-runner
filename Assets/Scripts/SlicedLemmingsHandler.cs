using UnityEngine;

public class SlicedLemmingsHandler 
{
    private const float DESTROY_DELAY = 2f;
    
    private GameObject _gameObject1;
    private GameObject _gameObject2;
    private Rigidbody _rigidbody1;
    private Rigidbody _rigidbody2;
    private ParticleSystem _bloodParticlesPrefab;
    
    

    public void HandleSlicedLemmings(GameObject gameObject1, GameObject gameObject2, ParticleSystem bloodParticles)
    {
        _gameObject1 = gameObject1;
        _gameObject2 = gameObject2;
        _bloodParticlesPrefab = bloodParticles;
        
        AddCapsuleColliders();
        AddRigidbodies();
        AddBloodParticles();
        AdjustRigidboies();
        DestroyAfterDelay();
    }
    
    private void AddBloodParticles()
    {
        // Создаём копии частиц для каждой части
        ParticleSystem blood1 = Object.Instantiate(_bloodParticlesPrefab, _gameObject1.transform);
        ParticleSystem blood2 = Object.Instantiate(_bloodParticlesPrefab, _gameObject2.transform);
        
        // Устанавливаем локальную позицию в центр объекта
        blood1.transform.localPosition = Vector3.zero;
        blood2.transform.localPosition = Vector3.zero;
        
        // Включаем loop
        var main1 = blood1.main;
        main1.loop = true;
        
        var main2 = blood2.main;
        main2.loop = true;
        
        // Запускаем частицы
        blood1.Play();
        blood2.Play();
    }
    
    private void DestroyAfterDelay()
    {
        Object.Destroy(_gameObject1, DESTROY_DELAY);
        Object.Destroy(_gameObject2, DESTROY_DELAY);
    }

    private void AddCapsuleColliders()
    {
        CapsuleCollider capsuleCollider = _gameObject1.AddComponent<CapsuleCollider>();
        CapsuleCollider capsuleCollider2 = _gameObject2.AddComponent<CapsuleCollider>();
        _gameObject1.layer = LayerMask.NameToLayer("Lemming");
        _gameObject2.layer = LayerMask.NameToLayer("Lemming");
    }

    private void AddRigidbodies()
    {
        _rigidbody1 = _gameObject1.AddComponent<Rigidbody>();
        _rigidbody2 = _gameObject2.AddComponent<Rigidbody>();
    }

    private void AdjustRigidboies()
    {
        _rigidbody1.isKinematic = false;
        _rigidbody2.isKinematic = false;
        _rigidbody1.mass = 1;
        _rigidbody2.mass = 1;
        // Разбрасываем части в стороны (по X), вверх (Y) и вперёд (Z)
        _rigidbody1.AddForce(new Vector3(2, 2, 5), ForceMode.Impulse);
        _rigidbody2.AddForce(new Vector3(-2, 2, 5), ForceMode.Impulse);
    }
}
