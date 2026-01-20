using UnityEngine;

public class Bonfire : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
        {
            if (!lemmingView.IsOnFire)
            {
                lemmingView.SetFire();
            }
        }
    }
}
