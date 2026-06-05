using UnityEngine;

public class SlicedLemmingsHandler 
{
    private const float DESTROY_DELAY = 5f;
    private const float SLICED_DYNAMIC_FRICTION = 0.06f;
    
    private GameObject _gameObject1;
    private GameObject _gameObject2;
    private Rigidbody _rigidbody1;
    private Rigidbody _rigidbody2;
    private ParticleSystem _bloodParticlesPrefab;
    private PhysicsMaterial _slicedPhysicsMaterial;
    
    

    public void HandleSlicedLemmings(GameObject gameObject1, GameObject gameObject2, ParticleSystem bloodParticles, PhysicsMaterial sourceMaterial = null)
    {
        _gameObject1 = gameObject1;
        _gameObject2 = gameObject2;
        _bloodParticlesPrefab = bloodParticles;
        _slicedPhysicsMaterial = CreateSlicedMaterial(sourceMaterial);
        
        AddCapsuleColliders();
        AddRigidbodies();
        AddBloodParticles();
        AdjustRigidboies();
        DestroyAfterDelay();
    }

    /// <summary>
    /// Клонирует физматериал родительского (разрубленного) лемминга и ставит dynamicFriction = 0.06.
    /// Клон, чтобы не менять общий asset для всех объектов с этим материалом.
    /// </summary>
    private PhysicsMaterial CreateSlicedMaterial(PhysicsMaterial source)
    {
        if (source == null)
            return null;

        return new PhysicsMaterial(source.name + " (Sliced)")
        {
            dynamicFriction = SLICED_DYNAMIC_FRICTION,
            staticFriction = source.staticFriction,
            bounciness = source.bounciness,
            frictionCombine = source.frictionCombine,
            bounceCombine = source.bounceCombine,
        };
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

        if (_slicedPhysicsMaterial != null)
        {
            capsuleCollider.sharedMaterial = _slicedPhysicsMaterial;
            capsuleCollider2.sharedMaterial = _slicedPhysicsMaterial;
        }
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
        _rigidbody1.AddForce(new Vector3(2, 4, 8), ForceMode.Impulse);
        _rigidbody2.AddForce(new Vector3(-2, 4, 8), ForceMode.Impulse);
    }
}
