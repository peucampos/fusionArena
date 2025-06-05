using UnityEngine;
using System.Collections.Generic; // Para usar Dictionary

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("O AudioSource principal que irá reproduzir os sons do jogo.")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Tooltip("O AudioSource para a música de fundo.")]
    [SerializeField] private AudioSource musicAudioSource; // NOVO: AudioSource para a música

    [Header("Música de Fundo")] // NOVO: Cabeçalho para organização no Inspector
    [Tooltip("O AudioClip da música de fundo.")]
    public AudioClip backgroundMusicClip; // NOVO: AudioClip da música de fundo

    [Range(0f, 1f)] // NOVO: Restringe o volume no Inspector de 0 a 1
    [Tooltip("O volume da música de fundo (0.0 a 1.0).")]
    public float backgroundMusicVolume = 0.3f; // NOVO: Volume padrão da música de fundo (baixo)

    [Header("Clips de Áudio do Jogo")]
    public AudioClip fusionToNormalSFX;
    public AudioClip fusionToLargeSFX;
    public AudioClip fusionToSmallSFX;
    public AudioClip massPresenceDestroySFX;    // Quando 5 iguais são destruídos

    // Você pode usar um Dictionary para sons de fusão mais dinâmicos
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
            DontDestroyOnLoad(gameObject); // Opcional: mantém o AudioManager entre cenas

            if (sfxAudioSource == null)
            {
                // Se não houver um AudioSource atribuído, tente adicionar um
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false; // Não tocar ao iniciar
                sfxAudioSource.outputAudioMixerGroup = null; // Opcional: atribuir a um AudioMixerGroup se tiver
            }

            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.loop = true; // A música deve repetir ininterruptamente
                musicAudioSource.playOnAwake = false; // Não tocar automaticamente, vamos controlá-la via script
                musicAudioSource.outputAudioMixerGroup = null; // Opcional: atribua a um AudioMixerGroup se tiver
            }
        }
    }

    private void Start()
    {
        // Tocar a música de fundo ao iniciar o jogo
        if (backgroundMusicClip != null)
        {
            PlayBackgroundMusic(backgroundMusicClip, backgroundMusicVolume);
        }
        else
        {
            Debug.LogWarning("Nenhum AudioClip de música de fundo atribuído no AudioManager.");
        }
    }

    /// <summary>
    /// Toca um clipe de áudio uma única vez.
    /// </summary>
    /// <param name="clip">O AudioClip a ser reproduzido.</param>
    /// <param name="volume">O volume de reprodução (0.0 a 1.0).</param>
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
    /// Inicia a reprodução da música de fundo.
    /// </summary>
    /// <param name="clip">O AudioClip da música de fundo.</param>
    /// <param name="volume">O volume da música de fundo (0.0 a 1.0).</param>
    public void PlayBackgroundMusic(AudioClip clip, float volume)
    {
        if (musicAudioSource != null && clip != null)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.volume = volume;
            // musicAudioSource.loop já é true no Awake
            if (!musicAudioSource.isPlaying) // Só toca se já não estiver tocando
            {
                musicAudioSource.Play();
                Debug.Log("Música de fundo iniciada.");
            }
        }
        else
        {
            Debug.LogWarning("Não foi possível iniciar a música de fundo: AudioSource BGM ou AudioClip está nulo.");
        }
    }

    /// <summary>
    /// Para a reprodução da música de fundo. (Opcional, para quando o jogo termina, por exemplo)
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
            Debug.Log("Música de fundo parada.");
        }
    }

}