using System;
using System.Collections.Generic;


public class LemmingsStateSet 
{
    public event Action<LemmingView> OnLemmingCountAdd;
    public event Action<LemmingView> OnLemmingCountRemove;
    public event Action OnLemmingOnFire;
    
    
    private List<LemmingView> _runningLemmingViews;
    
    private LemmingView _leaderLemmingView;
    public List<LemmingView> RunningLemmingViews => _runningLemmingViews;

    public LemmingsStateSet(LemmingView leaderLemmingView)
    {
        _runningLemmingViews = new List<LemmingView>();
        
        _leaderLemmingView = leaderLemmingView;

        AddLemmingInList(_leaderLemmingView);
    }

    

    private void AddLemmingInList(LemmingView lemmingView)
    {
        RunningLemmingViews.Add(lemmingView);
        SubscribeOnNewLemmingsCaught(lemmingView);
        SubscribeOnLemmingKilled(lemmingView);
        SubscribeLimmingSream(lemmingView);
        OnLemmingCountAdd?.Invoke(lemmingView);
    }

    private void SubscribeLimmingSream(LemmingView lemmingView)
    {
        lemmingView.OnLemmingOnFire += FireLemming;
    }

    private void FireLemming()
    {
        OnLemmingOnFire?.Invoke();
    }

    private void RemoveLemmingInList(LemmingView lemmingView)
    {
        RunningLemmingViews.Remove(lemmingView);
        UnsubscribeOnNewLemmingsCaught(lemmingView);
        lemmingView.OnLemmingOnFire -= FireLemming;
        OnLemmingCountRemove?.Invoke(lemmingView);
    }
    
    private void SubscribeOnNewLemmingsCaught(LemmingView lemmingView)
    {
        lemmingView.OnLemmingCaught += AddLemmingInList;
    }
    
    private void UnsubscribeOnNewLemmingsCaught(LemmingView lemmingView)
    {
        lemmingView.OnLemmingCaught -= AddLemmingInList;
        UnsubscribeOnLemmingKilled(lemmingView);
    }

    private void SubscribeOnLemmingKilled(LemmingView lemmingView)
    {
        lemmingView.OnLemmingKilled += RemoveLemmingInList;
    }

    private void UnsubscribeOnLemmingKilled(LemmingView lemmingView)
    {
        lemmingView.OnLemmingKilled -= RemoveLemmingInList;
    }
}
