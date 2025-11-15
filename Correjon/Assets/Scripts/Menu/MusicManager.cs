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
    [Range(0f, 1f)]
    public float volume = 0.8f;               // Volumen base

    private Coroutine transitionRoutine;      // PlayGameplaySequence
    private Coroutine fadeRoutine;            // Fades
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

    public void PlayMenuMusic()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        transitionRoutine = StartCoroutine(PlaySingleClip(menuTheme, loop: true));
    }

    public void PlayGameplayMusic()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        transitionRoutine = StartCoroutine(PlayGameplaySequence());
    }

    // Fade out y STOP total
    public void StopMusic()
    {
        // IMPORTANTE: matar también la corrutina de transición
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (musicSource.isPlaying)
            fadeRoutine = StartCoroutine(FadeOutAndStop());
    }

    // Fade out y PAUSE (para game over)
    public void FadeOutAndPause()
    {
        // IMPORTANTE: matar también la corrutina de transición
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (musicSource.isPlaying)
            fadeRoutine = StartCoroutine(FadeOutAndPauseRoutine());
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        musicSource.mute = mute;
    }

    // ==============================================================

    private IEnumerator PlaySingleClip(AudioClip clip, bool loop)
    {
        if (!clip) yield break;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        fadeRoutine = StartCoroutine(FadeIn());
    }

    private IEnumerator PlayGameplaySequence()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        // 1. Intro
        if (gameplayIntro)
        {
            musicSource.Stop();
            musicSource.clip = gameplayIntro;
            musicSource.loop = false;
            musicSource.volume = 0f;
            musicSource.Play();

            yield return StartCoroutine(FadeIn());

            // Esperar a que termine la intro (si aquí la pausas, isPlaying=false y esto terminaría)
            yield return new WaitWhile(() => musicSource.isPlaying);
        }

        // 2. Loop acelerado
        if (gameplayLoop)
        {
            musicSource.clip = gameplayLoop;
            musicSource.loop = true;
            musicSource.volume = volume;
            musicSource.Play();
        }

        transitionRoutine = null;
    }

    // ==============================================================

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
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
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = volume;
    }

    private IEnumerator FadeOutAndPauseRoutine()
    {
        float startVol = musicSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Pause();
        musicSource.volume = volume;
    }
}
