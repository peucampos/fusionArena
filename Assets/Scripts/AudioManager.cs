using UnityEngine;
using System.Collections.Generic; // Para usar Dictionary

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("O AudioSource principal que ir� reproduzir os sons do jogo.")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Tooltip("O AudioSource para a m�sica de fundo.")]
    [SerializeField] private AudioSource musicAudioSource; // NOVO: AudioSource para a m�sica

    [Header("M�sica de Fundo")] // NOVO: Cabe�alho para organiza��o no Inspector
    [Tooltip("O AudioClip da m�sica de fundo.")]
    public AudioClip backgroundMusicClip; // NOVO: AudioClip da m�sica de fundo

    [Range(0f, 1f)] // NOVO: Restringe o volume no Inspector de 0 a 1
    [Tooltip("O volume da m�sica de fundo (0.0 a 1.0).")]
    public float backgroundMusicVolume = 0.3f; // NOVO: Volume padr�o da m�sica de fundo (baixo)

    [Header("Clips de �udio do Jogo")]
    public AudioClip fusionToNormalSFX;
    public AudioClip fusionToLargeSFX;
    public AudioClip fusionToSmallSFX;
    public AudioClip massPresenceDestroySFX;    // Quando 5 iguais s�o destru�dos

    // Voc� pode usar um Dictionary para sons de fus�o mais din�micos
    // private Dictionary<EnemyAI.EnemyType, AudioClip> fusionSounds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: mant�m o AudioManager entre cenas

            if (sfxAudioSource == null)
            {
                // Se n�o houver um AudioSource atribu�do, tente adicionar um
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false; // N�o tocar ao iniciar
                sfxAudioSource.outputAudioMixerGroup = null; // Opcional: atribuir a um AudioMixerGroup se tiver
            }

            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.loop = true; // A m�sica deve repetir ininterruptamente
                musicAudioSource.playOnAwake = false; // N�o tocar automaticamente, vamos control�-la via script
                musicAudioSource.outputAudioMixerGroup = null; // Opcional: atribua a um AudioMixerGroup se tiver
            }
        }
    }

    private void Start()
    {
        // Tocar a m�sica de fundo ao iniciar o jogo
        if (backgroundMusicClip != null)
        {
            PlayBackgroundMusic(backgroundMusicClip, backgroundMusicVolume);
        }
        else
        {
            Debug.LogWarning("Nenhum AudioClip de m�sica de fundo atribu�do no AudioManager.");
        }
    }

    /// <summary>
    /// Toca um clipe de �udio uma �nica vez.
    /// </summary>
    /// <param name="clip">O AudioClip a ser reproduzido.</param>
    /// <param name="volume">O volume de reprodu��o (0.0 a 1.0).</param>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            // Debug.LogWarning($"Tentativa de tocar SFX falhou: AudioSource ou AudioClip nulo. Clip: {clip?.name ?? "Nulo"}");
        }
    }

    /// <summary>
    /// Inicia a reprodu��o da m�sica de fundo.
    /// </summary>
    /// <param name="clip">O AudioClip da m�sica de fundo.</param>
    /// <param name="volume">O volume da m�sica de fundo (0.0 a 1.0).</param>
    public void PlayBackgroundMusic(AudioClip clip, float volume)
    {
        if (musicAudioSource != null && clip != null)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.volume = volume;
            // musicAudioSource.loop j� � true no Awake
            if (!musicAudioSource.isPlaying) // S� toca se j� n�o estiver tocando
            {
                musicAudioSource.Play();
                Debug.Log("M�sica de fundo iniciada.");
            }
        }
        else
        {
            Debug.LogWarning("N�o foi poss�vel iniciar a m�sica de fundo: AudioSource BGM ou AudioClip est� nulo.");
        }
    }

    /// <summary>
    /// Para a reprodu��o da m�sica de fundo. (Opcional, para quando o jogo termina, por exemplo)
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("M�sica de fundo parada.");
        }
    }

}