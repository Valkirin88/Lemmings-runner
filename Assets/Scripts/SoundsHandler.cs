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
    private AudioClip _jumpClip;
    [SerializeField]
    private List<AudioClip> _screamClips;
    [SerializeField]
    [Min(0f)]
    private float _screamCooldownSeconds = 0.5f;
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Громкость всех криков в списке, кроме первого")]
    private float _screamVolumeExceptFirst = 0.8f;
    [SerializeField]
    private List<AudioClip> _bloodSplatterClips;
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    [Tooltip("Отдельный источник для криков — его можно остановить (PlayOneShot не останавливается)")]
    private AudioSource _screamAudioSource;

    private LemmingsEventsHandler _lemmingsEventsHandler;
    private float _lastScreamTime = float.NegativeInfinity;

    private void Awake()
    {
        if (_screamAudioSource == null)
        {
            _screamAudioSource = gameObject.AddComponent<AudioSource>();
            _screamAudioSource.playOnAwake = false;
            _screamAudioSource.loop = false;
        }
    }

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
        int clipIndex = Random.Range(0, _screamClips.Count);
        _screamAudioSource.clip = _screamClips[clipIndex];
        _screamAudioSource.volume = clipIndex == 0 ? 1f : _screamVolumeExceptFirst;
        _screamAudioSource.Play();
    }

    public void StopScream()
    {
        if (_screamAudioSource != null && _screamAudioSource.isPlaying)
            _screamAudioSource.Stop();
    }

    public void PlayAddLemming()
    {
        _audioSource.PlayOneShot(_lemmingGotClip);
    }

    public void PlayJump()
    {
        if (_jumpClip != null)
            _audioSource.PlayOneShot(_jumpClip);
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
