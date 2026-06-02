using System;
using System.Collections.Generic;


public class LemmingsEventsHandler 
{
    public event Action<LemmingView> OnLemmingCountAdd;
    public event Action<LemmingView, int> OnLemmingCountRemove;
    public event Action  OnLemmingKilled;
    public event Action OnLemmingOnFire;
    public event Action<int> OnCurrencyGot;
    
    
    private List<LemmingView> _runningLemmingViews;
    
    private LemmingView _leaderLemmingView;
    public List<LemmingView> RunningLemmingViews => _runningLemmingViews;

    /// <summary>
    /// Живые лемминги в списке. Удаляет «битые» ссылки (уничтожен без Kill).
    /// </summary>
    public int GetLivingLemmingCount()
    {
        int count = 0;
        for (int i = _runningLemmingViews.Count - 1; i >= 0; i--)
        {
            var view = _runningLemmingViews[i];
            if (view == null)
            {
                _runningLemmingViews.RemoveAt(i);
                continue;
            }

            if (!view.IsDead)
                count++;
        }

        return count;
    }

    public LemmingsEventsHandler(LemmingView leaderLemmingView)
    {
        _runningLemmingViews = new List<LemmingView>();
        
        _leaderLemmingView = leaderLemmingView;

        AddLemming(_leaderLemmingView);
    }

    
    
    public void AddLemming(LemmingView lemmingView)
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

    private void BonusGet(AppleCurrency appleCurrency)
    {
        OnCurrencyGot?.Invoke(appleCurrency.Score);
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
        if (removedIndex < 0)
            return;

        RunningLemmingViews.RemoveAt(removedIndex);
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
        lemmingView.OnLemmingCaught += AddLemming;
    }
    
    private void UnsubscribeOnNewLemmingsCaught(LemmingView lemmingView)
    {
        lemmingView.OnLemmingCaught -= AddLemming;
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
