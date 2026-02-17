using UnityEngine;

public class ObstaclesDestroyer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // IObstacle может висеть на корне префаба или на дочернем объекте — удаляем корень иерархии
        var obstacle = other.GetComponentInParent<IObstacle>();
        if (obstacle != null)
        {
            GameObject root = (obstacle as MonoBehaviour).transform.root.gameObject;
            Destroy(root);
            return;
        }

        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            Destroy(lemmingView.gameObject);
        }

        if (other.TryGetComponent<ScoreBonus>(out ScoreBonus scoreBonus))
        {
            Destroy(scoreBonus.gameObject);
        }
    }
}
