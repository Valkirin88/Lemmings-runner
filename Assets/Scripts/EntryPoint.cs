using UnityEngine;
using Zenject;

public class EntryPoint : MonoInstaller
{
    [SerializeField]
    private LemmingView _leaderLemmingView;

    [SerializeField]
    private LemmingPlaceHandler _lemmingPlaceHandler;
    
    [SerializeField]
    private EndTrack _endTrack;
    
    [SerializeField]
    private UIHandler _uiHandler;
    
    private InputController _inputController;
    private LemmingController _lemmingController;
    private RunningLemmingsSet _runningLemmingsSet;
    
    //mediators
    private GameStateUIMediator _gameStateUIMediator;
    
    
    private void Awake()
    {
        _inputController = new InputController();
        _lemmingController = new LemmingController(_leaderLemmingView, _inputController);
        _runningLemmingsSet = new RunningLemmingsSet(_leaderLemmingView);
        _lemmingPlaceHandler.Initialize(_runningLemmingsSet, _lemmingController);
        _gameStateUIMediator = new GameStateUIMediator(_endTrack, _uiHandler, _runningLemmingsSet);
    }

    private void Update()
    {
        _inputController.Update();
        _gameStateUIMediator.Update();
    }

    private void OnDestroy()
    {
        _lemmingController.Dispose();
        _gameStateUIMediator.Dispose();
    }
}
