using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Запись для спавна препятствия: префаб + минимальное кол-во очков для разблокировки (если фазы не заданы).
/// </summary>
[System.Serializable]
public class ObstacleSpawnEntry
{
    [Tooltip("Префаб препятствия (IObstacle)")]
    public GameObject prefab;

    [Tooltip("Минимальное кол-во очков, при котором это препятствие может появляться")]
    public int minScoreToUnlock = 0;
}

/// <summary>
/// Фаза: в диапазоне очков могут спавниться только перечисленные препятствия.
/// </summary>
[System.Serializable]
public class ObstacleScorePhase
{
    [Tooltip("С какого счёта (включительно)")]
    public int minScore;

    [Tooltip("До какого счёта (включительно). 0 — до конца игры")]
    public int maxScore;

    [Tooltip("Префабы препятствий, доступные только в этой фазе")]
    public List<GameObject> obstacles = new List<GameObject>();
}

/// <summary>
/// Спавнит префабы препятствий (IObstacle) и леммингов в заданной области с заданной вероятностью.
/// Область задаётся через инспектор (центр + размер бокса).
/// Препятствия: в заданных фазах очков — только выбранные; вне фаз — по minScoreToUnlock у каждой записи.
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
    [Tooltip("Список препятствий: префаб + мин. очки. Используется, если список фаз пуст")]
    private List<ObstacleSpawnEntry> _obstacleSpawnEntries = new List<ObstacleSpawnEntry>();

    [SerializeField]
    [Tooltip("Фазы по очкам: в диапазоне — только выбранные препятствия. Вне всех фаз — список выше по minScoreToUnlock. Если пусто — только список выше")]
    private List<ObstacleScorePhase> _obstacleScorePhases = new List<ObstacleScorePhase>();

    [Header("Префабы леммингов")]
    [SerializeField]
    [Tooltip("Список префабов леммингов (LemmingView)")]
    private List<GameObject> _lemmingPrefabs = new List<GameObject>();
    public IReadOnlyList<GameObject> LemmingPrefabs => _lemmingPrefabs;

    [Header("Префабы бонусов")]
    [SerializeField]
    [Tooltip("Список префабов с компонентом ScoreBonus")]
    private List<GameObject> _bonusPrefabs = new List<GameObject>();

    [Header("Вероятности спавна")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность появления препятствия в каждой ячейке сетки")]
    private float _obstacleSpawnProbability = 0.3f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Базовая вероятность появления лемминга в каждой ячейке сетки. " +
             "Если задан массив _lemmingProbabilityByScore — это значение игнорируется.")]
    private float _lemmingSpawnProbability = 0.2f;

    [SerializeField]
    [Tooltip("Пороги очков для шкалы вероятности появления лемминга (например 0, 10, 50, 100). " +
             "Длина должна совпадать с _lemmingProbabilityByScore. Оставь массивы пустыми, чтобы использовать _lemmingSpawnProbability.")]
    private int[] _lemmingProbabilityScoreThresholds = new int[0];

    [SerializeField]
    [Tooltip("Вероятность спавна лемминга для каждого порога (0..1). Между порогами интерполируется линейно.")]
    private float[] _lemmingProbabilityByScore = new float[0];

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность появления бонуса в каждой ячейке сетки")]
    private float _bonusSpawnProbability = 0.15f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность спавна двух препятствий сразу вместо одного")]
    private float _doubleObstacleProbability = 0.25f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Вероятность спавна третьего препятствия при срабатывании двойного спавна")]
    private float _tripleObstacleProbability = 0.1f;

    [SerializeField]
    [Tooltip("Смещение второго препятствия при двойном спавне (X, Y, Z)")]
    private Vector3 _doubleObstacleOffset = new Vector3(0f, 0f, 2.5f);

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
    [Tooltip("Задаётся из Entry Point через Initialize(). Оставь пустым, если прокидываешь из Entry Point.")]
    private ObstaclesSet _obstaclesSet;

    /// <summary> Прокинуть ObstaclesSet из Entry Point (вызывается в Awake). </summary>
    public void Initialize(ObstaclesSet obstaclesSet) => _obstaclesSet = obstaclesSet;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Минимальная дистанция до существующих препятствий. 0 = проверка отключена")]
    private float _minDistanceToObstacles = 1.5f;

    [Header("Fan — только по краям платформы")]
    [SerializeField]
    [Tooltip("Пустой объект на левом краю платформы (берётся position.x)")]
    private Transform _fanLeftEdge;
    [SerializeField]
    [Tooltip("Пустой объект на правом краю платформы (берётся position.x)")]
    private Transform _fanRightEdge;

    private Coroutine _periodicSpawnCoroutine;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_obstacleSpawnEntries == null)
            _obstacleSpawnEntries = new List<ObstacleSpawnEntry>();
        if (_obstacleScorePhases == null)
            _obstacleScorePhases = new List<ObstacleScorePhase>();
    }
