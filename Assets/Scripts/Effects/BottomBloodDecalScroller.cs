using UnityEngine;

public class BottomBloodDecalScroller : MonoBehaviour
{
    private Vector3 _direction = Vector3.back;
    private float _speedMultiplier = 1f;
    private PlatformTextureScroller _textureScroller;

    private Vector3 _virtualWorldPosition;
    private bool _initialized;

    public void Initialize(Vector3 direction, float speedMultiplier, PlatformTextureScroller textureScroller = null)
    {
        _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.back;
        _speedMultiplier = speedMultiplier;
        _textureScroller = textureScroller;
    }

    private void LateUpdate()
    {
        if (!_initialized)
        {
            _virtualWorldPosition = transform.position;
            _initialized = true;
        }

        Vector3 velocity = GetVelocity();
        _virtualWorldPosition += velocity * Time.deltaTime;

        // Принудительно задаём мировую позицию — даже если родитель движется,
        // декаль едет ровно с той скоростью, что задаёт нам _textureScroller.
        transform.position = _virtualWorldPosition;
    }

    private Vector3 GetVelocity()
    {
        // Скорость берётся из текстурного скроллера (если задан) либо из ScrollSpeedProvider.
        // Направление всегда берётся из настроек Bottom — так его проще флипнуть в инспекторе.
        float speed = _textureScroller != null
            ? _textureScroller.GetVisualWorldSpeed()
            : ScrollSpeedProvider.CurrentSpeed;

        speed *= _speedMultiplier;
        if (speed <= 0f) return Vector3.zero;
        return _direction * speed;
    }
}
