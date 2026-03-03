using System;
using UnityEngine;

public class ScoreHandler : IDisposable
{
    public event Action<int> OnScoreChanged;
    
    private readonly LemmingsStateSet _lemmingsStateSet;
    private float _currentTimeforScore;
    private int _score;
    
    public int Score => _score;
    public ScoreHandler(LemmingsStateSet lemmingsStateSet)
    {
        _lemmingsStateSet = lemmingsStateSet;

        _lemmingsStateSet.OnScoreBonusGot += IncreaseScore;
    }

  

    private void IncreaseScore(int score)
    {
        _score = Score + score;
        OnScoreChanged?.Invoke(_score);
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

    public void SaveScoreResult()
    {
        int bestScore = PlayerPrefs.GetInt(GameInfo.UnityLeaderboardName);
        if (Score > bestScore)
        {
            PlayerPrefs.SetInt(GameInfo.UnityLeaderboardName, Score);
        }
    }


    public void Dispose()
    {
        _lemmingsStateSet.OnScoreBonusGot -= IncreaseScore;
    }
}
