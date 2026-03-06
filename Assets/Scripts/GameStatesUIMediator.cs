using System;


public class GameStatesUIMediator : IDisposable
{
    private readonly EndTrack _endTrack;
    private readonly UIHandler _uiHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly ScoreHandler _scoreHandler;
    private readonly LeaderboardServerSender _leaderboardServerSender;
    
    private int _lemmingQuantity;
    private GameState _gameState;
    
    public LemmingsEventsHandler LemmingsEventsHandler => _lemmingsEventsHandler;

    public EndTrack EndTrack => _endTrack;

    public GameState State => _gameState;

    public GameStatesUIMediator(EndTrack endTrack,UIHandler uiHandler, LemmingsEventsHandler lemmingsEventsHandler, ScoreHandler scoreHandler, LeaderboardServerSender leaderboardServerSender)
    {
        _endTrack = endTrack;
        _uiHandler = uiHandler;
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _scoreHandler = scoreHandler;
        _leaderboardServerSender = leaderboardServerSender;
        
        EndTrack.OnFinished += Finish;
        _scoreHandler.OnScoreChanged += _uiHandler.ShowScore;

        _gameState = GameState.Game;
        _uiHandler.GameState = _gameState;
    }

    private void Finish()
    {
        UnityEngine.Debug.Log($"Finish called! LemmingQuantity: {_lemmingQuantity}, Current State: {_gameState}");
        
        if (_lemmingQuantity >= 12)
        {
            _gameState = GameState.Finish;
            _uiHandler.GameState = _gameState;
        }
        else
        {
            _gameState = GameState.GameOver;
            _uiHandler.GameState = _gameState;
        }
        
        UnityEngine.Debug.Log($"New State: {_gameState}");
    }

    public void Update()
    {
        CheckGameOver();
  
        _uiHandler.ShowCurrentQuantity(_lemmingQuantity);
    }

    private void CheckGameOver()
    {
        _lemmingQuantity = LemmingsEventsHandler.RunningLemmingViews.Count;
        if (_lemmingQuantity <= 0 && _gameState == GameState.Game)
        {
            _scoreHandler.SaveScoreResult();
            _leaderboardServerSender.SendScoreToServer(_scoreHandler.Score);
            _gameState = GameState.GameOver;
            _uiHandler.GameState = _gameState;
            
        }
    }
    
    
    public void Dispose()
    {
        EndTrack.OnFinished -= Finish;
        _scoreHandler.OnScoreChanged -= _uiHandler.ShowScore;
    }
}

public enum GameState
{
    Game,
    Paused,
    GameOver,
    Finish
}
