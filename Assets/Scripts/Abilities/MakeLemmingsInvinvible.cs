using UnityEngine;

public class MakeLemmingsInvinvible 
{
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    
    public MakeLemmingsInvinvible(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
    }

    public void Activate()
    {
        foreach (var lemming in _lemmingsEventsHandler.RunningLemmingViews)
        {
            
        }
    }
}
