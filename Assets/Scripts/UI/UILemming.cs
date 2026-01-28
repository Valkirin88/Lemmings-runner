using System;
using UnityEngine;
using UnityEngine.UI;

public class UILemming : MonoBehaviour
{
    [SerializeField] 
    private Transform _targetTransform;
    
    [SerializeField]
    private Vector2 _renderTextureSize = new Vector2(256, 256);
    
    [SerializeField]
    private float _cameraDistance = 2f;
    
    [SerializeField]
    private Vector3 _cameraOffset = new Vector3(0, 0.5f, 0);
    
    private RectTransform _rectTransform;
    private Camera _mainCamera;
    private Camera _renderCamera;
    private RenderTexture _renderTexture;
    private RawImage _rawImage;
    
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
        
        SetupRenderTexture();
        SetupRenderCamera();
        SetupRawImage();
    }
    
    private void SetupRenderTexture()
    {
        _renderTexture = new RenderTexture((int)_renderTextureSize.x, (int)_renderTextureSize.y, 16);
        _renderTexture.Create();
    }
    
    private void SetupRenderCamera()
    {
        GameObject cameraObj = new GameObject("LemmingRenderCamera");
        _renderCamera = cameraObj.AddComponent<Camera>();
        _renderCamera.targetTexture = _renderTexture;
        _renderCamera.clearFlags = CameraClearFlags.SolidColor;
        _renderCamera.backgroundColor = new Color(0, 0, 0, 0); // Прозрачный фон
        _renderCamera.cullingMask = 1 << _targetTransform.gameObject.layer; // Рендерим только слой лемминга
        _renderCamera.fieldOfView = 30f;
        _renderCamera.nearClipPlane = 0.1f;
        _renderCamera.farClipPlane = 100f;
    }
    
    private void SetupRawImage()
    {
        _rawImage = GetComponent<RawImage>();
        if (_rawImage == null)
        {
            _rawImage = gameObject.AddComponent<RawImage>();
        }
        _rawImage.texture = _renderTexture;
    }
    
    private void LateUpdate()
    {
        if (_targetTransform == null || _mainCamera == null)
            return;
            
        UpdatePosition();
        UpdateRenderCamera();
    }
    
    private void UpdatePosition()
    {
        // Конвертируем мировую позицию в экранную
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(_targetTransform.position);
        
        // Для Screen Space - Overlay используем экранные координаты, сохраняя z
        _rectTransform.position = new Vector3(screenPosition.x, screenPosition.y, _rectTransform.position.z);
    }
    
    private void UpdateRenderCamera()
    {
        if (_renderCamera == null || _mainCamera == null)
            return;
            
        // Позиционируем камеру чтобы она смотрела на лемминга с того же направления что и основная камера
        Vector3 targetPosition = _targetTransform.position + _cameraOffset;
        Vector3 directionFromMainCamera = (_mainCamera.transform.position - targetPosition).normalized;
        
        _renderCamera.transform.position = targetPosition + directionFromMainCamera * _cameraDistance;
        _renderCamera.transform.LookAt(targetPosition);
    }
    
    public void SetTarget(Transform target)
    {
        _targetTransform = target;
        
        if (_renderCamera != null)
        {
            _renderCamera.cullingMask = 1 << target.gameObject.layer;
        }
    }
    
    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
        
        if (_renderCamera != null)
        {
            Destroy(_renderCamera.gameObject);
        }
    }
}
