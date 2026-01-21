using System;
using UnityEngine;
using UnityEngine.UI;

public class BloodSplatter : MonoBehaviour
{
    private Image _image;
    private float _fadeDelay;
    private float _fadeDuration;
    private float _timer;
    private bool _isFading;
    private Action<BloodSplatter> _onFaded;
    private Color _originalColor;
    
    public void Initialize(Image image, float fadeDelay, float fadeDuration, Action<BloodSplatter> onFaded)
    {
        _image = image;
        _fadeDelay = fadeDelay;
        _fadeDuration = fadeDuration;
        _onFaded = onFaded;
        _timer = 0f;
        _isFading = false;
        _originalColor = _image.color;
        
        // Начальное появление - плавное
        _image.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        
        // Фаза появления (первые 0.3 секунды)
        if (_timer < 0.3f)
        {
            float appearProgress = _timer / 0.3f;
            _image.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, appearProgress);
            return;
        }
        
        // Ожидание перед затуханием
        if (_timer < _fadeDelay)
        {
            _image.color = _originalColor;
            return;
        }
        
        // Фаза затухания
        if (!_isFading)
        {
            _isFading = true;
        }
        
        float fadeProgress = (_timer - _fadeDelay) / _fadeDuration;
        
        if (fadeProgress >= 1f)
        {
            _onFaded?.Invoke(this);
            return;
        }
        
        float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
        _image.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
    }
}
