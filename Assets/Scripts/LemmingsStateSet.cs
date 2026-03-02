using System;
using System.Collections.Generic;


public class LemmingsStateSet 
{
    public event Action<LemmingView> OnLemmingCountAdd;
    public event Action<LemmingView, int> OnLemmingCountRemove;
    public event Action  OnLemmingKilled;
    public event Action OnLemmingOnFire;
    public event Action<int> OnScoreBonusGot;
    
    
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
        SubscribeLemmingScream(lemmingView);
        SubscribeScoreBonusGet(lemmingView);
        OnLemmingCountAdd?.Invoke(lemmingView);
    }

    private void SubscribeScoreBonusGet(LemmingView lemmingView)
    {
        lemmingView.OnScoreBonusGot += BonusGet;
    }

    private void BonusGet(ScoreBonus scoreBonus)
    {
        OnScoreBonusGot?.Invoke(scoreBonus.Score);
    }

    private void SubscribeLemmingScream(LemmingView lemmingView)
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
        UnsubscribeScoreBonusGet(lemmingView);
        lemmingView.OnLemmingOnDanger -= FireLemming;
        OnLemmingCountRemove?.Invoke(lemmingView, removedIndex);
        OnLemmingKilled?.Invoke();
    }

    private void UnsubscribeScoreBonusGet(LemmingView lemmingView)
    {
        lemmingView.OnScoreBonusGot -= BonusGet;
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
