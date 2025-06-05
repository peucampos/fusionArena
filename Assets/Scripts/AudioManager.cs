using UnityEngine;
using System.Collections.Generic; // Para usar Dictionary

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("O AudioSource principal que ir� reproduzir os sons do jogo.")]
    [SerializeField] private AudioSource sfxAudioSource;

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
}