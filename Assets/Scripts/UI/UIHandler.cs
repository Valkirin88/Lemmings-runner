using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public event Action OnResumedAfterGameOver;
    public event Action OnContinueGameplay;

    [HideInInspector]
    public bool IsAdsNeeded;

    public Func<bool> IsAdReadyCheck;
    
    [SerializeField]
    private TMP_Text _currentScoreText;

    [SerializeField]
    private TMP_Text _lemmingsNumbeText;

    [SerializeField]
    private RectTransform _lemmingsNumberIcon;
    [SerializeField]
    private float _lemmingsIconOffsetWhenOver9 = 30f;
    
    [SerializeField]
    private GameObject _pauseTextObject;

    [SerializeField]
    private TMP_Text _lemmingsQuantityText;
    
    [SerializeField]
    private TMP_Text _totalScoreText;
    
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
    private GameObject _manageButtonsObject;

    [SerializeField]
    private GameObject _adsImageObject;
    
    [SerializeField]
    private GameObject _pausePanel;
    
    [SerializeField]
    private GameObject _gameOverObject;


    [Header("Тряска очков")]
    [SerializeField]
    [Tooltip("Минимальный прирост за раз, при котором очки трясутся (трясёт при приросте > этого значения)")]
    private int _scoreShakeThreshold = 4;
    [SerializeField]
    private float _scoreShakeDuration = 0.4f;
    [SerializeField]
    private float _scoreShakeStrength = 25f;
    [SerializeField]
    private int _scoreShakeVibrato = 18;

    public GameState GameState;
    
    private int _lastDisplayedQuantity = -1;
    private GameState _lastProcessedState;
    private int _currentLevel;
    
    private int _score;
    private int _spawnScore;
    private Vector2 _scoreTextBasePosition;
    private bool _scoreBasePositionCaptured;
    private Tween _scoreShakeTween;
    private int _playAttempts;
    private bool _waitingContinueAfterAd;
    private Vector2 _lemmingsIconBasePosition;
    private bool _lemmingsIconBaseCaptured;

    public int Score => _score;
    public int SpawnScore => _spawnScore;

    private void Start()
    {
        _restartButton.onClick.AddListener(RestartGame);
        _pauseButton.onClick.AddListener(ShowPause);
        _mainMenuButton.onClick.AddListener(ShowMainMenu);
        _resumeButton.onClick.AddListener(ResumeGame);
        
        _restartButtonObject.SetActive(false);
        GameState = GameState.Game;
        _currentLevel = SceneManager.GetActiveScene().buildIndex;
        ShowScore(_score, 0, shake: false);
        _playAttempts = GameInfo.PlayAttempts;
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
        if (_resumeButton != null)
            _resumeButton.interactable = true;
        if (_adsImageObject != null)
            _adsImageObject.SetActive(false);
        _manageButtonsObject.SetActive(false);
        _pauseTextObject.SetActive(true);
    }

    private void ResumeGame()
    {
        if (GameState == GameState.GameOver)
        {
            if (IsAdsNeeded)
            {
                if (!(IsAdReadyCheck?.Invoke() ?? false))
                    return;

                OnResumedAfterGameOver?.Invoke();
                return;
            }

            if (!_waitingContinueAfterAd)
                return;

            OnContinueGameplay?.Invoke();
        }

        _waitingContinueAfterAd = false;

        GameState = GameState.Game;
        Time.timeScale = 1;
        _pausePanel.SetActive(false);
        _mainMenuButtonObject.SetActive(false);
        _restartButtonObject.SetActive(false);
        _resumeButtonObject.SetActive(false);
        _manageButtonsObject.SetActive(true);
        _pauseTextObject.SetActive(false);
        _gameOverObject.SetActive(false);
        if (_currentScoreText != null)
            _currentScoreText.gameObject.SetActive(true);
    }

    public void OnAdsRewardGranted()
    {
        _playAttempts--;
        _waitingContinueAfterAd = true;
    }

    private void ShowMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void SetSpawnScore(int spawnScore)
    {
        _spawnScore = spawnScore;
    }

    public void ShowScore(int score, int added, bool shake)
    {
        _score = score;
        _currentScoreText.text = Score.ToString();

        if (shake && added > _scoreShakeThreshold)
            ShakeScoreText();
    }

    public void ShowLemmingsNumber(int quantity)
    {
        if (_lemmingsNumbeText == null) return;
        _lemmingsNumbeText.text = "x" + quantity;
        UpdateLemmingsIconPosition(quantity);
    }

    private void UpdateLemmingsIconPosition(int quantity)
    {
        if (_lemmingsNumberIcon == null)
            return;

        if (!_lemmingsIconBaseCaptured)
        {
            _lemmingsIconBasePosition = _lemmingsNumberIcon.anchoredPosition;
            _lemmingsIconBaseCaptured = true;
        }

        float offsetX = quantity > 9 ? _lemmingsIconOffsetWhenOver9 : 0f;
        _lemmingsNumberIcon.anchoredPosition = _lemmingsIconBasePosition + new Vector2(offsetX, 0f);
    }

    private void ShakeScoreText()
    {
        if (_currentScoreText == null)
            return;

        var rectTransform = _currentScoreText.rectTransform;

        if (!_scoreBasePositionCaptured)
        {
            _scoreTextBasePosition = rectTransform.anchoredPosition;
            _scoreBasePositionCaptured = true;
        }

        _scoreShakeTween?.Kill();
        rectTransform.anchoredPosition = _scoreTextBasePosition;

        _scoreShakeTween = rectTransform
            .DOShakeAnchorPos(_scoreShakeDuration, _scoreShakeStrength, _scoreShakeVibrato, 90f, false, true)
            .SetUpdate(true)
            .OnComplete(() => rectTransform.anchoredPosition = _scoreTextBasePosition)
            .OnKill(() => rectTransform.anchoredPosition = _scoreTextBasePosition);
    }

    public void ShowCurrentQuantity(int quantity)
    {
        ShowLemmingsNumber(quantity);

        if (_lemmingsQuantityText == null) return;
        if (_lastDisplayedQuantity == quantity) return;
        _lastDisplayedQuantity = quantity;
        _lemmingsQuantityText.text = quantity.ToString();
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
        // _restartButtonObject.SetActive(true);
        // _finishObject.SetActive(true);
    }

    private void ShowGameOver()
    {
        Time.timeScale = 0;
        _restartButtonObject.SetActive(true);
        _mainMenuButtonObject.SetActive(true);
        _gameOverObject.SetActive(true);

        _waitingContinueAfterAd = false;
        IsAdsNeeded = _playAttempts > 0 && GameInfo.IsAdsOn;
        RefreshGameOverContinueButton();

        _pausePanel.SetActive(true);
        _manageButtonsObject.SetActive(false);
        _totalScoreText.text = _score.ToString();
        _currentScoreText.gameObject.SetActive(false);
        
        _totalScoreText.rectTransform.SetAsLastSibling();
        _restartButtonObject.transform.parent.SetAsLastSibling();
        
       
    }

    public void RefreshGameOverContinueButton()
    {
        if (GameState != GameState.GameOver)
            return;

        bool adReady = IsAdReadyCheck?.Invoke() ?? false;
        bool showForAd = _playAttempts > 0 && IsAdsNeeded && adReady;
        bool showForContinue = _waitingContinueAfterAd;
        bool showResume = showForAd || showForContinue;

        _resumeButtonObject.SetActive(showResume);
        if (_adsImageObject != null)
            _adsImageObject.SetActive(showForAd);

        if (_resumeButton != null)
            _resumeButton.interactable = showResume;
    }

    private void RestartGame()
    {
        GameState = GameState.Game; // Чтобы Update() не вызвал ShowPause() и не сбросил timeScale
        Time.timeScale = 1;
        SceneManager.LoadScene(_currentLevel);
    }

    private void OnDestroy()
    {
        _scoreShakeTween?.Kill();

        _restartButton.onClick.RemoveListener(RestartGame);
        _pauseButton.onClick.RemoveListener(ShowPause);
        _mainMenuButton.onClick.RemoveListener(ShowMainMenu);
        _resumeButton.onClick.RemoveListener(ResumeGame);
    }
}
