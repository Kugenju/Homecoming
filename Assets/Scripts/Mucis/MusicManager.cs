using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    // 音乐相关组件与设置
    private AudioSource musicSource;
    [Header("音乐设置")]
    public List<AudioClip> musicClips;
    public bool useFade = true;
    public float fadeDuration = 1f;

    // 音效相关组件与设置
    private List<AudioSource> soundSources = new List<AudioSource>();
    [Header("音效设置")]
    public int maxSoundSources = 10;

    // 音量设置（音乐）
    [Header("音量控制")]
    public AudioMixer audioMixer;
    [Range(0f, 1f)] public float defaultMusicVolume = 1f;
    private const string MusicPrefKey = "MusicVolume";
    private float targetMusicVolume;  // 音乐目标音量（私有存储）

    // 音量设置（音效，与音乐逻辑一致）
    [Range(0f, 1f)] public float defaultSoundVolume = 1f;
    private const string SoundPrefKey = "SoundVolume";
    private float targetSoundVolume;  // 音效目标音量（私有存储）

    private bool isFading;  // 音乐淡入淡出标记

    private void Awake()
    {
        // 初始化音乐源
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = defaultMusicVolume;
        targetMusicVolume = defaultMusicVolume;

        // 初始化音效目标音量（与音乐逻辑一致）
        targetSoundVolume = defaultSoundVolume;

        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundSources();
            LoadVolumeSettings();  // 统一加载两种音量设置
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 初始化音效源池（用于叠加播放）
    private void InitializeSoundSources()
    {
        for (int i = 0; i < maxSoundSources; i++)
        {
            GameObject sourceObj = new GameObject($"SoundSource_{i}");
            sourceObj.transform.parent = transform;
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;  // 音效不循环
            source.volume = targetSoundVolume;  // 初始音量使用目标值
            soundSources.Add(source);
        }
    }

    // 加载保存的音量设置（统一处理音乐和音效）
    private void LoadVolumeSettings()
    {
        // 加载音乐音量（逻辑一致）
        if (PlayerPrefs.HasKey(MusicPrefKey))
        {
            SetMusicVolume(PlayerPrefs.GetFloat(MusicPrefKey), false);
        }
        else
        {
            SetMusicVolume(defaultMusicVolume, true);
        }

        // 加载音效音量（与音乐逻辑完全一致）
        if (PlayerPrefs.HasKey(SoundPrefKey))
        {
            SetSoundVolume(PlayerPrefs.GetFloat(SoundPrefKey), false);
        }
        else
        {
            SetSoundVolume(defaultSoundVolume, true);
        }
    }

    // 音乐控制相关方法
    public void PlayMusic() => musicSource.Play();
    public void PauseMusic() => musicSource.Pause();
    public void StopMusic() => musicSource.Stop();
    public bool IsMusicPlaying() => musicSource.isPlaying;

    public void ChangeMusicByIndex(int index)
    {
        if (index >= 0 && index < musicClips.Count)
        {
            ChangeMusic(musicClips[index]);
        }
        else
        {
            Debug.LogWarning("音乐索引超出范围");
        }
    }

    public void ChangeMusic(AudioClip newClip)
    {
        if (newClip == null)
        {
            Debug.LogWarning("音乐片段为空");
            return;
        }

        if (useFade && musicSource.clip != null)
        {
            StartCoroutine(FadeOutAndChange(newClip));
        }
        else
        {
            musicSource.clip = newClip;
            musicSource.Play();
        }
    }

    private System.Collections.IEnumerator FadeOutAndChange(AudioClip newClip)
    {
        if (isFading) yield break;
        isFading = true;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // 淡出当前音乐
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeDuration);
            yield return null;
        }

        // 切换音乐
        musicSource.clip = newClip;
        musicSource.Play();

        // 淡入新音乐
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, targetMusicVolume, elapsed / fadeDuration);
            yield return null;
        }

        isFading = false;
    }

    // 音效控制相关方法
    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("音效片段为空");
            return;
        }

        // 找到空闲的音效源
        AudioSource freeSource = soundSources.Find(s => !s.isPlaying);
        if (freeSource != null)
        {
            freeSource.clip = clip;
            freeSource.volume = targetSoundVolume;  // 使用私有目标音量
            freeSource.Play();
        }
        else
        {
            Debug.LogWarning("音效源已耗尽，无法播放新音效");
        }
    }

    // 音乐音量控制（与音效保持一致逻辑）
    public void SetMusicVolume(float volume) => SetMusicVolume(volume, true);
    private void SetMusicVolume(float volume, bool save)
    {
        float clamped = Mathf.Clamp01(volume);
        targetMusicVolume = clamped;  // 更新私有目标变量
        musicSource.volume = clamped;

        // 音频混合器处理（一致逻辑）
        if (audioMixer != null)
        {
            float dB = clamped > 0 ? Mathf.Log10(clamped) * 20 : -80;
            audioMixer.SetFloat("MusicVolume", dB);
        }

        // 持久化保存（一致逻辑）
        if (save)
        {
            PlayerPrefs.SetFloat(MusicPrefKey, clamped);
            PlayerPrefs.Save();
        }
    }

    // 音效音量控制（与音乐完全一致的逻辑）
    public void SetSoundVolume(float volume) => SetSoundVolume(volume, true);
    private void SetSoundVolume(float volume, bool save)
    {
        float clamped = Mathf.Clamp01(volume);
        targetSoundVolume = clamped;  // 更新私有目标变量

        // 音频混合器处理（与音乐一致）
        if (audioMixer != null)
        {
            float dB = clamped > 0 ? Mathf.Log10(clamped) * 20 : -80;
            audioMixer.SetFloat("SoundVolume", dB);
        }
        else
        {
            // 无混合器时更新所有音效源（与音乐逻辑对应）
            foreach (var source in soundSources)
            {
                source.volume = clamped;
            }
        }

        // 持久化保存（与音乐一致）
        if (save)
        {
            PlayerPrefs.SetFloat(SoundPrefKey, clamped);
            PlayerPrefs.Save();
        }
    }

    // 获取音乐音量（与音效逻辑一致）
    public float GetMusicVolume() => targetMusicVolume;

    // 获取音效音量（与音乐逻辑一致）
    public float GetSoundVolume() => targetSoundVolume;
}