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
    
    [SerializeField]
    private SoundsHandler _soundHandler;
    
    private InputController _inputController;
    private LemmingController _lemmingController;
    private LemmingsStateSet _lemmingsStateSet;
    
    //mediators
    private GameStateCollector _gameStateCollector;
    
    
    private void Awake()
    {
        _inputController = new InputController();
        _lemmingController = new LemmingController(_leaderLemmingView, _inputController);
        _lemmingsStateSet = new LemmingsStateSet(_leaderLemmingView);
        _gameStateCollector = new GameStateCollector(_endTrack, _uiHandler, _lemmingsStateSet);
        _lemmingPlaceHandler.Initialize( _lemmingController, _gameStateCollector);
        _soundHandler.Initialize(_lemmingsStateSet);
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
