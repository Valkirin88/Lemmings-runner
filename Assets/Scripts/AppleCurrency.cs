using System.Collections;
using UnityEngine;

public class AppleCurrency : MonoBehaviour
{
   [SerializeField]
   private int _score;

   [Header("Pick Up Animation")]
   [SerializeField]
   [Tooltip("Сколько секунд объект уменьшается перед исчезновением.")]
   private float _shrinkDuration = 0.25f;

   [SerializeField]
   [Tooltip("Скорость вращения во время уменьшения (умножается на базовую).")]
   private float _shrinkRotationMultiplier = 6f;

   [SerializeField]
   [Tooltip("Локальное смещение точки следования относительно лемминга (например, поднять яблоко чуть выше центра).")]
   private Vector3 _followOffset = new Vector3(0f, 0.4f, 0f);

   private float _rotationSpeed = 5f;
   private bool _isPickedUp;
   private Transform _follow;

   public int Score => _score;

   private void Update()
   {
      Rotate();
   }

   private void Rotate()
   {
      float speed = _isPickedUp ? _rotationSpeed * _shrinkRotationMultiplier : _rotationSpeed;
      transform.Rotate(Vector3.up, speed * Time.deltaTime);
   }

   /// <summary>
   /// Запускает анимацию уменьшения и затем уничтожает объект.
   /// Если передан follow — пока яблоко уменьшается, оно следует за этим объектом.
   /// Повторные вызовы игнорируются.
   /// </summary>
   public void PickUp(Transform follow = null)
   {
      if (_isPickedUp) return;
      _isPickedUp = true;
      _follow = follow;

      foreach (var col in GetComponentsInChildren<Collider>())
         col.enabled = false;

      if (_shrinkDuration > 0f)
         StartCoroutine(ShrinkAndDestroy());
      else
         Destroy(gameObject);
   }

   private IEnumerator ShrinkAndDestroy()
   {
      Vector3 startScale = transform.localScale;
      Vector3 endScale = Vector3.zero;
      float elapsed = 0f;

      while (elapsed < _shrinkDuration)
      {
         elapsed += Time.deltaTime;
         float t = Mathf.Clamp01(elapsed / _shrinkDuration);
         transform.localScale = Vector3.Lerp(startScale, endScale, t);

         if (_follow != null)
         {
            transform.position = _follow.position + _followOffset;
         }

         yield return null;
      }

      transform.localScale = endScale;
      Destroy(gameObject);
   }
}
