using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;

    [SerializeField]
    private List<AudioClip> _audioClipsForGame;

    [SerializeField]
    private List<AudioClip> _audioClipsForMenu;

    private static MusicHandler instance;

    private AudioClip _lastClip;
    private AudioClip _newClip;
    private AudioClip _lastMenuClip;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (!_source.isPlaying)
            {
                PlayMusic();
            }
        }
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _source.Stop();
        PlayMusic();
    }

    private void Update()
    {
        if (!_source.isPlaying)
            PlayMusic();
    }

    private bool IsMenuScene()
    {
        return SceneManager.GetActiveScene().buildIndex == 0;
    }

    private AudioClip GetGameClip()
    {
        if (_audioClipsForGame == null || _audioClipsForGame.Count == 0)
            return null;
        _newClip = _audioClipsForGame[Random.Range(0, _audioClipsForGame.Count)];
        if (_audioClipsForGame.Count > 1)
        {
            while (_lastClip == _newClip)
            {
                _newClip = _audioClipsForGame[Random.Range(0, _audioClipsForGame.Count)];
            }
        }
        _lastClip = _newClip;
        return _newClip;
    }

    private AudioClip GetMenuClip()
    {
        if (_audioClipsForMenu == null || _audioClipsForMenu.Count == 0)
            return null;
        _newClip = _audioClipsForMenu[Random.Range(0, _audioClipsForMenu.Count)];
        if (_audioClipsForMenu.Count > 1)
        {
            while (_lastMenuClip == _newClip)
            {
                _newClip = _audioClipsForMenu[Random.Range(0, _audioClipsForMenu.Count)];
            }
        }
        _lastMenuClip = _newClip;
        return _newClip;
    }

    private void PlayMusic()
    {
        if (IsMenuScene())
        {
            AudioClip clip = GetMenuClip();
            if (clip != null)
                _source.PlayOneShot(clip);
        }
        else
        {
            AudioClip clip = GetGameClip();
            if (clip != null)
                _source.PlayOneShot(clip);
        }
    }
}
