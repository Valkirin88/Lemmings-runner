using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodSplatterManager : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Sprite[] _splatterSprites;
    [SerializeField] private Material _bloodMaterial;
    
    [Header("Splatter Settings")]
    [SerializeField] private float _fadeDelay = 1f;
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _minSize = 100f;
    [SerializeField] private float _maxSize = 300f;
    [SerializeField] private int _maxSplatters = 20;
    
    [Header("Splatters Per Kill")]
    [SerializeField] private int _minSplattersPerKill = 2;
    [SerializeField] private int _maxSplattersPerKill = 5;
    
    private List<BloodSplatter> _activeSplatters = new List<BloodSplatter>();
    
    private static BloodSplatterManager _instance;
    public static BloodSplatterManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddSplatter()
    {
        // Удаляем старые пятна, если достигли лимита
        if (_activeSplatters.Count >= _maxSplatters)
        {
            RemoveOldestSplatter();
        }
        
        // Создаём новое пятно
        GameObject splatterObj = new GameObject("BloodSplatter");
        splatterObj.transform.SetParent(_canvas.transform, false);
        
        // Добавляем Image компонент
        Image image = splatterObj.AddComponent<Image>();
        image.sprite = _splatterSprites[Random.Range(0, _splatterSprites.Length)];
        image.material = _bloodMaterial != null ? Instantiate(_bloodMaterial) : null;
        image.preserveAspect = true;
        image.raycastTarget = false;
        
        // Случайная позиция на экране
        RectTransform rect = splatterObj.GetComponent<RectTransform>();
        float screenWidth = _canvas.GetComponent<RectTransform>().rect.width;
        float screenHeight = _canvas.GetComponent<RectTransform>().rect.height;
        
        rect.anchoredPosition = new Vector2(
            Random.Range(-screenWidth / 2 + 100, screenWidth / 2 - 100),
            Random.Range(-screenHeight / 2 + 100, screenHeight / 2 - 100)
        );
        
        // Случайный размер
        float size = Random.Range(_minSize, _maxSize);
        rect.sizeDelta = new Vector2(size, size);
        
        // Случайный поворот
        rect.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        
        // Добавляем компонент управления пятном
        BloodSplatter splatter = splatterObj.AddComponent<BloodSplatter>();
        splatter.Initialize(image, _fadeDelay, _fadeDuration, OnSplatterFaded);
        
        _activeSplatters.Add(splatter);
    }
    
    public void AddSplattersOnKill()
    {
        int count = Random.Range(_minSplattersPerKill, _maxSplattersPerKill + 1);
        for (int i = 0; i < count; i++)
        {
            AddSplatter();
        }
    }
    
    public void AddMultipleSplatters(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddSplatter();
        }
    }
    
    private void RemoveOldestSplatter()
    {
        if (_activeSplatters.Count > 0)
        {
            BloodSplatter oldest = _activeSplatters[0];
            _activeSplatters.RemoveAt(0);
            if (oldest != null)
            {
                Destroy(oldest.gameObject);
            }
        }
    }
    
    private void OnSplatterFaded(BloodSplatter splatter)
    {
        _activeSplatters.Remove(splatter);
        Destroy(splatter.gameObject);
    }
    
    public void ClearAllSplatters()
    {
        foreach (var splatter in _activeSplatters)
        {
            if (splatter != null)
            {
                Destroy(splatter.gameObject);
            }
        }
        _activeSplatters.Clear();
    }
}
