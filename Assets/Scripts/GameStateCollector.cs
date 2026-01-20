using System;


public class GameStateCollector : IDisposable
{
    private readonly EndTrack _endTrack;
    private readonly UIHandler _uiHandler;
    private readonly RunningLemmingsSet _runningRunningLemmingsSet;
    
    public RunningLemmingsSet RunningLemmingsSet => _runningRunningLemmingsSet;

    public EndTrack EndTrack => _endTrack;

    public GameStateCollector(EndTrack endTrack,UIHandler uiHandler, RunningLemmingsSet runningRunningLemmingsSet)
    {
        _endTrack = endTrack;
        _uiHandler = uiHandler;
        _runningRunningLemmingsSet = runningRunningLemmingsSet;
        
        EndTrack.OnFinished += ShowResult;
    }

    private void ShowResult()
    {
        
    }

    public void Update()
    {
        _uiHandler.ShowCurrentQuantity(RunningLemmingsSet.RunningLemmingViews.Count);
    }
    public void Dispose()
    {
        EndTrack.OnFinished -= ShowResult;
    }
}
