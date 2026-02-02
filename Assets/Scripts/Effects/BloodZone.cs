using UnityEngine;

/// <summary>
/// Зона для появления пятен крови.
/// Добавьте к препятствию и настройте размер в инспекторе.
/// Все остальные настройки (количество, размер пятен) в BloodDecalSpawner.
/// </summary>
public class BloodZone : MonoBehaviour
{
    [Header("Zone Size")]
    [SerializeField] private Vector3 _zoneSize = new Vector3(2f, 0.5f, 2f);
    
    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;
    [SerializeField] private Color _gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    
    /// <summary>
    /// Создаёт пятна крови в этой зоне
    /// </summary>
    public void SpawnBlood()
    {
        if (BloodDecalSpawner.Instance == null)
        {
            Debug.LogError("[BloodZone] BloodDecalSpawner.Instance не найден! Создайте объект с компонентом BloodDecalSpawner на сцене.");
            return;
        }
        
        BloodDecalSpawner.Instance.SpawnDecalsInZone(transform.position, _zoneSize);
    }
    
    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;
        
        Gizmos.color = _gizmoColor;
        Gizmos.DrawCube(transform.position, _zoneSize);
        
        // Рамка
        Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);
        Gizmos.DrawWireCube(transform.position, _zoneSize);
    }
}
