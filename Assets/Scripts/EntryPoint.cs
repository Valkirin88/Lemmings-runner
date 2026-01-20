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
    private GameStateCollector _gameStateCollector;
    
    
    private void Awake()
    {
        _inputController = new InputController();
        _lemmingController = new LemmingController(_leaderLemmingView, _inputController);
        _runningLemmingsSet = new RunningLemmingsSet(_leaderLemmingView);
        _gameStateCollector = new GameStateCollector(_endTrack, _uiHandler, _runningLemmingsSet);
        _lemmingPlaceHandler.Initialize( _lemmingController, _gameStateCollector);
        
    }

    private void Update()
    {
        _inputController.Update();
        _gameStateCollector.Update();
    }

    private void OnDestroy()
    {
        _lemmingController.Dispose();
        _gameStateCollector.Dispose();
    }
}
