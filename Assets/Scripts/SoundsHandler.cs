using System.Collections.Generic;
using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField]
    private AudioClip _bonusGotClip;
    [SerializeField]
    private AudioClip _lemmingGotClip;
    [SerializeField]
    private AudioClip _fireScreamClip;
    [SerializeField]
    private List<AudioClip> _bloodSplatterClips;
    [SerializeField]
    private AudioSource _audioSource;
    private LemmingsEventsHandler _lemmingsEventsHandler;
   
    public void Initialize(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _lemmingsEventsHandler.OnLemmingOnFire += PlayFireScream;
        _lemmingsEventsHandler.OnLemmingKilled += PlaySplatter;
    }

    private void PlayFireScream()
    {
        _audioSource.PlayOneShot(_fireScreamClip);
        _audioSource.volume.Equals(80);
    }

    public void PlayAddLemming()
    {
        _audioSource.PlayOneShot(_lemmingGotClip);
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
        if (_lemmingsEventsHandler != null)
        {
            _lemmingsEventsHandler.OnLemmingOnFire -= PlayFireScream;
            _lemmingsEventsHandler.OnLemmingKilled -= PlaySplatter;
        }
    }
}
