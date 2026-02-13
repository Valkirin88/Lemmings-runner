using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;

    private LemmingsStateSet _lemmingsStateSet;
    private GameStateCollector _gameStateCollector;
    private LemmingPlaceView _lemmingPlaceView;
    private Coroutine _repositionCoroutine;

    public void Initialize(GameStateCollector gameStateCollector, LemmingPlaceView lemmingPlaceView = null)
    {
        _gameStateCollector = gameStateCollector;
        _lemmingPlaceView = lemmingPlaceView;
        _lemmingsStateSet = _gameStateCollector.LemmingsStateSet;

        _lemmingsStateSet.OnLemmingCountAdd += PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove += ReplaceLemmingsState;

        _gameStateCollector.EndTrack.OnFinished += StopLemmings;
        
        // Назначаем место первому леммингу (он уже в списке до подписки на событие)
        if (_lemmingsStateSet.RunningLemmingViews.Count > 0)
        {
            var leader = _lemmingsStateSet.RunningLemmingViews[0];
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

        var views = _lemmingsStateSet.RunningLemmingViews;
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
        var views = _lemmingsStateSet.RunningLemmingViews;
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
        foreach (var lemmingView in _lemmingsStateSet.RunningLemmingViews)
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
        _lemmingsStateSet.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStateCollector.EndTrack.OnFinished -= StopLemmings;
    }
}
