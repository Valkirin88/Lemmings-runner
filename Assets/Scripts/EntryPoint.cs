using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EntryPoint : MonoInstaller
{
    [SerializeField]
    private LemmingView _leaderLemmingView;
    
    [SerializeField]
    private LemmingConfig _lemmingConfig;
    
    [SerializeField]
    private Button _accelerateButton;
    [SerializeField]
    private Button _jumpButton;

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
        _inputController = new InputController(_accelerateButton, _jumpButton);
        _lemmingsStateSet = new LemmingsStateSet(_leaderLemmingView);
        _lemmingController = new LemmingController(_lemmingsStateSet, _inputController);
        _gameStateCollector = new GameStateCollector(_endTrack, _uiHandler, _lemmingsStateSet);
        _lemmingPlaceHandler.Initialize(_gameStateCollector);
        _lemmingPlaceController = new LemmingPlaceController(_lemmingPlaceView, _inputController, _lemmingConfig, _gameStateCollector);
        _soundHandler.Initialize(_lemmingsStateSet);
    }

    private void Update()
    {
        _inputController.Update();
        _gameStateCollector.Update();
        _lemmingPlaceController.Update();
    }

    private void OnDestroy()
    {
        _inputController.Dispose();
        _lemmingController.Dispose();
        _gameStateCollector.Dispose();
    }
}
