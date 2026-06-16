using System;
using UnityEngine;

public class ScoreHandler : IDisposable
{
    public event Action<int, int, bool> OnScoreChanged;
    
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private float _currentTimeforScore;
    private int _score;
    
    public int Score => _score;
    public ScoreHandler(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;

        _lemmingsEventsHandler.OnCurrencyGot += IncreaseScoreFromBonus;
    }

    private void IncreaseScoreFromBonus(int score)
    {
        IncreaseScore(score, fromBonus: true);
    }

    private void IncreaseScore(int score, bool fromBonus)
    {
        _score = Score + score;
        OnScoreChanged?.Invoke(_score, score, fromBonus);
    }

    public void Update()
    {
        _currentTimeforScore = _currentTimeforScore + Time.deltaTime;
        if (_currentTimeforScore >= GameInfo.TimeToIncreaseScore)
        {
            IncreaseScore(1, fromBonus: false);
            _currentTimeforScore = 0;
        }
    }

    public void SaveScoreResult(int displayScore)
    {
        int bestScore = PlayerPrefs.GetInt(GameInfo.UnityLeaderboardName);
        if (displayScore > bestScore)
        {
            PlayerPrefs.SetInt(GameInfo.UnityLeaderboardName, displayScore);
        }
    }


    public void Dispose()
    {
        _lemmingsEventsHandler.OnCurrencyGot -= IncreaseScoreFromBonus;
    }
}
