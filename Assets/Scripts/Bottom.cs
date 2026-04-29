using UnityEngine;

public class Bottom : MonoBehaviour
{
   [SerializeField] private Collider _collider;
   [SerializeField] private Vector3 _bloodZoneSize = new Vector3(1.2f, 0.3f, 1.2f);
   [SerializeField] [Tooltip("Родитель для крови. Если не задан, кровь будет дочерней для Bottom.")]
   private Transform _bloodScrollParent;

   [Header("Скорость движения крови")]
   [SerializeField] [Tooltip("Текстурный скроллер пола. Если задан, кровь едет с той же визуальной скоростью, что и текстура.")]
   private PlatformTextureScroller _textureScroller;

   [SerializeField] [Tooltip("Запасной режим (если _textureScroller не задан): направление движения крови по Bottom.")]
   private Vector3 _bloodScrollDirection = Vector3.back;

   [SerializeField] [Tooltip("Доп. множитель скорости крови. 1 = ровно как у текстуры (если _textureScroller задан).")]
   private float _bloodScrollSpeedMultiplier = 1f;

   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
      {
         SpawnBlood(other.transform.position);
         lemmingView.KillWithotBlood();
      }
   }

   private void SpawnBlood(Vector3 lemmingPosition)
   {
      if (BloodDecalSpawner.Instance == null) return;

      Vector3 contactPoint = lemmingPosition;
      if (_collider != null)
      {
         contactPoint.y = _collider.bounds.max.y;
      }

      var decals = BloodDecalSpawner.Instance.SpawnDecalsOnSurface(contactPoint, _bloodZoneSize, Vector3.up, _bloodScrollParent != null ? _bloodScrollParent : transform);
      foreach (var decal in decals)
      {
         decal.AddComponent<BottomBloodDecalScroller>().Initialize(_bloodScrollDirection, _bloodScrollSpeedMultiplier, _textureScroller);
      }
   }
}
