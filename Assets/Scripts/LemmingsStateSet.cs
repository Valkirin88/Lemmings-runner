using System;
using System.Collections.Generic;


public class LemmingsStateSet 
{
    public event Action<LemmingView> OnLemmingCountAdd;
    public event Action<LemmingView, int> OnLemmingCountRemove;
    public event Action  OnLemmingKilled;
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
        lemmingView.OnLemmingOnDanger += FireLemming;
    }

    private void FireLemming()
    {
        OnLemmingOnFire?.Invoke();
    }

    private void RemoveLemmingInList(LemmingView lemmingView)
    {
        int removedIndex = _runningLemmingViews.IndexOf(lemmingView);
        RunningLemmingViews.Remove(lemmingView);
        UnsubscribeOnNewLemmingsCaught(lemmingView);
        lemmingView.OnLemmingOnDanger -= FireLemming;
        OnLemmingCountRemove?.Invoke(lemmingView, removedIndex);
        OnLemmingKilled?.Invoke();
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
