using UnityEngine;

public class Bottom : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
      {
         lemmingView.KillWithotBlood();
      }
   }
}
