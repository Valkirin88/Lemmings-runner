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

    [Header("Частота появления")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("Спавн каждые N секунд. 0 = только при старте сцены")]
    private float _spawnIntervalSeconds = 0f;

    [Header("Опции")]
    [SerializeField]
    [Tooltip("Родительский объект для спавненных объектов (для порядка в иерархии)")]
    private Transform _spawnedObjectsParent;

    [SerializeField]
    [Tooltip("Добавлять спавненные препятствия в ObstaclesSet (для звуков и т.д.)")]
    private ObstaclesSet _obstaclesSet;

    private Coroutine _periodicSpawnCoroutine;

    private void Start()
    {
        Spawn();

        if (_spawnIntervalSeconds > 0f)
        {
            _periodicSpawnCoroutine = StartCoroutine(PeriodicSpawnCoroutine());
        }
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
        float interval = Mathf.Max(0.1f, _spawnIntervalSeconds);
        while (true)
        {
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
        if (prefab == null || !prefab.TryGetComponentInChildren<IObstacle>(out _))
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит IObstacle, пропуск.");
            return;
        }

        var instance = Instantiate(prefab, position, Quaternion.identity, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        var pos = instance.transform.position;
        if (instance.TryGetComponentInChildren<Bird>(out _))
        {
            pos.y = 3f;
            instance.transform.position = pos;
        }
        else if (instance.TryGetComponentInChildren<Chipper>(out _))
        {
            pos.y += 0.7f;
            instance.transform.position = pos;
        }

        if (_obstaclesSet != null && _obstaclesSet.Obstacles != null)
        {
            _obstaclesSet.Obstacles.Add(instance);
        }
    }

    private void TrySpawnLemming(Vector3 position)
    {
        var prefab = _lemmingPrefabs[Random.Range(0, _lemmingPrefabs.Count)];
        if (prefab == null || !prefab.TryGetComponentInChildren<LemmingView>(out _))
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит LemmingView, пропуск.");
            return;
        }

        var instance = Instantiate(prefab, position, Quaternion.identity, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        var lemmingView = instance.GetComponentInChildren<LemmingView>();
        if (lemmingView != null)
        {
            lemmingView.IsRun = false;
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
