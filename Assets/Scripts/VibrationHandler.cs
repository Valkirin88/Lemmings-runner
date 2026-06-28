using CandyCoded.HapticFeedback;
using UnityEngine;

public class VibrationHandler : MonoBehaviour
{
    private LemmingsEventsHandler _lemmingsEventsHandler;

    public void Initialize(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _lemmingsEventsHandler.OnLemmingCountAdd += OnLemmingAdded;
        _lemmingsEventsHandler.OnCurrencyGot += OnBonusCollected;
        foreach (var lemming in _lemmingsEventsHandler.RunningLemmingViews)
            SubscribeToLemming(lemming);
    }

    private void OnLemmingAdded(LemmingView lemmingView)
    {
        SubscribeToLemming(lemmingView);
        DoShortVibro();
    }

    private void OnBonusCollected(int _)
    {
        DoShortVibro();
    }

    private void SubscribeToLemming(LemmingView lemmingView)
    {
        lemmingView.OnLemmingKilled += OnLemmingKilled;
    }

    private void OnLemmingKilled(LemmingView lemmingView)
    {
        if (lemmingView.DeathCause == LemmingDeathCause.Drill)
            return;

        DoHeavyVibro();
    }

    public static void PlayDrillHitVibro()
    {
        Handheld.Vibrate();
    }

    private void DoShortVibro()
    {
        HapticFeedback.LightFeedback();
    }

    private void DoHeavyVibro()
    {
        HapticFeedback.HeavyFeedback();
    }

    private void OnDestroy()
    {
        if (_lemmingsEventsHandler == null)
            return;

        _lemmingsEventsHandler.OnLemmingCountAdd -= OnLemmingAdded;
        _lemmingsEventsHandler.OnCurrencyGot -= OnBonusCollected;
        foreach (var lemming in _lemmingsEventsHandler.RunningLemmingViews)
        {
            if (lemming != null)
                lemming.OnLemmingKilled -= OnLemmingKilled;
        }
    }
}
