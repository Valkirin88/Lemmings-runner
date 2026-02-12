using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Спавнит префабы препятствий (IObstacle) и леммингов в заданной области с заданной вероятностью.
/// Область задаётся через инспектор (центр + размер бокса).
/// </summary>
public class RandomSpawner : MonoBehaviour
{
    [Header("Область спавна")]
    [SerializeField]
    [Tooltip("Центр области спавна (мировые координаты)")]
    private Vector3 _spawnAreaCenter = Vector3.zero;

    [SerializeField]
    [Tooltip("Размер области (ширина X, высота Y, глубина Z)")]
    private Vector3 _spawnAreaSize = new Vector3(10f, 2f, 20f);

    [Header("Префабы препятствий")]
    [SerializeField]
    [Tooltip("Список префабов с компонентом IObstacle")]
    private List<GameObject> _obstaclePrefabs = new List<GameObject>();

    [Header("Префабы леммингов")]
    [SerializeField]
    [Tooltip("Список префабов леммингов (LemmingView)")]
    private List<GameObject> _lemmingPrefabs = new List<GameObject>();

    [Header("Вероятности спавна")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность появления препятствия в каждой ячейке сетки")]
    private float _obstacleSpawnProbability = 0.3f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность появления лемминга в каждой ячейке сетки")]
    private float _lemmingSpawnProbability = 0.2f;

    [Header("Сетка спавна")]
    [SerializeField]
    [Tooltip("Размер ячейки сетки. Меньше = больше потенциальных точек спавна")]
    private float _gridCellSize = 2f;

    [Header("Частота появления — нарастающая сложность")]
    [SerializeField]
    [Tooltip("Источник очков (UIHandler). Если null — используется интервал при 0 очков")]
    private UIHandler _scoreProvider;

    [SerializeField]
    [Tooltip("Включить нарастающую сложность. При выключении — только Spawn при старте")]
    private bool _useScalingDifficulty = true;

    [SerializeField]
    [Tooltip("Пороги очков (0, 10, 50, 100...)")]
    private int[] _scoreThresholds = { 0, 10, 50, 100 };

    [SerializeField]
    [Tooltip("Интервал спавна в сек для каждого порога (2, 1, 0.5, 0.3...)")]
    private float[] _intervalSeconds = { 2f, 1f, 0.5f, 0.3f };

    [Header("Опции")]
    [SerializeField]
    [Tooltip("Родительский объект для спавненных объектов (для порядка в иерархии)")]
    private Transform _spawnedObjectsParent;

    [SerializeField]
    [Tooltip("Добавлять спавненные препятствия в ObstaclesSet (для звуков и т.д.)")]
    private ObstaclesSet _obstaclesSet;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Минимальная дистанция до существующих препятствий. 0 = проверка отключена")]
    private float _minDistanceToObstacles = 1.5f;

    private Coroutine _periodicSpawnCoroutine;

    private void Start()
    {
        Spawn();

        if (_useScalingDifficulty)
        {
            _periodicSpawnCoroutine = StartCoroutine(PeriodicSpawnCoroutine());
        }
    }

    private float GetSpawnInterval()
    {
        int score = _scoreProvider != null ? _scoreProvider.Score : 0;

        if (_scoreThresholds == null || _intervalSeconds == null || _scoreThresholds.Length == 0 || _intervalSeconds.Length == 0)
            return 2f;

        int n = Mathf.Min(_scoreThresholds.Length, _intervalSeconds.Length);
        if (n == 0) return 2f;

        if (score <= _scoreThresholds[0])
            return _intervalSeconds[0];
        if (score >= _scoreThresholds[n - 1])
            return _intervalSeconds[n - 1];

        for (int i = 0; i < n - 1; i++)
        {
            if (score >= _scoreThresholds[i] && score < _scoreThresholds[i + 1])
            {
                int scoreDelta = _scoreThresholds[i + 1] - _scoreThresholds[i];
                float t = scoreDelta > 0 ? (float)(score - _scoreThresholds[i]) / scoreDelta : 0f;
                return Mathf.Lerp(_intervalSeconds[i], _intervalSeconds[i + 1], t);
            }
        }

        return _intervalSeconds[n - 1];
    }

    private void OnDestroy()
    {
        if (_periodicSpawnCoroutine != null)
        {
            StopCoroutine(_periodicSpawnCoroutine);
        }
    }

    private IEnumerator PeriodicSpawnCoroutine()
    {
        while (true)
        {
            float interval = Mathf.Max(0.1f, GetSpawnInterval());
            yield return new WaitForSeconds(interval);
            SpawnSingle();
        }
    }

    /// <summary>
    /// Спавнит один или два объекта в случайных точках области.
    /// </summary>
    private void SpawnSingle()
    {
        bool spawnObstacle = _obstaclePrefabs.Count > 0 && Random.value < _obstacleSpawnProbability;
        bool spawnLemming = _lemmingPrefabs.Count > 0 && Random.value < _lemmingSpawnProbability;

        if (spawnObstacle)
            TrySpawnObstacle(GetRandomPositionAtLowerBound());

        if (spawnLemming)
            TrySpawnLemming(GetRandomPositionAtLowerBound());
    }

    private float LowerBoundY => _spawnAreaCenter.y - _spawnAreaSize.y * 0.5f;

    private Vector3 GetRandomPositionAtLowerBound()
    {
        float minX = _spawnAreaCenter.x - _spawnAreaSize.x * 0.5f;
        float minZ = _spawnAreaCenter.z - _spawnAreaSize.z * 0.5f;
        return new Vector3(
            minX + Random.Range(0f, _spawnAreaSize.x),
            LowerBoundY,
            minZ + Random.Range(0f, _spawnAreaSize.z)
        );
    }

    /// <summary>
    /// Запускает спавн препятствий и леммингов в заданной области.
    /// </summary>
    public void Spawn()
    {
        var bounds = new Bounds(_spawnAreaCenter, _spawnAreaSize);
        int cellsX = Mathf.Max(1, Mathf.FloorToInt(_spawnAreaSize.x / _gridCellSize));
        int cellsY = Mathf.Max(1, Mathf.FloorToInt(_spawnAreaSize.y / _gridCellSize));
        int cellsZ = Mathf.Max(1, Mathf.FloorToInt(_spawnAreaSize.z / _gridCellSize));

        Vector3 min = bounds.min;

        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    Vector3 cellCenter = min + new Vector3(
                        (x + 0.5f) * _gridCellSize,
                        (y + 0.5f) * _gridCellSize,
                        (z + 0.5f) * _gridCellSize
                    );

                    if (!bounds.Contains(cellCenter))
                        continue;

                    float offsetRange = _gridCellSize * 0.4f;
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-offsetRange, offsetRange),
                        Random.Range(-offsetRange, offsetRange),
                        Random.Range(-offsetRange, offsetRange)
                    );

