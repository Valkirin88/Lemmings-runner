using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Zenject.Asteroids;

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
    private Button _leftButton;
    [SerializeField]
    private Button _rightButton;

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
    
    [SerializeField]
    private ObstaclesSet _obstaclesSet;

    [SerializeField]
    private RandomSpawner _randomSpawner;

    private InputController _inputController;
    private LemmingController _lemmingController;
    private LemmingsEventsHandler _lemmingsEventsHandler;
    private LemmingPlaceController _lemmingPlaceController;
    private ScoreHandler _scoreHandler;
    private LeaderboardServerSender _leaderboardServerSender;
    private CurrencyHandler _currencyHandler;
    
    //mediators
    private GameStatesUIMediator _gameStatesUIMediator;
    private EventsSoundMediator _eventsSoundMediator;

    private void Awake()
    {
        _inputController = new InputController(_accelerateButton, _jumpButton, _leftButton, _rightButton);
        _lemmingsEventsHandler = new LemmingsEventsHandler(_leaderLemmingView);
        _lemmingController = new LemmingController(_lemmingsEventsHandler, _inputController);
        _leaderboardServerSender = new LeaderboardServerSender();
        _scoreHandler = new ScoreHandler(_lemmingsEventsHandler);
        _gameStatesUIMediator = new GameStatesUIMediator(_endTrack, _uiHandler, _lemmingsEventsHandler, _scoreHandler,_leaderboardServerSender);
        _lemmingPlaceHandler.Initialize(_gameStatesUIMediator, _lemmingPlaceView);
        _lemmingPlaceController = new LemmingPlaceController(_lemmingPlaceView, _inputController, _lemmingConfig, _gameStatesUIMediator);
        _soundHandler.Initialize(_lemmingsEventsHandler);
        _eventsSoundMediator = new EventsSoundMediator(_soundHandler, _obstaclesSet, _lemmingsEventsHandler);
        _randomSpawner.Initialize(_obstaclesSet);
        _currencyHandler = new CurrencyHandler(_lemmingsEventsHandler);
        
        
    }

    private void Update()
    {
        _inputController.Update();
        _gameStatesUIMediator.Update();
        _lemmingPlaceController.Update();
        _scoreHandler.Update();
    }

    private void OnDestroy()
    {
        _inputController.Dispose();
        _lemmingController.Dispose();
        _gameStatesUIMediator.Dispose();
        _eventsSoundMediator.Dispose();
        _scoreHandler.Dispose();
        _currencyHandler.Dispose();
    }
}
