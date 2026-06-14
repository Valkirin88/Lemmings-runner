using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LemmingPlaceHandler : MonoBehaviour
{
    [SerializeField]
    private List<RunPlace> _lemmingPlaces;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Через сколько секунд после смерти лемминга его место займёт следующий")]
    private float _refillDelaySeconds = 1f;

    private LemmingsEventsHandler _lemmingsEventsHandler;
    private GameStatesUIMediator _gameStatesUIMediator;
    private LemmingPlaceView _lemmingPlaceView;
    private static LemmingPlaceHandler _activeInstance;

    public static void RepositionFormationIfActive() => _activeInstance?.ScheduleReposition();

    public void Initialize(GameStatesUIMediator gameStatesUIMediator, LemmingPlaceView lemmingPlaceView = null)
    {
        _activeInstance = this;
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
        ScheduleReposition();
    }

    private void ScheduleReposition()
    {
        if (_refillDelaySeconds <= 0f || !isActiveAndEnabled)
        {
            RepositionAllRunningLemmings();
            return;
        }

        StartCoroutine(RepositionAfterDelay());
    }

    private IEnumerator RepositionAfterDelay()
    {
        yield return new WaitForSeconds(_refillDelaySeconds);
        RepositionAllRunningLemmings();
    }

    private static bool ShouldOccupyFormationPlace(LemmingView view)
    {
        return view != null && view.IsRun && !view.IsOnFire && !view.IsDead;
    }

    private void RepositionAllRunningLemmings()
    {
        for (int p = 0; p < _lemmingPlaces.Count; p++)
            _lemmingPlaces[p].IsEmpty = true;

        int placeIdx = 0;
        foreach (var view in _lemmingsEventsHandler.RunningLemmingViews)
        {
            if (!ShouldOccupyFormationPlace(view))
                continue;

            if (placeIdx >= _lemmingPlaces.Count)
                break;

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
        int formationIdx = 0;
        foreach (var view in _lemmingsEventsHandler.RunningLemmingViews)
        {
            if (view == lemmingView)
                return formationIdx;

            if (ShouldOccupyFormationPlace(view))
                formationIdx++;
        }

        return -1;
    }

    private void SetNewPosition(LemmingView lemmingView)
    {
        if (!ShouldOccupyFormationPlace(lemmingView))
            return;

        int index = GetFormationIndex(lemmingView);
        if (index < 0 || index >= _lemmingPlaces.Count)
            return;

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
        if (_activeInstance == this)
            _activeInstance = null;

        _lemmingsEventsHandler.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsEventsHandler.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStatesUIMediator.EndTrack.OnFinished -= StopLemmings;
    }
}
