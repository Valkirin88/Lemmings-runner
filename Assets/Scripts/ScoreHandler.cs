using System;
using UnityEngine;

public class ScoreHandler : IDisposable
{
    public event Action<int> OnScoreChanged;
    
    private readonly LemmingsStateSet _lemmingsStateSet;
    private float _currentTimeforScore;
    public ScoreHandler(LemmingsStateSet lemmingsStateSet)
    {
        _lemmingsStateSet = lemmingsStateSet;

        _lemmingsStateSet.OnScoreBonusGot += IncreaseScore;
    }

    private void IncreaseScore(int score)
    {
        OnScoreChanged?.Invoke(score);
    }

    public void Update()
    {
        _currentTimeforScore = _currentTimeforScore + Time.deltaTime;
        if (_currentTimeforScore >= GameInfo.TimeToIncreaseScore)
        {
            IncreaseScore(1);
            _currentTimeforScore = 0;
        }
    }


    public void Dispose()
    {
        _lemmingsStateSet.OnScoreBonusGot -= IncreaseScore;
    }
}
