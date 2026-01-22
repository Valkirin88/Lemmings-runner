using System.Collections.Generic;
using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField]
    private AudioClip _fireScreamClip;
    [SerializeField]
    private List<AudioClip> _bloodSplatterClips;
    [SerializeField]
    private AudioSource _audioSource;
    private LemmingsStateSet _lemmingsStateSet;
   
    public void Initialize(LemmingsStateSet lemmingsStateSet)
    {
        _lemmingsStateSet = lemmingsStateSet;
        _lemmingsStateSet.OnLemmingOnFire += PlayFireScream;
        _lemmingsStateSet.OnLemmingCountRemove += PlaySplatter;
    }

    private void PlayFireScream()
    {
        _audioSource.PlayOneShot(_fireScreamClip);
        Debug.Log("Fire Scream Played");
    }
    
    private void PlaySplatter(LemmingView obj)
    {
        if (_bloodSplatterClips.Count > 0)
        {
            var randomClip = _bloodSplatterClips[Random.Range(0, _bloodSplatterClips.Count)];
            _audioSource.PlayOneShot(randomClip);
        }
    }

    private void OnDestroy()
    {
        _lemmingsStateSet.OnLemmingOnFire -= PlayFireScream;
        _lemmingsStateSet.OnLemmingCountRemove -= PlaySplatter;
    }
}
