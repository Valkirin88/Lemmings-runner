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

        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView)
            && !lemmingView.IsDead
            && !lemmingView.IsInvincible)
        {
            // Зона «сброса» сзади отряда — не игровая смерть, без капель на UI
            lemmingView.KillWithotBlood();
        }

        if (other.TryGetComponent<AppleCurrency>(out AppleCurrency scoreBonus))
        {
            Destroy(scoreBonus.gameObject);
        }
    }
}
