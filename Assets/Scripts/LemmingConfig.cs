using UnityEngine;

[CreateAssetMenu(fileName = "Lemming", menuName = "ScriptableObjects/Create lemming")]
public class LemmingConfig : ScriptableObject
{
    [SerializeField]
    private float _sideSpeed;
    [SerializeField]
    private float _forwardSpeed;
    [SerializeField]
    private float _followSpeed;
    [SerializeField]
    private float _stickDistance;
    [SerializeField]
    private float _stickSmoothing;
    [SerializeField]
    private float _onFireSpeed;
    [SerializeField]
    private float _jumpForce = 8f;
    [SerializeField]
    private float _accelerateDuration = 0.5f;
    [SerializeField]
    private float _accelerateMultiplier = 2f;
  
    public float SideSpeed => _sideSpeed;
    public float ForwardSpeed => _forwardSpeed;
    public float FollowSpeed => _followSpeed;
    public float StickDistance => _stickDistance;
    public float StickSmoothing => _stickSmoothing;
    public float OnFireSpeed => _onFireSpeed;
    public float JumpForce => _jumpForce;
    public float AccelerateDuration => _accelerateDuration;
    public float AccelerateMultiplier => _accelerateMultiplier;
}
