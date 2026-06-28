using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Zenject.Asteroids;

public class EntryPoint : MonoInstaller
{
    [SerializeField]
    private LemmingView _leaderLemmingView;

    [SerializeField]
    private List<AbilitiiesConfig> _abilitiesConfigs;
    
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
    private AbilitiesFX _abilitiesFX;
    
    [SerializeField]
    private AbilityButton _abilityButton;
    
    [SerializeField]
    private SoundsHandler _soundHandler;

    [SerializeField]
    private VibrationHandler _vibrationHandler;

    [SerializeField]
    private Bottom _bottom;

    [SerializeField]
    private AudioClip _destroyAllObstaclesSound;
    
    [SerializeField]
    private ObstaclesSet _obstaclesSet;

    [SerializeField]
    private RandomSpawner _randomSpawner;
    
    [SerializeField]
    private AdsHandler _adsHandler;

    [SerializeField]
    [Tooltip("Сколько леммингов оживлять при продолжении после просмотра рекламы")]
    private int _continueLemmingsCount = 1;

    private InputController _inputController;
    private LemmingController _lemmingController;
    private LemmingsEventsHandler _lemmingsEventsHandler;
    private LemmingPlaceController _lemmingPlaceController;
    private ScoreHandler _scoreHandler;
    private LeaderboardServerSender _leaderboardServerSender;
    private CurrencyHandler _currencyHandler;
    private AbilitiesHandler _abilitiesHandler;
    
    //mediators
    private GameStatesUIMediator _gameStatesUIMediator;
    private EventsSoundMediator _eventsSoundMediator;
    private AdsUIMediator _adsUIMediator;

    private void Awake()
    {
        SetSettings();
        
        _inputController = new InputController(_accelerateButton, _jumpButton, _leftButton, _rightButton);
        _lemmingsEventsHandler = new LemmingsEventsHandler(_leaderLemmingView);
        _lemmingController = new LemmingController(_lemmingsEventsHandler, _inputController, _soundHandler);
        _leaderboardServerSender = new LeaderboardServerSender();
        _scoreHandler = new ScoreHandler(_lemmingsEventsHandler);
        _gameStatesUIMediator = new GameStatesUIMediator(_endTrack, _uiHandler, _lemmingsEventsHandler, _scoreHandler,_leaderboardServerSender);
        _lemmingPlaceHandler.Initialize(_gameStatesUIMediator, _lemmingPlaceView);
        _lemmingPlaceController = new LemmingPlaceController(_lemmingPlaceView, _inputController, _lemmingConfig, _gameStatesUIMediator);
        _soundHandler.Initialize(_lemmingsEventsHandler);
        var vibrationHandler = _vibrationHandler;
        if (vibrationHandler == null && _soundHandler != null)
            vibrationHandler = _soundHandler.GetComponent<VibrationHandler>();
        if (vibrationHandler == null && _soundHandler != null)
            vibrationHandler = _soundHandler.gameObject.AddComponent<VibrationHandler>();
        if (vibrationHandler != null)
            vibrationHandler.Initialize(_lemmingsEventsHandler);
        if (_bottom != null)
            _bottom.Initialize(_soundHandler);
        _eventsSoundMediator = new EventsSoundMediator(_soundHandler, _obstaclesSet, _lemmingsEventsHandler);
        _randomSpawner.Initialize(_obstaclesSet);
        _currencyHandler = new CurrencyHandler(_lemmingsEventsHandler);
        _abilitiesHandler = new AbilitiesHandler(_obstaclesSet,_randomSpawner,_lemmingPlaceHandler, _lemmingsEventsHandler, _lemmingPlaceView, _abilitiesConfigs, _abilitiesFX, _destroyAllObstaclesSound);
        _abilityButton.Initialize(_abilitiesHandler);
        
        var adsHandler = AdsHandler.Instance != null ? AdsHandler.Instance : _adsHandler;
        _adsUIMediator = new AdsUIMediator(adsHandler, _uiHandler,
            _lemmingsEventsHandler, _randomSpawner, _gameStatesUIMediator,
            _obstaclesSet, _destroyAllObstaclesSound,
            _leaderLemmingView, _lemmingPlaceView, _continueLemmingsCount);
    }

    private void Update()
    {
        _inputController.Update();
        _gameStatesUIMediator.Update();
        _lemmingPlaceController.Update();
        _scoreHandler.Update();
        _abilitiesHandler.Update();
    }
    
    private void SetSettings()
    {
        Time.fixedDeltaTime = 1 / 120f;
        Physics2D.velocityIterations = 12;
        Physics2D.positionIterations = 6;
        Application.targetFrameRate = 60; // Фиксированный FPS
        QualitySettings.vSyncCount = 0;   // Отключить VSync
    }


    private void OnDestroy()
    {
        _inputController.Dispose();
        _lemmingController.Dispose();
        _gameStatesUIMediator.Dispose();
        _eventsSoundMediator.Dispose();
        _scoreHandler.Dispose();
        _currencyHandler.Dispose();
        _abilitiesHandler.Dispose();
        _adsUIMediator.Dispose();
    }
}
