using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EntryPoint : MonoInstaller
{
    [SerializeField]
    private LemmingView _leaderLemmingView;
    [SerializeField]
    private List<RunPlace> _lemmingRunPlaces;
    [SerializeField]
    private LemmingPlaceHandler _lemmingPlaceHandler;
    
    private InputController _inputController;
    private LemmingController _lemmingController;
    private RunningLemmingsSet _runningLemmingsSet;
    
    
    private void Awake()
    {
        _inputController = new InputController();
        _lemmingController = new LemmingController(_leaderLemmingView, _inputController);
        _runningLemmingsSet = new RunningLemmingsSet(_leaderLemmingView);
        _lemmingPlaceHandler.Initialize(_runningLemmingsSet);
    }

    private void Update()
    {
        _inputController.Update();
    }

    private void Dispose()
    {
        _lemmingController.Dispose();
    }
}
