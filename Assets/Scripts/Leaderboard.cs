using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class Leaderboard : MonoBehaviour
{
    [SerializeField]
    private GameObject _usualRowPrefab;
    [SerializeField]
    private GameObject _firstRowPrefab;
    [SerializeField]
    private GameObject _secondThirdRowPrefab;
    [SerializeField]
    private GameObject _thirdRowPrefab;    


    [SerializeField]
    public Transform _rowsParent;
    [SerializeField]
    public TMP_Text _positionText;
    [SerializeField]
    public TMP_Text _scoreText;

    [SerializeField]
    private GameObject _tryingToConnectToServer;

    private int _currentScore;
    
    private string LeaderboardId => GameInfo.UnityLeaderboardName;
    
    private async void Awake()
    {
        _scoreText.text = PlayerPrefs.GetInt(GameInfo.UnityLeaderboardName).ToString();
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            // Инициализация Unity Services
            await UnityServices.InitializeAsync();
            
            // Аутентификация через Device ID
            await SignInWithDeviceId();
            
            Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка инициализации: {e.Message}");
        }
    }

    private async Task SignInWithDeviceId()
    {
        // Проверяем, не залогинен ли уже
        if (AuthenticationService.Instance.IsSignedIn)
        {
            // Получаем лучший результат игрока
            await GetPlayerScore(LeaderboardId);
            
            // Получаем и отображаем топ-10 лучших результатов
            await ShowLeaderboard();
            _tryingToConnectToServer.gameObject.SetActive(false);
            Debug.Log("Игрок уже залогинен");
            return;
        }

        try
        {
            // Логинимся анонимно (Unity автоматически использует Device ID для идентификации)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            Debug.Log($"Успешная аутентификация! Player ID: {AuthenticationService.Instance.PlayerId}");
            
            // Получаем лучший результат игрока
            await GetPlayerScore(LeaderboardId);
            
            // Получаем и отображаем топ-10 лучших результатов
            await ShowLeaderboard();
            _tryingToConnectToServer.gameObject.SetActive(false);
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

        // Отправка очков в таблицу лидеров
        public async Task SubmitScore(string leaderboardId, int score)
        {
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
                Debug.Log($"Очки отправлены: {score}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка отправки очков: {e.Message}");
            }
        }

        // Получение очков игрока
        public async Task<int> GetPlayerScore(string leaderboardId)
        {
            try
            {
                var scores = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
                Debug.Log($"Текущие очки: {scores.Score}, Позиция: {scores.Rank}");
                _currentScore = (int)scores.Score;
                PlayerPrefs.SetInt(GameInfo.UnityLeaderboardName, _currentScore);
                PlayerPrefs.Save();
                _scoreText.text = _currentScore.ToString();
                
                _positionText.text = (scores.Rank + 1).ToString();
                
                return (int)scores.Score;
            }
            catch (System.Exception e)
            {
                // Если у игрока еще нет записи в таблице лидеров, это нормально
                if (e.Message.Contains("could not be found"))
                {
                    Debug.Log("У игрока еще нет записи в таблице лидеров. Используем локальное значение.");
                    _currentScore = PlayerPrefs.GetInt(GameInfo.UnityLeaderboardName, 0);
                    _scoreText.text = _currentScore.ToString();
                    _positionText.text = "-";
                    return _currentScore;
                }
                
                Debug.LogError($"Ошибка получения очков: {e.Message}");
                return 0;
            }
        }

        // Получение топ-10 лучших результатов
        public async Task<LeaderboardScoresPage> GetTopScores()
        {
            try
            {
                var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                    LeaderboardId,
                    new GetScoresOptions { Offset = 0, Limit = 8 }
                );
                
                Debug.Log($"Получено {scoresResponse.Results.Count} результатов");
                return scoresResponse;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка получения топ-10: {e.Message}");
                return null;
            }
        }

        // Отображение таблицы лидеров
        public async Task ShowLeaderboard()
        {
            // Очищаем старые записи
            foreach (Transform item in _rowsParent)
            {
                Destroy(item.gameObject);
            }

            var leaderboardData = await GetTopScores();
            
            if (leaderboardData == null || leaderboardData.Results.Count == 0)
            {
                Debug.Log("Нет данных для отображения");
                return;
            }

            int rowNumber = 0;
            foreach (var entry in leaderboardData.Results)
            {
                rowNumber++;
                GameObject rowObject = InstantiateRow(rowNumber);
                TMP_Text[] texts = rowObject.GetComponentsInChildren<TMP_Text>();
                
                // texts[0] - позиция, texts[1] - очки
                texts[0].text = (entry.Rank + 1).ToString();
                texts[1].text = entry.Score.ToString();
            }
        }

        // Обертка для вызова из UI кнопок (UnityEvent не поддерживает async Task)
        public async void RefreshLeaderboard()
        {
            await ShowLeaderboard();
        }

        // Обновление очков игрока с отображением
        public async void RefreshPlayerScore()
        {
            int score = await GetPlayerScore(LeaderboardId);
            _scoreText.text = score.ToString();
        }

    private GameObject InstantiateRow(int number)
    {
        GameObject gameObject = new GameObject();
        if (number == 1)
        {
             gameObject = Instantiate(_firstRowPrefab, _rowsParent);
        }
        else if (number == 2)
        {
            gameObject = Instantiate(_secondThirdRowPrefab, _rowsParent);
        }
        else if (number == 3)
        {
            gameObject = Instantiate(_thirdRowPrefab, _rowsParent);
        }
        else
        {
            gameObject = Instantiate(_usualRowPrefab, _rowsParent);
        }
        return gameObject;
    }
}
