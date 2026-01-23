using UnityEngine;
using Zenject;

public class EntryPoint : MonoInstaller
{
    [SerializeField]
    private LemmingView _leaderLemmingView;
    
    [SerializeField]
    private LemmingConfig _lemmingConfig;

    [SerializeField]
    private LemmingPlaceHandler _lemmingPlaceHandler;
    [SerializeField]
    private LemmingPlaceView _lemmingPlaceView;
    
    [SerializeField]
    private EndTrack _endTrack;
    
    [SerializeField]
    private UIHandler _uiHandler;
    
    [SerializeField]
    private SoundsHandler _soundHandler;
    
    private InputController _inputController;
    private LemmingController _lemmingController;
    private LemmingsStateSet _lemmingsStateSet;
    private LemmingPlaceController _lemmingPlaceController;
    
    //mediators
    private GameStateCollector _gameStateCollector;
    
    
    private void Awake()
    {
        _inputController = new InputController();
        _lemmingController = new LemmingController(_leaderLemmingView, _inputController);
        _lemmingsStateSet = new LemmingsStateSet(_leaderLemmingView);
        _gameStateCollector = new GameStateCollector(_endTrack, _uiHandler, _lemmingsStateSet);
        _lemmingPlaceHandler.Initialize(_gameStateCollector);
        _lemmingPlaceController = new LemmingPlaceController(_lemmingPlaceView, _inputController, _lemmingConfig);
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
