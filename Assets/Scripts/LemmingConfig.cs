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

    public float SideSpeed => _sideSpeed;

    public float ForwardSpeed => _forwardSpeed;

    public float FollowSpeed => _followSpeed;

    public float StickDistance => _stickDistance;

    public float StickSmoothing => _stickSmoothing;
}
