using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardServerSender 
{
    private string LeaderboardId => GameInfo.UnityLeaderboardName;

    public LeaderboardServerSender()
    {
        InitializeUnityServices();
    }

    private void InitializeUnityServices()
    {
        try
        {
            // Инициализация Unity Services
            UnityServices.InitializeAsync();
            
            // Аутентификация через Device ID
            SignInWithDeviceId();
            
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка инициализации: {e.Message}");
        }
    }
    
    private void SignInWithDeviceId()
    {
        // Проверяем, не залогинен ли уже
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Игрок уже залогинен");
            return;
        }

        try
        {
            // Логинимся анонимно (Unity автоматически использует Device ID для идентификации)
            AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            Debug.Log($"Успешная аутентификация! Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Ошибка аутентификации: {ex.Message}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Ошибка запроса: {ex.Message}");
        }
    }

    public void SendScoreToServer(int score)
    {
        SubmitScore(LeaderboardId, score);
    }

    // Отправка очков на сервер
    private void SubmitScore(string leaderboardId, int score)
    {
        try
        {
            LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка отправки очков: {e.Message}");
        }
    }
}
