using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Fuentes y clips")]
    public AudioSource musicSource;           // Fuente principal de música
    public AudioClip menuTheme;               // Canción del menú
    public AudioClip gameplayIntro;           // Canción del juego (inicia lenta)
    public AudioClip gameplayLoop;            // Canción del juego (versión acelerada)

    [Header("Transiciones")]
    public float fadeDuration = 1.5f;         // Duración del fade in/out
    public float volume = 0.8f;               // Volumen base

    private Coroutine transitionRoutine;
    private bool isMuted;

    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
        }

        musicSource.loop = true;
        musicSource.volume = volume;
    }

    // ==============================================================
    // --- MÉTODOS DE CONTROL ---
    // ==============================================================

    public void PlayMenuMusic()
    {
        PlayMusic(menuTheme, loop: true);
    }

    public void PlayGameplayMusic()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(PlayGameplaySequence());
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            StartCoroutine(FadeOutAndStop());
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        musicSource.mute = mute;
    }

    // ==============================================================
    // --- IMPLEMENTACIÓN INTERNA ---
    // ==============================================================

    private void PlayMusic(AudioClip clip, bool loop)
    {
        if (!clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();
        StartCoroutine(FadeIn());
    }

    private IEnumerator PlayGameplaySequence()
    {
        // 1. Reproduce intro lenta
        if (gameplayIntro)
        {
            musicSource.clip = gameplayIntro;
            musicSource.loop = false;
            musicSource.volume = 0f;
            musicSource.Play();
            yield return StartCoroutine(FadeIn());

            // Espera a que termine la canción lenta
            yield return new WaitWhile(() => musicSource.isPlaying);
        }

        // 2. Transición a canción acelerada (loop)
        if (gameplayLoop)
        {
            yield return StartCoroutine(FadeOutAndSwitch(gameplayLoop, loop: true));
            yield return StartCoroutine(FadeIn());
        }
    }

    // ==============================================================
    // --- EFECTOS DE TRANSICIÓN ---
    // ==============================================================

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, volume, t / fadeDuration);
            yield return null;
        }
        musicSource.volume = volume;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = volume;
    }

    private IEnumerator FadeOutAndSwitch(AudioClip next, bool loop)
    {
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = next;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();
    }
}
