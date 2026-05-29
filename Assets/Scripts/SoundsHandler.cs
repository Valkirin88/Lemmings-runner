using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField]
    private AudioClip _bonusGotClip;
    [SerializeField]
    private AudioClip _lemmingGotClip;
    [SerializeField]
    private List<AudioClip> _screamClips;
    [SerializeField]
    [Min(0f)]
    private float _screamCooldownSeconds = 0.5f;
    [SerializeField]
    private List<AudioClip> _bloodSplatterClips;
    [SerializeField]
    private AudioSource _audioSource;
    private LemmingsEventsHandler _lemmingsEventsHandler;
    private float _lastScreamTime = float.NegativeInfinity;
   
    public void Initialize(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _lemmingsEventsHandler.OnLemmingOnFire += PlayFireScream;
        _lemmingsEventsHandler.OnLemmingKilled += PlaySplatter;
    }

    private void PlayFireScream()
    {
        if (_screamClips == null || _screamClips.Count == 0)
            return;
        if (Time.time - _lastScreamTime < _screamCooldownSeconds)
            return;

        _lastScreamTime = Time.time;
        _audioSource.PlayOneShot(_screamClips[Random.Range(0, _screamClips.Count)]);
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
