using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;

    [SerializeField]
    private List<AudioClip> _audioClips;

    private static MusicHandler instance;

    private AudioClip _lastClip;
    private AudioClip _newClip;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            if (!_source.isPlaying)
            {
                PlayMusic();
            }
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // if (!GameInfo.IsMusicOn)
        //     _source.Stop();
        // else
        // {
            if(!_source.isPlaying)
            PlayMusic();
        // }
    }

    private AudioClip GetClip()
    {
        _newClip = _audioClips[Random.Range(0, _audioClips.Count)];
        if (_audioClips.Count > 0)
        {
            while (_lastClip == _newClip)
            {
                _newClip = _audioClips[Random.Range(0, _audioClips.Count)];
            }
            _lastClip = _newClip;
            return _newClip;
        }
        else
        {
            return _newClip;
        }
    }

    private void PlayMusic()
    {
        _source.PlayOneShot(GetClip());
    }

}