#endif

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
        return LerpValueByScore(_scoreThresholds, _intervalSeconds, fallback: 2f);
    }

    /// <summary>
    /// Возвращает вероятность спавна лемминга. Если массивы порогов/вероятностей заданы — берёт значение по очкам,
    /// иначе возвращает базовое _lemmingSpawnProbability.
    /// </summary>
    private float GetLemmingSpawnProbability()
    {
        if (_lemmingProbabilityScoreThresholds != null && _lemmingProbabilityByScore != null
            && _lemmingProbabilityScoreThresholds.Length > 0 && _lemmingProbabilityByScore.Length > 0)
        {
            float p = LerpValueByScore(_lemmingProbabilityScoreThresholds, _lemmingProbabilityByScore, _lemmingSpawnProbability);
            return Mathf.Clamp01(p);
        }
        return _lemmingSpawnProbability;
    }

    /// <summary>
    /// Линейно интерполирует значение из массива values по текущему счёту, опираясь на массив thresholds.
    /// До первого порога — values[0], после последнего — values[n-1].
    /// </summary>
    private float LerpValueByScore(int[] thresholds, float[] values, float fallback)
    {
        if (thresholds == null || values == null || thresholds.Length == 0 || values.Length == 0)
            return fallback;

        int score = _scoreProvider != null ? _scoreProvider.Score : 0;

        int n = Mathf.Min(thresholds.Length, values.Length);
        if (n == 0) return fallback;

        if (score <= thresholds[0])
            return values[0];
        if (score >= thresholds[n - 1])
            return values[n - 1];

        for (int i = 0; i < n - 1; i++)
        {
            if (score >= thresholds[i] && score < thresholds[i + 1])
            {
                int scoreDelta = thresholds[i + 1] - thresholds[i];
                float t = scoreDelta > 0 ? (float)(score - thresholds[i]) / scoreDelta : 0f;
                return Mathf.Lerp(values[i], values[i + 1], t);
            }
        }

        return values[n - 1];
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
    /// Префабы препятствий при текущем счёте: активная фаза — только её список; иначе — minScoreToUnlock.
    /// </summary>
    private List<GameObject> GetAvailableObstaclePrefabs()
    {
        int score = _scoreProvider != null ? _scoreProvider.Score : 0;

        if (_obstacleScorePhases != null && _obstacleScorePhases.Count > 0)
        {
            var phase = GetActiveScorePhase(score);
            if (phase != null)
                return GetObstaclePrefabsFromPhase(phase);
        }

        return GetObstaclePrefabsFromEntries(score);
    }

    private List<GameObject> GetObstaclePrefabsFromPhase(ObstacleScorePhase phase)
    {
        var list = new List<GameObject>();
        if (phase?.obstacles == null)
            return list;

        foreach (var prefab in phase.obstacles)
        {
            if (prefab != null && prefab.GetComponentInChildren<IObstacle>() != null)
                list.Add(prefab);
        }

        return list;
    }

    private List<GameObject> GetObstaclePrefabsFromEntries(int score)
    {
        var list = new List<GameObject>();
        if (_obstacleSpawnEntries == null)
            return list;

        foreach (var entry in _obstacleSpawnEntries)
        {
            if (entry?.prefab == null || entry.prefab.GetComponentInChildren<IObstacle>() == null)
                continue;
            if (score < entry.minScoreToUnlock)
                continue;
            list.Add(entry.prefab);
        }

        return list;
    }

    private ObstacleScorePhase GetActiveScorePhase(int score)
    {
        ObstacleScorePhase best = null;
        foreach (var phase in _obstacleScorePhases)
        {
            if (phase == null)
                continue;
            if (score < phase.minScore)
                continue;
            if (phase.maxScore > 0 && score > phase.maxScore)
                continue;
            if (best == null || phase.minScore > best.minScore)
                best = phase;
        }

        return best;
    }

    /// <summary>
    /// Спавнит один, два или три объекта в случайных точках области.
    /// </summary>
    private void SpawnSingle()
    {
        var availableObstacles = GetAvailableObstaclePrefabs();
        float lemmingProbability = GetLemmingSpawnProbability();
        bool spawnObstacle = availableObstacles.Count > 0 && Random.value < _obstacleSpawnProbability;
        bool spawnLemming = _lemmingPrefabs.Count > 0 && Random.value < lemmingProbability;
        bool spawnBonus = _bonusPrefabs.Count > 0 && Random.value < _bonusSpawnProbability;

        if (spawnObstacle)
            TrySpawnObstacleWithDouble(GetRandomPositionAtLowerBound());

        if (spawnLemming)
            TrySpawnLemming(GetRandomPositionAtLowerBound());

        if (spawnBonus)
            TrySpawnBonus(GetRandomPositionAtLowerBound());
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
        var availableObstacles = GetAvailableObstaclePrefabs();
        bool anyObstacleAvailable = availableObstacles.Count > 0;
        float lemmingProbability = GetLemmingSpawnProbability();

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

                    bool spawnObstacle = anyObstacleAvailable && Random.value < _obstacleSpawnProbability;
                    bool spawnLemming = _lemmingPrefabs.Count > 0 && Random.value < lemmingProbability;
                    bool spawnBonus = _bonusPrefabs.Count > 0 && Random.value < _bonusSpawnProbability;

                    if (spawnObstacle)
                    {
                        Vector3 obstaclePos = new Vector3(cellCenter.x + randomOffset.x, LowerBoundY, cellCenter.z + randomOffset.z);
                        TrySpawnObstacleWithDouble(obstaclePos);
                    }

                    if (spawnLemming)
                    {
                        Vector3 lemmingPos = new Vector3(cellCenter.x + randomOffset.x, LowerBoundY, cellCenter.z + randomOffset.z);
                        TrySpawnLemming(lemmingPos);
                    }

                    if (spawnBonus)
                    {
                        Vector3 bonusPos = new Vector3(cellCenter.x + randomOffset.x, LowerBoundY, cellCenter.z + randomOffset.z);
                        TrySpawnBonus(bonusPos);
                    }
                }
            }
        }
    }

    private void TrySpawnObstacleWithDouble(Vector3 position)
    {
        if (!TrySpawnObstacle(position, out Vector3 firstSpawnedPosition, out bool firstIsBonfire))
            return;

        if (Random.value >= _doubleObstacleProbability)
            return;

        Vector3 secondPosition = firstIsBonfire
            ? firstSpawnedPosition + new Vector3(0f, 0f, _doubleObstacleOffset.z)
            : position + _doubleObstacleOffset;

        secondPosition.y = LowerBoundY;
        if (!TrySpawnObstacle(secondPosition, out Vector3 secondSpawnedPosition, out bool secondIsBonfire, firstSpawnedPosition))
            return;

        if (Random.value >= _tripleObstacleProbability)
            return;

        Vector3 thirdPosition = secondIsBonfire
            ? secondSpawnedPosition + new Vector3(0f, 0f, _doubleObstacleOffset.z)
            : secondPosition + _doubleObstacleOffset;

        thirdPosition.y = LowerBoundY;
        TrySpawnObstacle(thirdPosition, out _, out _, secondSpawnedPosition);
    }

    private Bounds GetSpawnBounds() => new Bounds(_spawnAreaCenter, _spawnAreaSize);

    private bool IsInsideSpawnAreaXZ(Vector3 position)
    {
        Bounds bounds = GetSpawnBounds();
        return position.x >= bounds.min.x && position.x <= bounds.max.x
               && position.z >= bounds.min.z && position.z <= bounds.max.z;
    }

    private bool IsFarEnoughFrom(Vector3 position, Vector3? other)
    {
        if (!other.HasValue || _minDistanceToObstacles <= 0f)
            return true;

        float dx = position.x - other.Value.x;
        float dz = position.z - other.Value.z;
        return dx * dx + dz * dz >= _minDistanceToObstacles * _minDistanceToObstacles;
    }

    private bool IsValidObstacleSpawnPosition(Vector3 position, Vector3? keepAwayFrom, bool onlyCheckDistanceToPrevious)
    {
        if (!IsInsideSpawnAreaXZ(position))
            return false;

        if (!onlyCheckDistanceToPrevious && WouldOverlapObstacle(position))
            return false;

        if (keepAwayFrom.HasValue && !IsFarEnoughFrom(position, keepAwayFrom))
            return false;

        return true;
    }

    private bool TryFindNearestGridSpawnPosition(Vector3 preferred, Vector3? keepAwayFrom, bool onlyCheckDistanceToPrevious, out Vector3 result)
    {
        var candidates = new List<Vector3>();
        Bounds bounds = GetSpawnBounds();
        int cellsX = Mathf.Max(1, Mathf.FloorToInt(_spawnAreaSize.x / _gridCellSize));
        int cellsZ = Mathf.Max(1, Mathf.FloorToInt(_spawnAreaSize.z / _gridCellSize));
        Vector3 min = bounds.min;

        for (int x = 0; x < cellsX; x++)
        {
            for (int z = 0; z < cellsZ; z++)
            {
                Vector3 cellCenter = min + new Vector3(
                    (x + 0.5f) * _gridCellSize,
                    0f,
                    (z + 0.5f) * _gridCellSize);

                Vector3 candidate = new Vector3(cellCenter.x, LowerBoundY, cellCenter.z);
                if (!IsInsideSpawnAreaXZ(candidate))
                    continue;

                candidates.Add(candidate);
            }
        }

        candidates.Sort((a, b) =>
        {
            float da = (a.x - preferred.x) * (a.x - preferred.x) + (a.z - preferred.z) * (a.z - preferred.z);
            float db = (b.x - preferred.x) * (b.x - preferred.x) + (b.z - preferred.z) * (b.z - preferred.z);
            return da.CompareTo(db);
        });

        foreach (var candidate in candidates)
        {
            if (!IsValidObstacleSpawnPosition(candidate, keepAwayFrom, onlyCheckDistanceToPrevious))
                continue;

            result = candidate;
            return true;
        }

        result = preferred;
        return false;
    }

    private bool TrySpawnObstacle(Vector3 position, out Vector3 spawnedPosition, out bool isBonfire, Vector3? keepAwayFrom = null)
    {
        spawnedPosition = position;
        isBonfire = false;

        var available = GetAvailableObstaclePrefabs();
        if (available.Count == 0) return false;

        var prefab = available[Random.Range(0, available.Count)];
        if (prefab == null || prefab.GetComponentInChildren<IObstacle>() == null)
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит IObstacle, пропуск.");
            return false;
        }

        isBonfire = prefab.GetComponentInChildren<Bonfire>() != null;

        bool isFan = prefab.GetComponentInChildren<Fan>() != null;
        Vector3 spawnPos = position;
        spawnPos.y = LowerBoundY;

        bool fanSpawnOnRight = false;
        if (isFan && _fanLeftEdge != null && _fanRightEdge != null)
        {
            fanSpawnOnRight = Random.value < 0.5f;
            spawnPos.x = fanSpawnOnRight ? _fanRightEdge.position.x : _fanLeftEdge.position.x;
            spawnPos.z = position.z;
        }

        bool secondInPair = keepAwayFrom.HasValue;
        // Вентиляторы — только по краям (_fanLeftEdge / _fanRightEdge), вне spawn area по X
        bool needsGridSnap = !isFan
                             && (!IsInsideSpawnAreaXZ(spawnPos)
                                 || (secondInPair && !IsFarEnoughFrom(spawnPos, keepAwayFrom))
                                 || (!secondInPair && WouldOverlapObstacle(spawnPos)));

        if (needsGridSnap)
        {
            if (!TryFindNearestGridSpawnPosition(spawnPos, keepAwayFrom, secondInPair, out Vector3 resolvedGridPos))
                return false;

            spawnPos.x = resolvedGridPos.x;
            spawnPos.z = resolvedGridPos.z;
        }

        if (prefab.GetComponentInChildren<Bird>() != null)
            spawnPos.y = 3f;
        else if (prefab.GetComponentInChildren<Chipper>() != null)
            spawnPos.y += 0.7f;
        else if (prefab.GetComponentInChildren<WoodLog>() != null)
            spawnPos.y += 0.4f;
        else if (prefab.GetComponentInChildren<JumpTrap>() != null)
            spawnPos.y -= 0.9f;

        Quaternion rotation = Quaternion.identity;
        if (isFan)
            rotation = Quaternion.Euler(0f, fanSpawnOnRight ? 270f : 90f, 0f); // 90° поперёк уровня, +180° для правого края

        var instance = Instantiate(prefab, spawnPos, rotation, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        var pos = instance.transform.position;
        if (instance.GetComponentInChildren<Bird>() != null)
        {
            pos.y = 3f;
            instance.transform.position = pos;
        }

        if (instance.GetComponentInChildren<CircularSaw>() != null || instance.GetComponentInChildren<CircularSawMoving>() != null)
        {
            if (Random.value < 0.5f)
                instance.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
        }

        var acidPond = instance.GetComponentInChildren<AcidPond>();
        if (acidPond != null && _obstaclesSet != null)
            acidPond.SetObstaclesSet(_obstaclesSet);

        if (_obstaclesSet != null)
            _obstaclesSet.AddObstacle(instance);

        spawnedPosition = instance.transform.position;
        return true;
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

    private void TrySpawnBonus(Vector3 position)
    {
        var prefab = _bonusPrefabs[Random.Range(0, _bonusPrefabs.Count)];
        if (prefab == null || prefab.GetComponentInChildren<AppleCurrency>() == null)
        {
            Debug.LogWarning($"[RandomSpawner] Prefab {prefab?.name} не содержит ScoreBonus, пропуск.");
            return;
        }

        if (_minDistanceToObstacles > 0f && WouldOverlapObstacle(position))
            return;

        var instance = Instantiate(prefab, position, Quaternion.identity, _spawnedObjectsParent);
        instance.name = prefab.name + " (Spawned)";

        if (_obstaclesSet != null)
            _obstaclesSet.AddObstacle(instance);
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
