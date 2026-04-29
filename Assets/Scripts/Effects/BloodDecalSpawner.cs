using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Спавнит пятна крови (декали) на поверхностях.
/// Singleton - вызывайте BloodDecalSpawner.Instance.SpawnDecalsAt(position) при убийстве.
/// </summary>
public class BloodDecalSpawner : MonoBehaviour
{
    [Header("Decal Settings")]
    [SerializeField] private GameObject _decalPrefab;
    [SerializeField] private Sprite[] _decalSprites;
    
    [Header("Size")]
    [SerializeField] private float _minSize = 0.5f;
    [SerializeField] private float _maxSize = 1.2f;
    
    [Header("Lifetime")]
    [SerializeField] private float _decalLifetime = 15f;
    [SerializeField] private float _fadeOutDuration = 3f;
    
    [Header("Spawn Settings")]
    [SerializeField] private int _maxDecals = 200;
    [SerializeField] private float _surfaceOffset = 0.02f;
    [SerializeField] private LayerMask _groundLayers = ~0;
    
    [Header("Decals Per Kill")]
    [SerializeField] private int _minDecalsPerKill = 8;
    [SerializeField] private int _maxDecalsPerKill = 15;
    
    private Queue<BloodDecal> _activeDecals = new Queue<BloodDecal>();
    
    private static BloodDecalSpawner _instance;
    public static BloodDecalSpawner Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null || _instance.gameObject == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    /// <summary>
    /// Создаёт пятна крови внутри прямоугольной зоны (вызывается из BloodZone).
    /// parent — объект, к которому привязываются декали (двигаются вместе с препятствием).
    /// </summary>
    public void SpawnDecalsInZone(Vector3 zoneCenter, Vector3 zoneSize, Transform parent = null)
    {
        int count = Random.Range(_minDecalsPerKill, _maxDecalsPerKill + 1);
        
        float halfX = zoneSize.x / 2f;
        float halfZ = zoneSize.z / 2f;
        float rayStartHeight = zoneCenter.y + zoneSize.y / 2f + 0.5f;
        
        int successCount = 0;
        for (int i = 0; i < count; i++)
        {
            // Случайная точка внутри зоны
            Vector3 rayStart = new Vector3(
                zoneCenter.x + Random.Range(-halfX, halfX),
                rayStartHeight,
                zoneCenter.z + Random.Range(-halfZ, halfZ)
            );
            
            // Рейкаст вниз чтобы найти поверхность
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, zoneSize.y + 2f, _groundLayers))
            {
                SpawnSingleDecal(hit.point, hit.normal, parent);
                successCount++;
            }
        }

    }

    /// <summary>
    /// Создаёт пятна крови в зоне без raycast. Удобно для trigger-зон вроде Bottom.
    /// </summary>
    public List<GameObject> SpawnDecalsOnSurface(Vector3 zoneCenter, Vector3 zoneSize, Vector3 surfaceNormal, Transform parent = null)
    {
        var spawnedDecals = new List<GameObject>();
        int count = Random.Range(_minDecalsPerKill, _maxDecalsPerKill + 1);

        float halfX = zoneSize.x / 2f;
        float halfZ = zoneSize.z / 2f;
        Vector3 normal = surfaceNormal.normalized;

        if (normal == Vector3.zero)
        {
            normal = Vector3.up;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 decalPosition = new Vector3(
                zoneCenter.x + Random.Range(-halfX, halfX),
                zoneCenter.y,
                zoneCenter.z + Random.Range(-halfZ, halfZ)
            );

            GameObject decal = SpawnSingleDecal(decalPosition, normal, parent);
            if (decal != null)
            {
                spawnedDecals.Add(decal);
            }
        }

        return spawnedDecals;
    }
    
    /// <summary>
    /// Создаёт одно пятно в указанной точке с указанной нормалью.
    /// parent — декаль становится ребёнком (двигается вместе с препятствием).
    /// </summary>
    public GameObject SpawnSingleDecal(Vector3 position, Vector3 normal, Transform parent = null)
    {
       
        // Удаляем старые декали если достигли лимита
        while (_activeDecals.Count >= _maxDecals)
        {
            RemoveOldestDecal();
        }
        
        // Создаём декаль
        GameObject decalObj;
        
        Vector3 decalPosition = position + normal * _surfaceOffset;

        if (_decalPrefab != null)
        {
            decalObj = Instantiate(_decalPrefab, decalPosition, Quaternion.identity, parent);
        }
        else
        {
            decalObj = CreateDefaultDecal();
            decalObj.transform.SetParent(parent);
            decalObj.transform.position = decalPosition;
        }
        
        // Поворачиваем декаль по нормали поверхности
        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.back, normal);
        Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);
        decalObj.transform.rotation = randomRotation * surfaceRotation;
        
        // Случайный размер в мировых единицах. Если декаль — child, компенсируем масштаб родителя.
        float size = Random.Range(_minSize, _maxSize);
        if (parent != null)
        {
            Vector3 parentScale = parent.lossyScale;
            float avgScale = (Mathf.Abs(parentScale.x) + Mathf.Abs(parentScale.y) + Mathf.Abs(parentScale.z)) / 3f;
            if (avgScale > 0.001f)
                size /= avgScale;
        }
        decalObj.transform.localScale = new Vector3(size, size, size);
        
        // Если есть спрайты, выбираем случайный
        if (_decalSprites != null && _decalSprites.Length > 0)
        {
            var spriteRenderer = decalObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = _decalSprites[Random.Range(0, _decalSprites.Length)];
            }
        }
        
        // Добавляем компонент управления декалью
        BloodDecal decal = decalObj.AddComponent<BloodDecal>();
        decal.Initialize(_decalLifetime, _fadeOutDuration, OnDecalExpired);
        
        _activeDecals.Enqueue(decal);
        return decalObj;
    }
    
    private GameObject CreateDefaultDecal()
    {
        GameObject decalObj = new GameObject("BloodDecal");
        
        var spriteRenderer = decalObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCircleSprite();
        spriteRenderer.color = new Color(0.839f, 0f, 0f, 1f); // #D60000 - непрозрачный красный
        spriteRenderer.sortingOrder = 1; // Поверх других объектов
        
        return decalObj;
    }
    
    private static Sprite _cachedCircleSprite;
    
    private Sprite CreateCircleSprite()
    {
        // Кэшируем спрайт чтобы не создавать каждый раз
        if (_cachedCircleSprite != null)
            return _cachedCircleSprite;
        
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        float center = size / 2f;
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance < radius - 1) // -1 для небольшого сглаживания края
                {
                    // Полностью непрозрачный пиксель
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        _cachedCircleSprite = Sprite.Create(
            texture, 
            new Rect(0, 0, size, size), 
            new Vector2(0.5f, 0.5f), 
            100f
        );
        
        return _cachedCircleSprite;
    }
    
    private void RemoveOldestDecal()
    {
        if (_activeDecals.Count > 0)
        {
            BloodDecal oldest = _activeDecals.Dequeue();
            if (oldest != null)
            {
                Destroy(oldest.gameObject);
            }
        }
    }
    
    private void OnDecalExpired(BloodDecal decal)
    {
        // Декаль сама себя удалит, просто убираем из очереди если она там есть
        // (она может быть уже удалена через RemoveOldestDecal)
    }
    
    /// <summary>
    /// Очистить все декали (например при перезапуске уровня)
    /// </summary>
    public void ClearAllDecals()
    {
        while (_activeDecals.Count > 0)
        {
            var decal = _activeDecals.Dequeue();
            if (decal != null)
            {
                Destroy(decal.gameObject);
            }
        }
    }
}

