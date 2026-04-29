using UnityEngine;

public class BottomBloodDecalScroller : MonoBehaviour
{
    private Vector3 _direction = Vector3.back;
    private float _speedMultiplier = 1f;
    private PlatformTextureScroller _textureScroller;

    public void Initialize(Vector3 direction, float speedMultiplier, PlatformTextureScroller textureScroller = null)
    {
        _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.back;
        _speedMultiplier = speedMultiplier;
        _textureScroller = textureScroller;
    }

    private void Update()
    {
        Vector3 velocity = GetVelocity();
        if (velocity.sqrMagnitude <= 0f) return;

        transform.position += velocity * Time.deltaTime;
    }

    private Vector3 GetVelocity()
    {
        if (_textureScroller != null)
        {
            // Едем ровно с той же скоростью, с которой визуально движется текстура.
            return _textureScroller.GetVisualWorldScrollVelocity() * _speedMultiplier;
        }

        float speed = ScrollSpeedProvider.CurrentSpeed * _speedMultiplier;
        if (speed <= 0f) return Vector3.zero;
        return _direction * speed;
    }
}
