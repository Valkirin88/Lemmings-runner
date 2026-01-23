using System.Collections;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("Leader Switch Settings")]
    [SerializeField, Tooltip("Задержка перед началом перехода к новому лидеру")]
    private float _transitionDelay = 1f;
    
    [SerializeField, Tooltip("Скорость плавного перехода к новому лидеру по X и Y")]
    private float _leaderTransitionSpeed = 3f;
    
    private LemmingsStateSet _lemmingsStateSet;
    private Transform _currentLeader;
    private Vector3 _offset;
    
    // Для плавного перехода при смене лидера
    private float _currentX;
    private float _currentY;
    private bool _isTransitioning = false;
    private bool _isWaitingForTransition = false;
    
    private bool _isInitialized = false;
    
    public void Initialize(LemmingsStateSet lemmingsStateSet)
    {
        _lemmingsStateSet = lemmingsStateSet;
        
        // Подписываемся на смену лидера
        _lemmingsStateSet.OnLemmingCountRemove += OnLemmingRemoved;
        
        // Устанавливаем начального лидера
        UpdateCurrentLeader();
        
        // Запоминаем начальное смещение камеры относительно лидера
        if (_currentLeader != null)
        {
            _offset = transform.position - _currentLeader.position;
        }
        
        _currentX = transform.position.x;
        _currentY = transform.position.y;
        
        _isInitialized = true;
    }
    
    private void LateUpdate()
    {
        if (!_isInitialized || _currentLeader == null)
            return;
        
        Vector3 leaderPos = _currentLeader.position;
        
        // Целевая позиция камеры (точное следование)
        float targetX = leaderPos.x + _offset.x;
        float targetY = leaderPos.y + _offset.y;
        float targetZ = leaderPos.z + _offset.z;
        
        if (_isWaitingForTransition)
        {
            // Ждём — камера стоит на месте по X и Y
            // _currentX и _currentY не меняются
        }
        else if (_isTransitioning)
        {
            // Плавно двигаем X и Y к целевой позиции с фиксированной скоростью
            _currentX = Mathf.MoveTowards(_currentX, targetX, _leaderTransitionSpeed * Time.deltaTime);
            _currentY = Mathf.MoveTowards(_currentY, targetY, _leaderTransitionSpeed * Time.deltaTime);
            
            // Если достигли цели — переход завершён
            if (Mathf.Approximately(_currentX, targetX) && Mathf.Approximately(_currentY, targetY))
            {
                _isTransitioning = false;
            }
        }
        else
        {
            // Точное следование
            _currentX = targetX;
            _currentY = targetY;
        }
        
        transform.position = new Vector3(_currentX, _currentY, targetZ);
    }
    
    private void OnLemmingRemoved(LemmingView removedLemming)
    {
        // Если убит лидер — обновляем на нового
        // if (removedLemming.IsLeader)
        // {
        //     // Сразу обновляем лидера (чтобы камера следовала за новым по Z)
        //     UpdateCurrentLeader();
        //     
        //     // Запускаем переход с задержкой только если ещё не ждём
        //     if (!_isWaitingForTransition)
        //     {
        //         StartCoroutine(DelayedTransition());
        //     }
        //     // Если уже ждём — лидер обновился, корутина продолжит работу с новым лидером
        // }
    }
    
    private IEnumerator DelayedTransition()
    {
        _isWaitingForTransition = true;
        
        // Запоминаем текущую позицию камеры по X и Y
        _currentX = transform.position.x;
        _currentY = transform.position.y;
        
        // Ждём заданное время (камера стоит на месте по X/Y, но следует по Z)
        yield return new WaitForSeconds(_transitionDelay);
        
        // Запоминаем позицию снова (на случай если камера двигалась)
        _currentX = transform.position.x;
        _currentY = transform.position.y;
        
        // Начинаем плавный переход
        if (_currentLeader != null)
        {
            _isTransitioning = true;
        }
        
        _isWaitingForTransition = false;
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
        if (_lemmingsStateSet != null)
        {
            _lemmingsStateSet.OnLemmingCountRemove -= OnLemmingRemoved;
        }
    }
}
