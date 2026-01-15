using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private  List<RunPlace> _lemmingPlaces;
    
    private  RunningLemmingsSet _runningLemmingsSet;

    private GameObject _leaderObject;
    
    private bool _isLeaderKilled = false;
    
    public void Initialize(RunningLemmingsSet runningLemmingsSet)
    {
        _runningLemmingsSet = runningLemmingsSet;

        _runningLemmingsSet.OnLemmingCountAdd += PlaceNewLemming;
        _runningLemmingsSet.OnLemmingCountRemove += ReplaceLemmings;

        _leaderObject = _runningLemmingsSet.RunningLemmingViews[0].gameObject;
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
        foreach (var view in  _runningLemmingsSet.RunningLemmingViews)
        {
            SetNewPosition(view);
            if (_isLeaderKilled)
            {
                view.IsLeader = true;
                _leaderObject = view.gameObject;
                _isLeaderKilled = false;
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
            if (place.IsEmpty)
            {
                place.IsEmpty = false;
                lemmingView.RunningPlace = place.transform;
                return;
            }
        }
    }

    public void OnDestroy()
    {
        _runningLemmingsSet.OnLemmingCountAdd -= PlaceNewLemming;
        _runningLemmingsSet.OnLemmingCountRemove -= ReplaceLemmings;
    }
}
