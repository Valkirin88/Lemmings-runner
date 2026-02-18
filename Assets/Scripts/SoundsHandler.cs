using System.Collections.Generic;
using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField]
    private AudioClip _bonusGotClip;
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
        _lemmingsStateSet.OnLemmingKilled += PlaySplatter;
    }

    private void PlayFireScream()
    {
        _audioSource.PlayOneShot(_fireScreamClip);
    }
    
    public void PlaySplatter()
    {
        if (_bloodSplatterClips.Count > 0)
        {
            var randomClip = _bloodSplatterClips[Random.Range(0, _bloodSplatterClips.Count)];
            _audioSource.PlayOneShot(randomClip);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    public void PlayBonusGot()
    {
        _audioSource.PlayOneShot(_bonusGotClip);
    }

    private void OnDestroy()
    {
        if (_lemmingsStateSet != null)
        {
            _lemmingsStateSet.OnLemmingOnFire -= PlayFireScream;
            _lemmingsStateSet.OnLemmingKilled -= PlaySplatter;
        }
    }
}
