using UnityEngine;

public class ObstaclesDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IObstacle>(out IObstacle obstacle))
        {
            Destroy(other.gameObject);
        }
    }
}
