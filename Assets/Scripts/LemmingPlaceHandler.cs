using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;
    
    private LemmingsStateSet _lemmingsStateSet;
    private GameStateCollector _gameStateCollector;
    
    public void Initialize(GameStateCollector gameStateCollector)
    {
        _gameStateCollector = gameStateCollector;
        _lemmingsStateSet = _gameStateCollector.LemmingsStateSet;

        _lemmingsStateSet.OnLemmingCountAdd += PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove += ReplaceLemmingsState;

        _gameStateCollector.EndTrack.OnFinished += StopLemmings;
        
        // Назначаем место первому леммингу (он уже в списке до подписки на событие)
        if (_lemmingsStateSet.RunningLemmingViews.Count > 0)
        {
            SetNewPosition(_lemmingsStateSet.RunningLemmingViews[0]);
        }
    }

    private void ReplaceLemmingsState(LemmingView lemmingView)
    {
        // Релокация всех леммингов через 1 секунду
        StartCoroutine(DelayedReposition());
    }

    private IEnumerator DelayedReposition()
    {
        yield return new WaitForSeconds(1f);
        
        // Освобождаем все места
        foreach (var place in _lemmingPlaces)
        {
            place.IsEmpty = true;
        }
        
        // Назначаем новые места всем живым леммингам
        foreach (var view in _lemmingsStateSet.RunningLemmingViews)
        {
            SetNewPosition(view);
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
