using System;


public class GameStateCollector : IDisposable
{
    private readonly EndTrack _endTrack;
    private readonly UIHandler _uiHandler;
    private readonly LemmingsStateSet _lemmingsStateSet;
    
    private int _lemmingQuantity;
    private GameState _gameState;
    
    public LemmingsStateSet LemmingsStateSet => _lemmingsStateSet;

    public EndTrack EndTrack => _endTrack;

    public GameState State => _gameState;

    public GameStateCollector(EndTrack endTrack,UIHandler uiHandler, LemmingsStateSet lemmingsStateSet)
    {
        _endTrack = endTrack;
        _uiHandler = uiHandler;
        _lemmingsStateSet = lemmingsStateSet;
        
        EndTrack.OnFinished += Finish;

        _gameState = GameState.Game;
        _uiHandler.GameState = _gameState;
    }

    private void Finish()
    {
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
    }

    public void Update()
    {
        _lemmingQuantity = LemmingsStateSet.RunningLemmingViews.Count;
        if (_lemmingQuantity<=0)
        {
            _gameState = GameState.GameOver;
            _uiHandler.GameState = _gameState;
        }
        _uiHandler.ShowCurrentQuantity(_lemmingQuantity);
        
    }
    public void Dispose()
    {
        EndTrack.OnFinished -= Finish;
    }
}

public enum GameState
{
    Game,
    Paused,
    GameOver,
    Finish
}
