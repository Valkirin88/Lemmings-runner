using UnityEngine;

public class ObstaclesDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.TryGetComponent<IObstacle>(out IObstacle obstacle))
        {
            Destroy(other.gameObject);
        }
    }
}