/// <summary>
/// Компонент для отдельной декали крови на поверхности
/// </summary>
public class BloodDecal : MonoBehaviour
{
    private float _lifetime;
    private float _fadeOutDuration;
    private float _timer;
    private System.Action<BloodDecal> _onExpired;
    
    private SpriteRenderer _spriteRenderer;
    private Material _cachedMaterial;
    private Color _originalColor;
    
    public void Initialize(float lifetime, float fadeOutDuration, System.Action<BloodDecal> onExpired)
    {
        _lifetime = lifetime;
        _fadeOutDuration = fadeOutDuration;
        _onExpired = onExpired;
        _timer = 0f;
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
        else
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                _cachedMaterial = renderer.material; // Кэшируем material один раз
                _originalColor = _cachedMaterial.color;
            }
        }
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        
        float fadeStartTime = _lifetime - _fadeOutDuration;
        
        // Ждём до начала затухания
        if (_timer < fadeStartTime)
            return;
        
        // Вычисляем прогресс затухания
        float fadeProgress = (_timer - fadeStartTime) / _fadeOutDuration;
        
        if (fadeProgress >= 1f)
        {
            _onExpired?.Invoke(this);
            Destroy(gameObject);
            return;
        }
        
        // Применяем затухание
        float alpha = Mathf.Lerp(_originalColor.a, 0f, fadeProgress);
        Color newColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);
        
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = newColor;
        }
        else if (_cachedMaterial != null)
        {
            _cachedMaterial.color = newColor;
        }
    }
}
