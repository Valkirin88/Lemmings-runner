using System;
using UnityEngine;

public class MakeLemmingsInvinsible : IAbility
{
    public event Action OnDeactivated;
    public AbilitiiesConfig AbilitiesConfig;
    private readonly LemmingPlaceView _lemmingPlaceView;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private bool IsStarted;
    private float Timer;
    public MakeLemmingsInvinsible(LemmingPlaceView lemmingPlaceView, LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingPlaceView = lemmingPlaceView;
        _lemmingsEventsHandler = lemmingsEventsHandler;
    }

    public void Update()
    {
        if (IsStarted)
        {
            Timer += Time.deltaTime;
            if (AbilitiesConfig.DurationTime < Timer)
            {
                Deactivate();
                IsStarted = false;
            }
        }
    }

    public void Activate()
    {
        Timer = 0f;
        _lemmingPlaceView.IsInteractable = true;
        if (_lemmingsEventsHandler != null)
            _lemmingsEventsHandler.OnLemmingCountAdd += MakeNewLemmingInvincible;
        SetInvincibleForAllLemmings(true);
        IsStarted = true;
    }

    public void Deactivate()
    {
        _lemmingPlaceView.IsInteractable = false;
        if (_lemmingsEventsHandler != null)
            _lemmingsEventsHandler.OnLemmingCountAdd -= MakeNewLemmingInvincible;
        SetInvincibleForAllLemmings(false);
        OnDeactivated?.Invoke();
    }

    private void MakeNewLemmingInvincible(LemmingView lemming)
    {
        if (!IsStarted || lemming == null) return;
        lemming.IsInvincible = true;
    }

    private void SetInvincibleForAllLemmings(bool isInvincible)
    {
        if (_lemmingsEventsHandler == null) return;

        foreach (var lemming in _lemmingsEventsHandler.RunningLemmingViews)
        {
            if (lemming == null) continue;
            lemming.IsInvincible = isInvincible;
        }
    }
}
