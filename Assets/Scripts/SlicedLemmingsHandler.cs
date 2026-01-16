using UnityEngine;

public class SlicedLemmingsHandler 
{
    private GameObject _gameObject1;
    private GameObject _gameObject2;
    private Rigidbody _rigidbody1;
    private Rigidbody _rigidbody2;
    private LemmingView _lemming1;
    private LemmingView _lemming2;

    public void HandleSlicedLemmings(GameObject gameObject1, GameObject gameObject2)
    {
        _gameObject1 = gameObject1;
        _gameObject2 = gameObject2;
        AddCapsuleColliders();
        AddRigidbodies();
        AddLogComponent();
        AdjustLogs();
        AdjustRigidboies();
    }

    private void AddCapsuleColliders()
    {
        CapsuleCollider capsuleCollider = _gameObject1.AddComponent<CapsuleCollider>();
        CapsuleCollider capsuleCollider2 = _gameObject2.AddComponent<CapsuleCollider>();
    }

    private void AddRigidbodies()
    {
        _rigidbody1 = _gameObject1.AddComponent<Rigidbody>();
        _rigidbody2 = _gameObject2.AddComponent<Rigidbody>();
    }

    private void AddLogComponent()
    {
        _lemming1 = _gameObject1.AddComponent<LemmingView>();
        _lemming2 = _gameObject2.AddComponent<LemmingView>();
        // _lemming1.IsDead = true;
        // _lemming2.IsDead = true;
    }

    private void AdjustLogs()
    {
        _lemming1.Rigidbody = _rigidbody1;
        _lemming2.Rigidbody = _rigidbody2;
    }

    private void AdjustRigidboies()
    {
        _rigidbody1.isKinematic = false;
        _rigidbody2.isKinematic = false;
        _rigidbody1.mass = 10;
        _rigidbody2.mass = 10;
        _rigidbody1.AddForce(new Vector3(3000, 0, 40), ForceMode.Impulse);
        _rigidbody2.AddForce(new Vector3(3000, 0, -40), ForceMode.Impulse);
    }
}
