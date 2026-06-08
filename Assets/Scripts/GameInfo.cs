using UnityEngine;

public static class GameInfo
{
    private static string _unityLeaderboardName = "Test";
    private static float _timeToIncreaseScore = 5f;
    private static int _playAttempts = 1;
    private static bool _isAdsOn = true;
    private static string _adsHandlerName = "demo-rewarded-yandex";
    

    public static string UnityLeaderboardName => _unityLeaderboardName;

    public static float TimeToIncreaseScore => _timeToIncreaseScore;

    public static int PlayAttempts => _playAttempts;

    public static bool IsAdsOn => _isAdsOn;

    public static string AdsHandlerName => _adsHandlerName;
}