                    bool spawnObstacle = _obstaclePrefabs.Count > 0 && Random.value < _obstacleSpawnProbability;
                    bool spawnLemming = _lemmingPrefabs.Count > 0 && Random.value < _lemmingSpawnProbability;

                    if (spawnObstacle)
                    {
                        Vector3 obstaclePos = new Vector3(cellCenter.x + randomOffset.x, LowerBoundY, cellCenter.z + randomOffset.z);
                        TrySpawnObstacle(obstaclePos);
                    }

                    if (spawnLemming)
                    {
                        Vector3 lemmingPos = new Vector3(cellCenter.x + randomOffset.x, LowerBoundY, cellCenter.z + randomOffset.z);
                        TrySpawnLemming(lemmingPos);
                    }
                }
            }
        }
    }

    private void TrySpawnObstacle(Vector3 position)
    {
        var prefab = _obstaclePrefabs[Random.Range(0, _obstaclePrefabs.Count)];
        if (prefab == null || prefab.GetComponentInChildren<IObstacle>() == null)
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит IObstacle, пропуск.");
            return;
        }

        Vector3 spawnPos = position;
        if (prefab.GetComponentInChildren<Bird>() != null)
            spawnPos.y = 3f;
        else if (prefab.GetComponentInChildren<Chipper>() != null)
            spawnPos.y += 0.7f;
        else if (prefab.GetComponentInChildren<WoodLog>() != null)
            spawnPos.y += 0.4f;

        if (_minDistanceToObstacles > 0f && WouldOverlapObstacle(spawnPos))
            return;

        var instance = Instantiate(prefab, position, Quaternion.identity, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        var pos = instance.transform.position;
        if (instance.GetComponentInChildren<Bird>() != null)
        {
            pos.y = 3f;
            instance.transform.position = pos;
        }
        else if (instance.GetComponentInChildren<Chipper>() != null)
        {
            pos.y += 0.7f;
            instance.transform.position = pos;
        }
        else if (instance.GetComponentInChildren<WoodLog>() != null)
        {
            pos.y += 0.4f;
            instance.transform.position = pos;
        }

        if (instance.GetComponentInChildren<CircularSaw>() != null || instance.GetComponentInChildren<CircularSawMoving>() != null)
        {
            if (Random.value < 0.5f)
                instance.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
        }

        if (_obstaclesSet != null && _obstaclesSet.Obstacles != null)
        {
            _obstaclesSet.Obstacles.Add(instance);
        }
    }

    private bool WouldOverlapObstacle(Vector3 position)
    {
        if (_obstaclesSet == null || _obstaclesSet.Obstacles == null) return false;

        float minDistSq = _minDistanceToObstacles * _minDistanceToObstacles;

        foreach (var obj in _obstaclesSet.Obstacles)
        {
            if (obj == null) continue;
            float distSq = (obj.transform.position - position).sqrMagnitude;
            if (distSq < minDistSq) return true;
        }
        return false;
    }

    private void TrySpawnLemming(Vector3 position)
    {
        var prefab = _lemmingPrefabs[Random.Range(0, _lemmingPrefabs.Count)];
        if (prefab == null || prefab.GetComponentInChildren<LemmingView>() == null)
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит LemmingView, пропуск.");
            return;
        }

        if (_minDistanceToObstacles > 0f && WouldOverlapObstacle(position))
            return;

        var instance = Instantiate(prefab, position, Quaternion.identity, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        var lemmingView = instance.GetComponentInChildren<LemmingView>();
        if (lemmingView != null)
        {
            lemmingView.IsRun = false;
            lemmingView.IsScroll = true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Gizmos.matrix = Matrix4x4.TRS(_spawnAreaCenter, Quaternion.identity, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, _spawnAreaSize);
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, _spawnAreaSize);
    }
#endif
}
