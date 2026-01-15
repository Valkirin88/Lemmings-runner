using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Unity.VisualScripting;

public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private  List<RunPlace> _lemmingPlaces;
    
    private  RunningLemmingsSet _runningLemmingsSet;
    
    
    private bool _isLeaderKilled = false;
    
    public void Initialize(RunningLemmingsSet runningLemmingsSet)
    {
        _runningLemmingsSet = runningLemmingsSet;

        _runningLemmingsSet.OnLemmingCountAdd += PlaceNewLemming;
        _runningLemmingsSet.OnLemmingCountRemove += ReplaceLemmings;
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
                lemmingView.transform.position = place.transform.position;
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
