using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardServerSender 
{
    private string LeaderboardId => GameInfo.UnityLeaderboardName;

    private bool _isInitialized;
    private Task _initializationTask;

    public LeaderboardServerSender()
    {
        _initializationTask = InitializeUnityServicesAsync();
    }

    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            // Дожидаемся инициализации Unity Services перед обращением к AuthenticationService
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            await SignInWithDeviceIdAsync();

            _isInitialized = AuthenticationService.Instance.IsSignedIn;

            if (_isInitialized)
                Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка инициализации: {e.Message}");
        }
    }

    private async Task SignInWithDeviceIdAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Игрок уже залогинен");
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

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

    public async void SendScoreToServer(int score)
    {
        if (_initializationTask != null)
            await _initializationTask;

        if (!_isInitialized)
            return;

        await SubmitScoreAsync(LeaderboardId, score);
    }

    private async Task SubmitScoreAsync(string leaderboardId, int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка отправки очков: {e.Message}");
        }
    }
}
