using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;
    
    private LemmingsStateSet _lemmingsStateSet;
    private LemmingController _lemmingController;
    private GameStateCollector _gameStateCollector;
    
    
    private GameObject _leaderObject;
    
    private bool _isLeaderKilled = false;
    
    public void Initialize(LemmingController lemmingController, GameStateCollector gameStateCollector)
    {
        _lemmingController = lemmingController;
        _gameStateCollector = gameStateCollector;
        _lemmingsStateSet = _gameStateCollector.LemmingsStateSet;

        _lemmingsStateSet.OnLemmingCountAdd += PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove += ReplaceLemmingsState;

        _gameStateCollector.EndTrack.OnFinished += StopLemmings;

        _leaderObject = _lemmingsStateSet.RunningLemmingViews[0].gameObject;
        
        // Резервируем первое место для лидера
        _lemmingPlaces[0].IsEmpty = false;
    }

    private void Update()
    {
        if (_leaderObject != null)
        {
            transform.position = _leaderObject.transform.position;
        }
    }

    private void ReplaceLemmingsState(LemmingView lemmingView)
    {
        if (lemmingView.IsLeader)
        {
            _isLeaderKilled = true;
        }
        
        // Назначаем нового лидера сразу
        if (_isLeaderKilled)
        {
            foreach (var view in _lemmingsStateSet.RunningLemmingViews)
            {
                view.IsLeader = true;
                _leaderObject = view.gameObject;
                _lemmingController.View = view;
                _isLeaderKilled = false;
                break; // Только первый становится лидером
            }
        }
        
        // Репозиция остальных через 1 секунду
        StartCoroutine(DelayedReposition());
    }

    private IEnumerator DelayedReposition()
    {
        yield return new WaitForSeconds(1f);
        
        foreach (var place in _lemmingPlaces)
        {
            place.IsEmpty = true;
        }
        
        // Резервируем первое место для лидера
        _lemmingPlaces[0].IsEmpty = false;
        
        foreach (var view in _lemmingsStateSet.RunningLemmingViews)
        {
            view.RunningPlace = null;
            
            if (!view.IsLeader)
            {
                SetNewPosition(view);
            }
        }
    }

    private void PlaceNewLemmingState(LemmingView lemmingView)
    {
        SetNewPosition(lemmingView);
    }

    private void SetNewPosition(LemmingView lemmingView)
    {
        foreach (RunPlace place in _lemmingPlaces)
        {
            if (place.IsEmpty && !lemmingView.IsOnFire)
            {
                place.IsEmpty = false;
                lemmingView.RunningPlace = place.transform;
                return;
            }
        }
    }

    private void StopLemmings()
    {
        foreach (var lemmingView in _lemmingsStateSet.RunningLemmingViews)
        {
            Debug.Log("StopLemmings   ");
            lemmingView.IsRun = false;
        }
    }

    private void OnDestroy()
    {
        _lemmingsStateSet.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStateCollector.EndTrack.OnFinished -= StopLemmings;
    }
}
