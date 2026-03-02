using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [FormerlySerializedAs("_currentQuantityText")] [SerializeField]
    private TMP_Text _currentScoreText;
   
    [SerializeField]
    private Button _restartButton;
    [SerializeField]
    private Button _resumeButton;
    [SerializeField]
    private Button _pauseButton;
    [SerializeField]
    private Button _mainMenuButton;
    
    [SerializeField]
    private GameObject _restartButtonObject;
    [SerializeField]
    private GameObject _mainMenuButtonObject;
    [SerializeField]
    private GameObject _pauseButtonObject;
    [SerializeField]
    private GameObject _resumeButtonObject;

    [SerializeField]
    private GameObject _pausePanel;
    
    [SerializeField]
    private GameObject _gameOverObject;

    [SerializeField]
    private GameObject _finishObject;

    
    
    public GameState GameState;
    
    private int _lastDisplayedQuantity = -1;
    private GameState _lastProcessedState;
    private int _currentLevel;
    
    private int _score;

    public int Score => _score;

    private void Start()
    {
        _restartButton.onClick.AddListener(RestartGame);
        _pauseButton.onClick.AddListener(ShowPause);
        _mainMenuButton.onClick.AddListener(ShowMainMenu);
        _resumeButton.onClick.AddListener(ResumeGame);
        
        _restartButtonObject.SetActive(false);
        GameState = GameState.Game;
        _currentLevel = SceneManager.GetActiveScene().buildIndex;
        ShowScore(_score);
    }

    private void ShowPause()
    {
        Debug.Log("Showing Pause Panel");
        GameState = GameState.Paused;
        _pausePanel.SetActive(true);
        Time.timeScale = 0;
        _mainMenuButtonObject.SetActive(true);
        _restartButtonObject.SetActive(true);
        _resumeButtonObject.SetActive(true);
    }

    private void ResumeGame()
    {
        GameState = GameState.Game;
        Time.timeScale = 1;
        _pausePanel.SetActive(false);
        _mainMenuButtonObject.SetActive(false);
        _restartButtonObject.SetActive(false);
        _resumeButtonObject.SetActive(false);
    }

    private void ShowMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void ShowScore(int score)
    {
        _score = score;
        _currentScoreText.text = Score.ToString();
    }

    public void ShowCurrentQuantity(int quantity)
    {
        // if (_lastDisplayedQuantity == quantity) return;
        // _lastDisplayedQuantity = quantity;
        // _currentScoreText.text = quantity.ToString();
    }

    private void Update()
    {
        if (_lastProcessedState != GameState)
        {
            _lastProcessedState = GameState;

            switch (GameState)
            {
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                case GameState.Finish:
                    ShowFinish();
                    break;
                case GameState.Paused:
                    ShowPause();
                    break;
            }
        }
    }
    private void ShowFinish()
    {
        _restartButtonObject.SetActive(true);
        _finishObject.SetActive(true);
    }

    private void ShowGameOver()
    {
        
        _restartButtonObject.SetActive(true);
        _gameOverObject.SetActive(true);
    }

    private void RestartGame()
    {
        GameState = GameState.Game; // Чтобы Update() не вызвал ShowPause() и не сбросил timeScale
        Time.timeScale = 1;
        SceneManager.LoadScene(_currentLevel);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(RestartGame);
        _pauseButton.onClick.RemoveListener(ShowPause);
        _mainMenuButton.onClick.RemoveListener(ShowMainMenu);
        _resumeButton.onClick.RemoveListener(ResumeGame);
    }
}
