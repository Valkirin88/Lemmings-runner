using UnityEngine;

public class AcidSphere : MonoBehaviour, IObstacle
{
  [SerializeField] private float _timeTillDestroy = 3f;
  
  private void Start()
  {
    Destroy(gameObject,_timeTillDestroy);
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.TryGetComponent<LemmingView>(out LemmingView lemmingView))
    {
      if (lemmingView.IsRun && !lemmingView.IsInvincible)
      {
        lemmingView.Kill();
        Destroy(gameObject);
      }
    }

    if (other.TryGetComponent<PlatformTextureScroller>(out PlatformTextureScroller platformTextureScroller))
    {
      Destroy(gameObject);
    }
    
  }

  public BloodZone BloodZone { get; }
  public void SpawnBlood()
  {
    
  }

  public void MakeSound()
  {
    
  }

  public void OnDestroy()
  {
    
  }
}
