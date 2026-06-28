using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startButton;

    [SerializeField]
    private Button _leaderboardButton;
    
    [SerializeField]
    private float _bloodDisplayDelay = 0.3f;
    [SerializeField]
    private SoundsHandler _soundsHandler;
    
    private string _leaderboardSceneName = "Leaderboard";
    private void Start()
    {
        // Гарантируем что время идёт нормально при загрузке главного меню
        Time.timeScale = 1;
        _startButton.onClick.AddListener(StartGame);
        _leaderboardButton.onClick.AddListener(ShowLeaderboard);
    }

    private void ShowLeaderboard()
    {
        SceneManager.LoadScene(_leaderboardSceneName);
    }

    private void StartGame()
    {
        _startButton.interactable = false;
        _soundsHandler.PlaySplatter();
        StartCoroutine(StartGameWithDelay());
    }
    
    private IEnumerator StartGameWithDelay()
    {
        BloodSplatterManager.Instance.AddSplattersOnKill();
        yield return new WaitForSeconds(_bloodDisplayDelay);
        SceneManager.LoadScene(2);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(StartGame);
    }
}
