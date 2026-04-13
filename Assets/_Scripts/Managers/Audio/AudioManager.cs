using UnityEngine;
using UnityEngine.Audio;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour, IService
{
    [Header("Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private int poolSize = 10;
    
    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    
    [Header("Debug")]
    [SerializeField] private Utility.Logger _logger;    

    private List<AudioSource> _sfxPool;
    private AudioSource _musicSource;
    
    public async Task InitializeAsync()
    {
        // Создаем музыкальный канал
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.outputAudioMixerGroup = musicGroup;

        // Создаем пул для SFX
        _sfxPool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            var source = CreateNewSource("SFX_Pool_Item");
            source.outputAudioMixerGroup = sfxGroup;
            _sfxPool.Add(source);
        }

        _logger.Log($"[AudioManager] Initialized with pool size: {poolSize}", this);
        await Task.CompletedTask;
    }
    
    private AudioSource CreateNewSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    // --- Публичное API ---

    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (_musicSource.clip == clip) return;
        
        _musicSource.clip = clip;
        _musicSource.Play();
        // Добавить логику плавного затухания (fade)?
    }

    //бПример - PlaySfx(jumpClip, 0.8f, Random.Range(0.9f, 1.1f));
    public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        var source = GetFreeSource();
        if (source == null) return;

        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }

    // Звук остаётся в мировой позиции worldPosition и не следует за игроком.
    public void PlaySfxAtPoint(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        var source = GetFreeSource();
        if (source == null) return;

        source.transform.position = worldPosition;
        source.spatialBlend = 1f;
        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }

    public void PlayUiSound(AudioClip clip)
    {
        // UI обычно звучит без пространственного позиционирования
        var source = GetFreeSource();
        if (source == null) return;

        source.outputAudioMixerGroup = uiGroup;
        source.PlayOneShot(clip);
    }

    private AudioSource GetFreeSource()
    {
        foreach (var source in _sfxPool)
        {
            if (!source.isPlaying) return source;
        }
        
        // Расширяем пул, если все занято
        var newSource = CreateNewSource("SFX_Pool_Extended");
        newSource.outputAudioMixerGroup = sfxGroup;
        _sfxPool.Add(newSource);
        return newSource;
    }

    // Например SetVolume("MusicVolume", 0.5f);
    public void SetVolume(string parameterName, float volume)
    {
        // volume ожидается от 0.0001 до 1, переводим в децибелы
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
        mainMixer.SetFloat(parameterName, db);
    }
}