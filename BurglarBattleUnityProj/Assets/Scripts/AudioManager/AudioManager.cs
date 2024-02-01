// Author: William Whitehouse (WSWhitehouse)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

// TODO(WSWhitehouse):
// - Fix gamepad audio for player space
// - Add support for music

/// <summary>
/// This script manages Audio within the game. It is a "pseudo-singleton" which
/// means it has a static instance, however this should never be accessed by
/// other scripts. All interactions should be done through the appropriate public
/// static functions.
/// </summary>
[RequireComponent(typeof(AudioListener))]
public class AudioManager : MonoBehaviour
{
    // NOTE(WSWhitehouse): This is the singleton instance, do NOT make public.
    private static AudioManager s_instance = null;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer _audioMixer;
    
    [Header("Audio Source Pool Settings")]
    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _initialAudioSourcePoolCount = 50;
    [SerializeField] private int _audioSourceResizeFactor = 2;
    
    private AudioMixerGroup _masterGroup;
    private AudioMixerGroup _musicGroup;
    private AudioMixerGroup _playerSpaceGroup;
    private AudioMixerGroup _screenSpaceGroup;
    private AudioMixerGroup _3dGroup;

    private AudioSource _playerSpaceSource = null;
    private AudioSource _screenSpaceSource = null;
    
    private List<AudioSource> _3dAudioSourcePool = null;
    
    // NOTE(WSWhitehouse): This reference is updated when a new scene is
    // loaded, if it's null, there is no player manager in the scene.
    private FourPlayerManager _playerManager = null;

    private struct AudioSourceRef
    {
        public AudioSource3D audioSource3D;
        public AudioSource   audioSource;
    }
    
    private static Dictionary<AudioSource3D, AudioSourceRef> s_active3dAudioSources = null;
    
    /// <summary> Check if the Audio System been initialised. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInitialised() => s_instance != null;
    
