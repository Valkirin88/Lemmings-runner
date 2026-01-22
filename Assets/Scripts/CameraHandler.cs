using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Follow Speed")]
    [SerializeField, Tooltip("Скорость следования по осям X и Y (при смене лидера)")]
    private float _followSpeedXY = 3f;
    
    private LemmingsStateSet _lemmingsStateSet;
    private Transform _currentLeader;
    private Vector3 _offset;
    private float _currentX;
    private float _currentY;
    private bool _isInitialized = false;
    
    public void Initialize(LemmingsStateSet lemmingsStateSet)
    {
        _lemmingsStateSet = lemmingsStateSet;
        
        // Устанавливаем начального лидера
        UpdateCurrentLeader();
        
        // Запоминаем начальное смещение камеры относительно лидера
        if (_currentLeader != null)
        {
            _offset = transform.position - _currentLeader.position;
            _currentX = transform.position.x;
            _currentY = transform.position.y;
        }
        
        _isInitialized = true;
    }
    
    private void LateUpdate()
    {
        if (!_isInitialized)
            return;
        
        UpdateCurrentLeader();
        
        if (_currentLeader == null)
            return;
        
        Vector3 leaderPos = _currentLeader.position;
        
        // По Z следуем точно (без сглаживания) — никакого дребезга
        float newZ = leaderPos.z + _offset.z;
        
        // По X и Y плавно переходим к новой позиции (для смены лидера)
        float targetX = leaderPos.x + _offset.x;
        float targetY = leaderPos.y + _offset.y;
        
        _currentX = Mathf.Lerp(_currentX, targetX, _followSpeedXY * Time.deltaTime);
        _currentY = Mathf.Lerp(_currentY, targetY, _followSpeedXY * Time.deltaTime);
        
        transform.position = new Vector3(_currentX, _currentY, newZ);
    }
    
    private void UpdateCurrentLeader()
    {
        if (_lemmingsStateSet.RunningLemmingViews.Count > 0)
        {
            _currentLeader = _lemmingsStateSet.RunningLemmingViews[0].transform;
        }
        else
        {
            _currentLeader = null;
        }
    }
    
    private void OnDestroy()
    {
        // Ничего не нужно очищать
    }
}
