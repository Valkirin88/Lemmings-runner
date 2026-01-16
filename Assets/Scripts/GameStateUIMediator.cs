using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GameStateUIMediator : IDisposable
{
    private readonly EndTrack _endTrack;
    private readonly UIHandler _uiHandler;
    private readonly RunningLemmingsSet _runningLemmingsSet;
    
    

    public GameStateUIMediator(EndTrack endTrack,UIHandler uiHandler, RunningLemmingsSet runningLemmingsSet)
    {
        _endTrack = endTrack;
        _uiHandler = uiHandler;
        _runningLemmingsSet = runningLemmingsSet;
        
        _endTrack.OnFinished += ShowResult;
    }

    private void ShowResult()
    {
        
    }

    public void Update()
    {
        _uiHandler.ShowCurrentQuantity(_runningLemmingsSet.RunningLemmingViews.Count);
    }
    public void Dispose()
    {
        _endTrack.OnFinished -= ShowResult;
    }
}
