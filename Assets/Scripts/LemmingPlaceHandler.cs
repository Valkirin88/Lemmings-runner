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
        // Ничего не меняем сразу — сдвиг только через секунду
        StartCoroutine(DelayedReposition(removedIndex));
    }

    private IEnumerator DelayedReposition(int startIndex)
    {
        yield return new WaitForSeconds(1f);

        var views = _lemmingsStateSet.RunningLemmingViews;
        // Освобождаем место погибшего и места хвоста, которые освободятся при сдвиге
        for (int p = startIndex; p < _lemmingPlaces.Count; p++)
            _lemmingPlaces[p].IsEmpty = true;

        // Сдвигаем вперёд: лемминг на позиции startIndex занимает место startIndex и т.д.
        for (int i = startIndex; i < views.Count && i < _lemmingPlaces.Count; i++)
        {
            var view = views[i];
            var place = _lemmingPlaces[i];
            place.IsEmpty = false;
            view.RunningPlace = place.transform;
        }
    }

    private void PlaceNewLemmingState(LemmingView lemmingView)
    {
        SetNewPosition(lemmingView);
    }

    private void SetNewPosition(LemmingView lemmingView)
    {
        if (lemmingView.IsOnFire) return;

        int index = _lemmingsStateSet.RunningLemmingViews.IndexOf(lemmingView);
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
        _lemmingsStateSet.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStateCollector.EndTrack.OnFinished -= StopLemmings;
    }
}
