using System;
using UnityEngine;

public class UILemmingStateMediator : IDisposable
{
    private readonly UIHandler _uiHandler;
    private readonly LemmingsStateSet _lemmingsStateSet;

    public UILemmingStateMediator(UIHandler uiHandler, LemmingsStateSet lemmingsStateSet)
    {
        _uiHandler = uiHandler;
        _lemmingsStateSet = lemmingsStateSet;

        _lemmingsStateSet.OnScoreBonusGot += IncreaseScore;
    }

    private void IncreaseScore(ScoreBonus scoreBonus)
    {
        _uiHandler.AddScore(scoreBonus.Score);
    }


    public void Dispose()
    {
        _lemmingsStateSet.OnScoreBonusGot -= IncreaseScore;
    }
}
