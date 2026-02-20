using UnityEngine;

public class MainPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 _bloodZoneSize = new Vector3(1.2f, 0.3f, 1.2f);

    private void OnCollisionEnter(Collision collision)
    {
        var lemming = collision.gameObject.GetComponent<LemmingView>() ?? collision.gameObject.GetComponentInParent<LemmingView>();
        if (lemming == null || !lemming.IsPushed || lemming.IsDead) return;

        Vector3 contactPoint = collision.GetContact(0).point;
        if (BloodDecalSpawner.Instance != null)
            BloodDecalSpawner.Instance.SpawnDecalsInZone(contactPoint, _bloodZoneSize, transform);

        lemming.Kill(destroyImmediately: true);
    }
}
