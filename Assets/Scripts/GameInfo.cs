using UnityEngine;

public static class GameInfo
{
    private static string _unityLeaderboardName = "Test";
    private static float _timeToIncreaseScore = 5f;

    public static string UnityLeaderboardName => _unityLeaderboardName;

    public static float TimeToIncreaseScore => _timeToIncreaseScore;
}
