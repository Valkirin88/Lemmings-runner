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
    private GameObject _restartButtonObject;

    [SerializeField]
    private GameObject _gameOverObject;

    [SerializeField]
    private GameObject _finishObject;

    public GameState GameState;

    private void Start()
    {
        _restartButton.onClick.AddListener(RestartGame);
        _restartButtonObject.SetActive(false);
        GameState = GameState.Game;
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

    private void ShowPause()
    {
        throw new NotImplementedException();
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
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveListener(RestartGame);
    }
}
