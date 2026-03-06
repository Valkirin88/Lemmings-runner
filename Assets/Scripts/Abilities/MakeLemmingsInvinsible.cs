using System;
using UnityEngine;

public class MakeLemmingsInvinsible : IAbility
{
    public event Action OnDeactivated;
    public AbilitiiesConfig AbilitiesConfig;
    private readonly LemmingPlaceView _lemmingPlaceView;
    private bool IsStarted;
    private float Timer;
    public MakeLemmingsInvinsible(LemmingPlaceView lemmingPlaceView)
    {
        _lemmingPlaceView = lemmingPlaceView;
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
        _lemmingPlaceView.IsInteractable = true;
        IsStarted = true;
    }

    public void Deactivate()
    {
        _lemmingPlaceView.IsInteractable = false;
        OnDeactivated?.Invoke();
    }
}