    /// <summary>
    /// Play a One Shot sound in world space at the specified position. A world space
    /// sound has 3D sound and is picked up by all players in the area.
    /// </summary>
    /// <param name="audio">Audio to play.</param>
    /// <param name="worldPosition">Position (in world space!) to play the audio.</param>
    public static void PlayOneShotWorldSpace(Audio audio, Vector3 worldPosition)
    {
        Debug.Assert(s_instance != null,  "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
        Debug.Assert(audio      != null, $"Audio Manager was sent a null `audio` parameter on {nameof(PlayOneShotWorldSpace)}!");

        if (s_instance._playerManager == null) { return; }
        List<PlayerProfile> players = s_instance._playerManager.Players;
        int playerCount = players.Count;
        
        if (playerCount <= 0) { return; }
        
        if (s_instance._3dAudioSourcePool.Count <= 0)
        {
            ////Debug.Log("Audio Manager: Resizing the 3D Audio Source Pool! Consider increasing the initial spawn amount.");
            int newCapacity = s_instance._3dAudioSourcePool.Capacity * s_instance._audioSourceResizeFactor;
            for (int i = 0; i < newCapacity; i++)
            {
                s_instance.Spawn3dAudioSource();
            }
        }
        
        AudioSource source = s_instance._3dAudioSourcePool[0];
        s_instance._3dAudioSourcePool.RemoveAt(0);

        Audio.Clip clip = audio.RandomlySelectClip();
        source.volume   = clip.volumeRange.Random();
        source.pitch    = clip.pitchRange.Random();

        float3 sourcePos      = worldPosition;
        float3 firstPlayerPos = players[0].transform.position;
        float dist = math.distance(sourcePos, firstPlayerPos);
        float3 dir = sourcePos - firstPlayerPos;
        
        for (int i = 1; i < playerCount; i++)
        {
            float3 playerPosition = players[i].transform.position;
            float playerDist = math.distance(sourcePos, playerPosition);
            
            if (playerDist < dist)
            {
                dist = playerDist;
                dir  = sourcePos - playerPosition;
            }
        }

        dir = math.normalizesafe(dir);

        Transform sourceTransform = source.transform;
        sourceTransform.localPosition = dir * dist;
        
        source.PlayOneShot(clip.audioClip);

        // Return source to pool
        s_instance._3dAudioSourcePool.Add(source);
    }
    
    /// <summary>
    /// Play a One Shot sound in player space. Should play audio spaceialised to the players
    /// screen position and output to their gamepad (if one is connected).
    /// </summary>
    /// <param name="audio">Audio to play.</param>
    /// <param name="playerID">ID of the player. MUST BE A VALID PLAYER!</param>
    public static void PlayPlayerSpace(Audio audio, int playerID)
    {
        Debug.Assert(s_instance != null,      "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!"); 
        Debug.Assert(audio != null,          $"Audio Manager was sent a null `audio` parameter on {nameof(PlayPlayerSpace)}!");
        Debug.Assert(audio.Clips.Length > 0, $"Audio Manager was sent an `audio` ({audio.name}) with no audio clips ({nameof(PlayPlayerSpace)})!");
        // Debug.Assert(InputDevices.Devices[playerID] != null, $"Audio Manager was sent an invalid playerID parameter on {nameof(PlayOneShotPlayerSpace)} (player ID: {playerID}!");
        
        Audio.Clip clip = audio.RandomlySelectClip();
        
        ref AudioSource source = ref s_instance._playerSpaceSource;
        
        // NOTE(WSWhitehouse): The player ID is used as a lookup for the stereo pan in this
        // array. This represents if the audio should come out the left or right speakers.
        // Using a stack allocated array here so we don't allocate a small array on the heap
        // each time!
        Span<float> stereoPan = stackalloc float[InputDevices.MAX_DEVICE_COUNT]
        {
            -1.0f, 1.0f, -1.0f, 1.0f
        };
        
        source.panStereo = stereoPan[playerID];
        source.volume    = clip.volumeRange.Random();
        source.pitch     = clip.pitchRange.Random();
        source.PlayOneShot(clip.audioClip);
        
        // if (InputDevices.Devices[playerID].IsDeviceGamepad)
        // {
        //     // REVIEW(WSWhitehouse): The gamepad "slot" and the playerID probably won't match,
        //     // will need to figure out a way of lining them up using the new Input system.
        //     // This actually doesnt seem to work at all.
        //     source.panStereo = 0.0f;
        //     source.PlayOnGamepad(playerID);
        // }
    }
    
    /// <summary>
    /// Play a One Shot sound in screen space. All players hear this effect equally. Should be
    /// used for global or UI sound effects.
    /// </summary>
    /// <param name="audio">Audio to play.</param>
    public static void PlayScreenSpace(Audio audio)
    {
        Debug.Assert(s_instance != null,      "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
        Debug.Assert(audio != null,          $"Audio Manager was sent a null `audio` parameter on {nameof(PlayScreenSpace)}!");
        Debug.Assert(audio.Clips.Length > 0, $"Audio Manager was sent an `audio` parameter with no audio clips on {nameof(PlayScreenSpace)}!");

        Audio.Clip clip = audio.RandomlySelectClip();
        
        ref AudioSource source = ref s_instance._screenSpaceSource;
        source.volume = clip.volumeRange.Random();
        source.pitch  = clip.pitchRange.Random();
        source.PlayOneShot(clip.audioClip);
    }
    
    private void Awake()
    {
        // If an instance already exists destroy this object...
        if (s_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Debug.Assert(_audioMixer        != null, "The audio mixer is null on the Audio Manager! Please assign one.");
        Debug.Assert(_audioSourcePrefab != null, "The audio source prefab is null on the Audio Manager! Please assign one.");
        
#if UNITY_EDITOR
        // NOTE(WSWhitehouse): This checks for more than one audio listener in the scene, if there is it throws an error.
        // This is only done during the editor as a debug check, it should never be compiled into the actual build.
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        Debug.Assert(audioListeners.Length == 1, "There should only be one Audio Listener in the scene! Please ensure the AudioManager is the only object with one.");
#endif // UNITY_EDITOR
        
        s_instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Get Mixer Groups
        _masterGroup      = _audioMixer.FindMatchingGroups("Master")[0];
        _musicGroup       = _audioMixer.FindMatchingGroups("Music")[0];
        _playerSpaceGroup = _audioMixer.FindMatchingGroups("PlayerSpace")[0];
        _screenSpaceGroup = _audioMixer.FindMatchingGroups("ScreenSpace")[0];
        _3dGroup          = _audioMixer.FindMatchingGroups("3dSpace")[0];
        
        Debug.Assert(_masterGroup      != null, "The 'Master' audio mixer group cant be found!");
        Debug.Assert(_playerSpaceGroup != null, "The 'Player Space' audio mixer group cant be found!");
        Debug.Assert(_screenSpaceGroup != null, "The 'Screen Space' audio mixer group cant be found!");
        Debug.Assert(_3dGroup          != null, "The '3D Space' audio mixer group cant be found!");
        
        // Init Player Space Audio Source
        _playerSpaceSource = SpawnAudioSource(_playerSpaceGroup, "PLAYER SPACE AUDIO SOURCE");
        _playerSpaceSource.panStereo    = 0.0f;
        _playerSpaceSource.spatialize   = false;
        _playerSpaceSource.spatialBlend = 0.0f;
        
        // Init Screen Space Audio Source
        _screenSpaceSource = SpawnAudioSource(_screenSpaceGroup, "SCREEN SPACE AUDIO SOURCE");
        _screenSpaceSource.panStereo    = 0.0f;
        _screenSpaceSource.spatialize   = false;
        _screenSpaceSource.spatialBlend = 0.0f;
        
        // Init 3D Audio Sources
        _3dAudioSourcePool     = new List<AudioSource>(_initialAudioSourcePoolCount);
        s_active3dAudioSources = new Dictionary<AudioSource3D, AudioSourceRef>();
        for (int i = 0; i < _initialAudioSourcePoolCount; i++)
        {
            Spawn3dAudioSource();
        }
        
        // Sub to scene loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (s_instance != this) return;
        
        s_instance                = null;
        s_active3dAudioSources    = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // NOTE(WSWhitehouse): Shouldn't have to destroy the audio sources as if this object is being destroyed we know
        // its the end of the game (this is DontDestroyOnLoad) so the audio sources will be destroyed alongside...
    }
    
    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    { 
        s_instance._playerManager = FindObjectOfType<FourPlayerManager>();
    }

    private void Update()
    {
        UpdateWorldSpaceAudioSources();
    }
    
    private void UpdateWorldSpaceAudioSources()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MuteAllSources()
        { 
            // NOTE(WSWhitehouse): Unfortunately, we have to use a foreach loop to iterate through a dictionary!
            foreach (KeyValuePair<AudioSource3D,AudioSourceRef> keyValuePair in s_active3dAudioSources)
            {
                AudioSourceRef sourceRef    = keyValuePair.Value;
                ref AudioSource audioSource = ref sourceRef.audioSource;
                audioSource.mute = true;
            }
        }
        
        // NOTE(WSWhitehouse): If there is no active player manager in this scene then we can't have 3D audio
        if (_playerManager == null)
        {
            MuteAllSources();
            return;
        }
        
        List<PlayerProfile> players = _playerManager.Players;
        int playerCount = players.Count;
        
        if (playerCount <= 0)
        {
            MuteAllSources();
            return;
        }

        // NOTE(WSWhitehouse): Unfortunately, we have to use a foreach loop to iterate through a dictionary!
        foreach (KeyValuePair<AudioSource3D,AudioSourceRef> keyValuePair in s_active3dAudioSources)
        {
            AudioSourceRef sourceRef        = keyValuePair.Value;
            ref AudioSource audioSource     = ref sourceRef.audioSource;
            ref AudioSource3D audioSource3D = ref sourceRef.audioSource3D;
            
            // if (!audioSource.isPlaying) continue; 

            float3 source3dPos = audioSource3D.transform.position;
            
            float3 firstPlayerPos = players[0].transform.position;
            float dist = math.distance(source3dPos, firstPlayerPos);
            float3 dir = source3dPos - firstPlayerPos;
            
            for (int i = 1; i < playerCount; i++)
            {
                float3 playerPosition = players[i].transform.position;
                float playerDist = math.distance(source3dPos, playerPosition);
                
                if (playerDist < dist)
                {
                    dist = playerDist;
                    dir  = source3dPos - playerPosition;
                }
            }

            if (dist >= audioSource3D.MaxDistance)
            {
                audioSource.mute = true;
                continue;
            }
            
            audioSource.mute = audioSource3D.Mute;
            dir = math.normalizesafe(dir);

            Transform sourceTransform = audioSource.transform;
            sourceTransform.localPosition = dir * dist;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AudioSource SpawnAudioSource(AudioMixerGroup mixerGroup, string name = "AUDIO SOURCE")
    {
        AudioSource newSource = Instantiate(_audioSourcePrefab, this.transform);
        newSource.gameObject.name       = name;
        newSource.outputAudioMixerGroup = mixerGroup;
        newSource.playOnAwake           = false;
        return newSource;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Spawn3dAudioSource()
    {
        AudioSource source = SpawnAudioSource(_3dGroup, "3D AUDIO SOURCE");
        source.panStereo    = 0.0f;
        source.spatialize   = true;
        source.spatialBlend = 1.0f;
        
        _3dAudioSourcePool.Add(source);
    }
    
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Registers an audio source 3D with the internal data structure.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DRegister(AudioSource3D source3D)
    {
        Debug.Assert(s_instance != null, "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
        
        if (s_instance._3dAudioSourcePool.Count <= 0)
        {
           // //Debug.Log("Audio Manager: Resizing the 3D Audio Source Pool! Consider increasing the initial spawn amount.");
            int newCapacity = s_instance._3dAudioSourcePool.Capacity * s_instance._audioSourceResizeFactor;
            for (int i = 0; i < newCapacity; i++)
            {
                s_instance.Spawn3dAudioSource();
            }
        }
        
        AudioSource source = s_instance._3dAudioSourcePool[0];
        s_instance._3dAudioSourcePool.RemoveAt(0);
        
        s_active3dAudioSources.Add(
            source3D,
            new AudioSourceRef
            {
                audioSource3D = source3D,
                audioSource = source
            }
        );
        
        AudioSource3DUpdate(source3D);
    }
        
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Unregisters an audio source 3D with the internal data structure.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DUnregister(AudioSource3D source3D)
    {
        if (s_instance == null) return;
        if (!s_active3dAudioSources.ContainsKey(source3D)) return;
        
        AudioSource source = s_active3dAudioSources[source3D].audioSource;
        if (source.isPlaying) { source.Stop(); }
        
        s_instance._3dAudioSourcePool.Add(source);

        s_active3dAudioSources.Remove(source3D);
    }
            
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Updates the internal audio source with the values from the 3D audio source,
    /// this is automatically invoked when updating a value on the AudioSource3D.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DUpdate(AudioSource3D source3D)
    {
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;
        
        audioSource.mute         = source3D.Mute;
        audioSource.loop         = source3D.Loop;
        audioSource.dopplerLevel = source3D.DopplerLevel;
        audioSource.spread       = source3D.Spread;
        audioSource.rolloffMode  = source3D.VolumeRolloff;
        audioSource.minDistance  = source3D.MinDistance;
        audioSource.maxDistance  = source3D.MaxDistance;
    }

    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// </summary>
    /// <param name="source3D"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static bool AudioSource3DGetUpdate(AudioSource3D source3D, out float time)
    {
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;

        time = audioSource.time;
        return audioSource.isPlaying;
    }
    
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Plays audio on the internal audio source from the 3D source.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    /// <param name="delayInSeconds">Delay in seconds before playback begins.</param>
    public static void AudioSource3DPlay(AudioSource3D source3D, float delayInSeconds)
    {
        Debug.Assert(s_instance != null, "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
        
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;
        
        Audio.Clip clip    = source3D.AudioClip.RandomlySelectClip();
        audioSource.clip   = clip.audioClip;
        audioSource.volume = clip.volumeRange.Random();
        audioSource.pitch  = clip.pitchRange.Random();
        
        if (delayInSeconds > float.Epsilon)
        {
            audioSource.PlayDelayed(delayInSeconds);
            return;
        }
        
        audioSource.Play();
    }
    
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Stops playing audio on the internal audio source.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DStop(AudioSource3D source3D)
    {
        Debug.Assert(s_instance != null, "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");       
        
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;
        
        audioSource.Stop();
    }
    
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Pauses audio source playback.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DPause(AudioSource3D source3D)
    {
        Debug.Assert(s_instance != null, "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
                
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;
        
        audioSource.Pause();
    }
    
    /// <summary>
    /// DO NOT INVOKE THIS FUNCTION! IT IS INTERNAL TO THE AUDIO SYSTEM!
    /// Resumes audio source playback.
    /// </summary>
    /// <param name="source3D">Audio source 3D reference.</param>
    public static void AudioSource3DUnpause(AudioSource3D source3D)
    {
        Debug.Assert(s_instance != null, "Audio Manager is not initialised! Please place an AudioManager prefab in the scene!");
                
        AudioSourceRef audioSourceRef = s_active3dAudioSources[source3D];
        ref AudioSource audioSource   = ref audioSourceRef.audioSource;
        
        audioSource.UnPause();
    }
}
