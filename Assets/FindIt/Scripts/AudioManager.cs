using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    public AudioClip sfxButtonClick;
    public AudioClip sfxCorrect;
    public AudioClip sfxWrong;
    public AudioClip sfxTreasureClaim;
    public AudioClip sfxMissionComplete;

    [Header("BGM")]
    public AudioClip bgmMenu;

    AudioSource sfxSource;
    AudioSource bgmSource;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.spatialBlend = 0f;
        bgmSource.loop = true;
        bgmSource.volume = 0.35f;
        bgmSource.playOnAwake = false;
    }

    void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmMenu == null || bgmSource == null) return;
        bgmSource.clip = bgmMenu;
        bgmSource.Play();
    }

    public void StopBGM() { if (bgmSource) bgmSource.Stop(); }

    public void PlayButtonClick()     => PlaySFX(sfxButtonClick, 1.0f);
    public void PlayCorrect()         => PlaySFX(sfxCorrect, 1.0f);
    public void PlayWrong()           => PlaySFX(sfxWrong, 1.0f);
    public void PlayTreasureClaim()   => PlaySFX(sfxTreasureClaim, 1.0f);
    public void PlayMissionComplete() => PlaySFX(sfxMissionComplete, 0.8f);

    void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySpatialSFX(AudioClip clip, Vector3 worldPos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, volume);
    }
}
