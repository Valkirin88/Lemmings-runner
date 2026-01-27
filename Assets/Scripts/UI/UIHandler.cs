using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _currentQuantityText;
   
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

    private void Start()
    {
        _restartButton.onClick.AddListener(RestartGame);
        _pauseButton.onClick.AddListener(ShowPause);
        _mainMenuButton.onClick.AddListener(ShowMainMenu);
        _resumeButton.onClick.AddListener(ResumeGame);
        
        _restartButtonObject.SetActive(false);
        GameState = GameState.Game;
    }

    private void ShowPause()
    {
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
        SceneManager.LoadScene(0);
    }

    public void ShowCurrentQuantity(int quantity)
    {
        _currentQuantityText.text = quantity.ToString() + "/12";
    }

    private void Update()
    {
        switch (GameState)
        {
            case GameState.GameOver:
                ShowGameOver();
                break;
            case GameState.Finish:
                ShowFinish();
                break;
            case GameState.Game:
                
                break;
            case GameState.Paused:
                ShowPause();
                break;
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
        SceneManager.LoadScene(1);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(RestartGame);
        _pauseButton.onClick.RemoveListener(ShowPause);
        _mainMenuButton.onClick.RemoveListener(ShowMainMenu);
        _resumeButton.onClick.RemoveListener(ResumeGame);
    }
}
