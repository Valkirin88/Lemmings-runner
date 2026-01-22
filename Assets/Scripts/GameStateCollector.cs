using System;


public class GameStateCollector : IDisposable
{
    private readonly EndTrack _endTrack;
    private readonly UIHandler _uiHandler;
    private readonly LemmingsStateSet _lemmingsStateSet;
    
    private int _lemmingQuantity;
    
    public LemmingsStateSet LemmingsStateSet => _lemmingsStateSet;

    public EndTrack EndTrack => _endTrack;

    public GameStateCollector(EndTrack endTrack,UIHandler uiHandler, LemmingsStateSet lemmingsStateSet)
    {
        _endTrack = endTrack;
        _uiHandler = uiHandler;
        _lemmingsStateSet = lemmingsStateSet;
        
        EndTrack.OnFinished += Finish;

        _uiHandler.GameState = GameState.Game;
    }

    private void Finish()
    {
        if(_lemmingQuantity >= 12)
            _uiHandler.GameState = GameState.Finish;
        else
        {
            _uiHandler.GameState = GameState.GameOver;
        }
    }

    public void Update()
    {
        _lemmingQuantity = LemmingsStateSet.RunningLemmingViews.Count;
        if (_lemmingQuantity<=0)
        {
            _uiHandler.GameState = GameState.GameOver;
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
