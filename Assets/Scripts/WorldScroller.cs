using UnityEngine;

/// <summary>
/// Двигает в сторону -Z препятствия (IObstacle). Лемминги с IsScroll двигаются в LemmingView.
/// Лидер бежит на месте, Obstacles и idle-лемминги движутся навстречу.
/// </summary>
public class WorldScroller : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Источник скорости (LemmingPlaceView). ObstaclesSet для списка препятствий")]
    private LemmingPlaceView _lemmingPlaceView;

    [SerializeField]
    private ObstaclesSet _obstaclesSet;

    private void FixedUpdate()
    {
        if (_lemmingPlaceView == null)
            return;

        float speed = _lemmingPlaceView.EffectiveForwardSpeed;
        ScrollSpeedProvider.CurrentSpeed = speed;

        if (speed <= 0f)
            return;

        MoveObstacles(speed);
    }

    private void MoveObstacles(float speed)
    {
        if (_obstaclesSet == null || _obstaclesSet.Obstacles == null)
            return;

        Vector3 scrollDelta = Vector3.back * speed * Time.fixedDeltaTime;

        foreach (var obj in _obstaclesSet.Obstacles)
        {
            if (obj == null) continue;

            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                var vel = rb.linearVelocity;
                vel.z = -speed;
                rb.linearVelocity = vel;
            }
            else
            {
                obj.transform.position += scrollDelta;
            }
        }
    }
}
