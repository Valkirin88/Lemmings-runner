using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;

    private LemmingsEventsHandler _lemmingsEventsHandler;
    private GameStatesUIMediator _gameStatesUIMediator;
    private LemmingPlaceView _lemmingPlaceView;
    private Coroutine _repositionCoroutine;

    public void Initialize(GameStatesUIMediator gameStatesUIMediator, LemmingPlaceView lemmingPlaceView = null)
    {
        _gameStatesUIMediator = gameStatesUIMediator;
        _lemmingPlaceView = lemmingPlaceView;
        _lemmingsEventsHandler = _gameStatesUIMediator.LemmingsEventsHandler;

        _lemmingsEventsHandler.OnLemmingCountAdd += PlaceNewLemmingState;
        _lemmingsEventsHandler.OnLemmingCountRemove += ReplaceLemmingsState;

        _gameStatesUIMediator.EndTrack.OnFinished += StopLemmings;
        
        // Назначаем место первому леммингу (он уже в списке до подписки на событие)
        if (_lemmingsEventsHandler.RunningLemmingViews.Count > 0)
        {
            var leader = _lemmingsEventsHandler.RunningLemmingViews[0];
            SetNewPosition(leader);
        }
    }

    private void ReplaceLemmingsState(LemmingView lemmingView, int removedIndex)
    {
        if (_repositionCoroutine != null)
            StopCoroutine(_repositionCoroutine);
        _repositionCoroutine = StartCoroutine(DelayedRepositionAll());
    }

    private IEnumerator DelayedRepositionAll()
    {
        yield return new WaitForSeconds(1f);
        _repositionCoroutine = null;

        var views = _lemmingsEventsHandler.RunningLemmingViews;
        for (int p = 0; p < _lemmingPlaces.Count; p++)
            _lemmingPlaces[p].IsEmpty = true;

        int placeIdx = 0;
        foreach (var view in views)
        {
            if (view == null || placeIdx >= _lemmingPlaces.Count) break;
            if (view.IsOnFire) continue;
            var place = _lemmingPlaces[placeIdx];
            place.IsEmpty = false;
            view.RunningPlace = place.transform;
            placeIdx++;
        }
    }

    private void PlaceNewLemmingState(LemmingView lemmingView)
    {
        SetNewPosition(lemmingView);
    }

    private int GetFormationIndex(LemmingView lemmingView)
    {
        var views = _lemmingsEventsHandler.RunningLemmingViews;
        int formationIdx = 0;
        for (int i = 0; i < views.Count; i++)
        {
            if (views[i] == lemmingView) return formationIdx;
            if (!views[i].IsOnFire) formationIdx++;
        }
        return -1;
    }

    private void SetNewPosition(LemmingView lemmingView)
    {
        if (lemmingView.IsOnFire) return;
        int index = GetFormationIndex(lemmingView);
        if (index < 0 || index >= _lemmingPlaces.Count) return;

        RunPlace place = _lemmingPlaces[index];
        place.IsEmpty = false;
        lemmingView.RunningPlace = place.transform;
    }

    private void StopLemmings()
    {
        foreach (var lemmingView in _lemmingsEventsHandler.RunningLemmingViews)
        {
            lemmingView.IsRun = false;
        }
        if (_lemmingPlaceView != null)
            _lemmingPlaceView.IsMoving = false;
    }

    private void OnDestroy()
    {
        if (_repositionCoroutine != null)
            StopCoroutine(_repositionCoroutine);
        _lemmingsEventsHandler.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsEventsHandler.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStatesUIMediator.EndTrack.OnFinished -= StopLemmings;
    }
}
