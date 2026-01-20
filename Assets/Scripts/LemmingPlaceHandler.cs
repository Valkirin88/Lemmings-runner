using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;
    
    private RunningLemmingsSet _runningLemmingsSet;
    private LemmingController _lemmingController;
    private GameStateCollector _gameStateCollector;
    
    
    private GameObject _leaderObject;
    
    private bool _isLeaderKilled = false;
    
    public void Initialize(LemmingController lemmingController, GameStateCollector gameStateCollector)
    {
        _lemmingController = lemmingController;
        _gameStateCollector = gameStateCollector;
        _runningLemmingsSet = _gameStateCollector.RunningLemmingsSet;

        _runningLemmingsSet.OnLemmingCountAdd += PlaceNewLemming;
        _runningLemmingsSet.OnLemmingCountRemove += ReplaceLemmings;

        _gameStateCollector.EndTrack.OnFinished += StopLemmings;

        _leaderObject = _runningLemmingsSet.RunningLemmingViews[0].gameObject;
        
        // Резервируем первое место для лидера
        _lemmingPlaces[0].IsEmpty = false;
    }

    private void Update()
    {
        transform.position = _leaderObject.transform.position;
    }

    private void ReplaceLemmings(LemmingView lemmingView)
    {
        if (lemmingView.IsLeader)
        {
            _isLeaderKilled = true;
        }
        
        foreach (var place in _lemmingPlaces)
        {
            place.IsEmpty = true;
        }
        
        // Резервируем первое место для лидера
        _lemmingPlaces[0].IsEmpty = false;
        
        foreach (var view in _runningLemmingsSet.RunningLemmingViews)
        {
            view.RunningPlace = null;
            
            if (_isLeaderKilled)
            {
                view.IsLeader = true;
                _leaderObject = view.gameObject;
                _lemmingController.View = view;
                _isLeaderKilled = false;
                // Лидер не должен иметь RunningPlace
            }
            else if (!view.IsLeader)
            {
                SetNewPosition(view);
            }
        }
    }

    private void PlaceNewLemming(LemmingView lemmingView)
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
        foreach (var lemmingView in _runningLemmingsSet.RunningLemmingViews)
        {
            Debug.Log("StopLemmings   ");
            lemmingView.IsRun = false;
        }
    }

    private void OnDestroy()
    {
        _runningLemmingsSet.OnLemmingCountAdd -= PlaceNewLemming;
        _runningLemmingsSet.OnLemmingCountRemove -= ReplaceLemmings;
        
        _gameStateCollector.EndTrack.OnFinished -= StopLemmings;
    }
}
