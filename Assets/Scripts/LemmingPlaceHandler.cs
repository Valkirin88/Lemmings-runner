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
            var leader = _lemmingsStateSet.RunningLemmingViews[0];
            SetNewPosition(leader);
        }
    }

    private void ReplaceLemmingsState(LemmingView lemmingView, int removedIndex)
    {
        // Сразу освобождаем место погибшего лемминга
        if (lemmingView.RunningPlace != null)
        {
            var runPlace = lemmingView.RunningPlace.GetComponent<RunPlace>();
            if (runPlace != null)
                runPlace.IsEmpty = true;
        }
        // Сразу сдвигаем хвост вперёд — без задержки, чтобы все догоняли кучкой
        RepositionTail(removedIndex);
    }

    private void RepositionTail(int startIndex)
    {
        var views = _lemmingsStateSet.RunningLemmingViews;
        // Освобождаем места, которые освободятся при сдвиге (хвост колонны)
        for (int p = startIndex + 1; p < _lemmingPlaces.Count; p++)
            _lemmingPlaces[p].IsEmpty = true;

        // Сдвигаем вперёд: лемминг на позиции startIndex занимает место startIndex, следующий — startIndex+1 и т.д.
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
    }

    private void OnDestroy()
    {
        _lemmingsStateSet.OnLemmingCountAdd -= PlaceNewLemmingState;
        _lemmingsStateSet.OnLemmingCountRemove -= ReplaceLemmingsState;
        
        _gameStateCollector.EndTrack.OnFinished -= StopLemmings;
    }
}
