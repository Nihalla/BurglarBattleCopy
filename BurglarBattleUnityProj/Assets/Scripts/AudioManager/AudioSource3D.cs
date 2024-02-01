using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AudioSource3D : MonoBehaviour
{
    [Tooltip("The audio clip used in this AudioSource.")]
    [SerializeField] private Audio _audioClip;
    [Tooltip("Sets the priority of the AudioSource.")]
    [SerializeField] [Range(0.0f, 1.0f)] private float _priority = 0.5f;
    [Space]
    [Tooltip("Is the audio source muted?")]
    [SerializeField] private bool _mute = false;
    [Tooltip("If set to true, the audio source will automatically start playing on awake.")]
    [SerializeField] private bool _playOnAwake = true;
    [Tooltip("Is the audio clip looping?")]
    [SerializeField] private bool _loop = false;

    [Header("3D Sound Settings")]
    [Tooltip("Sets the Doppler scale for this AudioSource.")]
    [SerializeField] [Range(0.0f, 5.0f)] private float _dopplerLevel = 1.0f;
    [Tooltip("Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.")]
    [SerializeField] [Range(0, 360)] private float _spread = 0.0f;
    [Tooltip("Sets/Gets how the AudioSource attenuates over distance.")]
    [SerializeField] private AudioRolloffMode _volumeRolloff = AudioRolloffMode.Logarithmic;
    [Tooltip("Within the Min distance the AudioSource will cease to grow louder in volume.")]
    [SerializeField] private float _minDistance = 1.0f;
    [Tooltip("(Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.")]
    [SerializeField] private float _maxDistance = 25.0f;
    
    /// <summary> The audio clip used in this AudioSource. </summary>
    public Audio AudioClip { get => _audioClip; set => _audioClip = value; }
    
    /// <summary> Is the audio source muted? </summary>
    public bool Mute { get => _mute; set { _mute = value; UpdateAudioSource(); } }
    
    /// <summary> If set to true, the audio source will automatically start playing on awake. </summary>
    public bool PlayOnAwake { get => _playOnAwake; set => _playOnAwake = value; }
    
    /// <summary> Is the audio clip looping? </summary>
    public bool Loop { get => _loop; set { _loop = value; UpdateAudioSource(); } }
    
    /// <summary> Sets the priority of the AudioSource. </summary>
    public float Priority { get => _priority; set => _priority = value; }

    /// <summary> Sets the Doppler scale for this AudioSource. </summary>
    public float DopplerLevel { get => _dopplerLevel; set { _dopplerLevel = value; UpdateAudioSource(); } }
    
    /// <summary> Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space. </summary>
    public float Spread { get => _spread; set { _spread = value; UpdateAudioSource(); } }
    
    /// <summary> Sets/Gets how the AudioSource attenuates over distance. </summary>
    public AudioRolloffMode VolumeRolloff { get => _volumeRolloff; set { _volumeRolloff = value; UpdateAudioSource(); } }
    
    /// <summary> Within the Min distance the AudioSource will cease to grow louder in volume. </summary>
    public float MinDistance { get => _minDistance; set { _minDistance = value; UpdateAudioSource(); } }
    
    /// <summary> (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at. </summary>
    public float MaxDistance { get => _maxDistance; set { _maxDistance = value; UpdateAudioSource(); } }
    
    /// <summary> Is the audio source currently playing audio </summary>
    public bool IsPlaying { get; private set; } = false;
    
    /// <summary> Current playback time of audio clip. </summary>
    public float PlaybackTime { get; private set; } = 0.0f;
    
    private bool _registered = false;
    private Coroutine _waitAndRegisterCoroutine = null;
    
    private delegate IEnumerator WaitAndRegisterDel(AudioSource3D source);
    private static readonly WaitAndRegisterDel _waitAndRegisterFunc = WaitAndRegister;

    private void OnEnable()
    {
        _waitAndRegisterCoroutine = StartCoroutine(_waitAndRegisterFunc(this));
    }
    
    private static IEnumerator WaitAndRegister(AudioSource3D source)
    {
        // NOTE(WSWhitehouse): Waiting until the AudioManager is initialised...
        while(!AudioManager.IsInitialised())
        {
            yield return null; // Wait for update
        }
        
        AudioManager.AudioSource3DRegister(source);
        source._registered               = true;
        source._waitAndRegisterCoroutine = null;
        
        if (source.PlayOnAwake)
        {
            source.Play();
        }
    }

    private void OnDisable()
    {
        if (_waitAndRegisterCoroutine != null)
        {
            StopCoroutine(_waitAndRegisterCoroutine);
            _waitAndRegisterCoroutine = null;
        }
        
        if (!_registered)                  return;
        if (!AudioManager.IsInitialised()) return;
        
        AudioManager.AudioSource3DUnregister(this);
        _registered = false;
    }

    private void Update()
    {
        if (!IsPlaying) return;

        IsPlaying = AudioManager.AudioSource3DGetUpdate(this, out float time);
        PlaybackTime = time;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateAudioSource()
    {
        if (!_registered) return;
        AudioManager.AudioSource3DUpdate(this);
    }
    
    /// <summary>
    /// Play audio from this audio source.
    /// </summary>
    /// <param name="delayInSeconds">Optional! Delay in seconds before playback begins.</param>
    public void Play(float delayInSeconds = 0.0f)
    {
        if (IsPlaying) return;
        IsPlaying = true;
        
        if (!_registered) return;
        AudioManager.AudioSource3DPlay(this, delayInSeconds);
    }
    
    /// <summary>
    /// Stop playing audio from this audio source.
    /// </summary>
    public void Stop()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        
        if (!_registered) return;
        AudioManager.AudioSource3DStop(this);
    }
    
    /// <summary>
    /// Pause playback at the current time.
    /// </summary>
    public void Pause()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        
        if (!_registered) return;
        AudioManager.AudioSource3DPause(this);
    }
    
    /// <summary>
    /// Unpause playback. Does NOT choose a new audio source.
    /// </summary>
    public void Unpause()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        
        if (!_registered) return;
        AudioManager.AudioSource3DUnpause(this);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color distanceColour = new Color(0.0f, 0.0f, 1.0f);
        Gizmos.color = distanceColour;
        
        Gizmos.DrawWireSphere(transform.position, _minDistance);
        Gizmos.DrawWireSphere(transform.position, _maxDistance);
    }
#endif
}
