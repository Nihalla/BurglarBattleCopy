// Author: William Whitehouse (WSWhitehouse)

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "New Audio", menuName = "Audio Clip", order = 0)]
public class Audio : ScriptableObject
{
    [Serializable]
    public class Clip
    {
        public AudioClip audioClip = null; 
        
        [Header("General Settings")]
        [Tooltip("The weighting of this audio clip in comparison to the others in the audio clips array.\n 0 = low weighting, 1 = high weighting.")]
        [Range(0.0f, 1.0f)] public float clipWeighting = 1.0f;
        
        [Header("Sound Settings")]
        [Tooltip("The volume range for this audio clip. A value is randomly chosen between the range each time this clip is played.")]
        [RangedType(0.0f, 1.0f)] public RangedFloat volumeRange = new RangedFloat(1.0f, 1.0f);
        
        [Tooltip("The pitch range for this audio clip. A value is randomly chosen between the range each time this clip is played.")]
        [RangedType(-3.0f, 3.0f)] public RangedFloat pitchRange = new RangedFloat(1.0f, 1.0f);
    }
    
    [SerializeField] private Clip[] _audioClips = Array.Empty<Clip>();
    public Span<Clip> Clips => _audioClips.AsSpan();
    
    public Clip RandomlySelectClip()
    {
#if UNITY_EDITOR
        Debug.Assert(_audioClips != null,    $"The Audio Clips array is null on '{name}'! Please add a clip!");
        Debug.Assert(_audioClips.Length > 0, $"The Audio Clips array has a length of '0' on '{name}'! Please add a clip!");
        
        // NOTE(WSWhitehouse): In editor ONLY, checking all clips have valid values. As some people have been running
        // into issues where clips have a pitch/volume of 0! 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckPitchValue(ref Clip clip)
        {
            if (math.abs(clip.pitchRange.minValue) <= float.Epsilon && 
                math.abs(clip.pitchRange.maxValue) <= float.Epsilon)
            {
                Debug.LogError($"Audio Clip \"{clip.audioClip.name}\" has a pitch value of '0'. This audio clip won't play, try changing the pitch value!");
                return;
            }
            
            if (clip.pitchRange.InRange(0.0f))
            {
                Debug.LogWarning($"Audio Clip \"{clip.audioClip.name}\" pitch value contains '0' as a possible random value! This may result in the audio not playing.");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckVolumeValue(ref Clip clip)
        {
            if (math.abs(clip.volumeRange.minValue) <= float.Epsilon && 
                math.abs(clip.volumeRange.maxValue) <= float.Epsilon)
            {
                Debug.LogError($"Audio Clip \"{clip.audioClip.name}\" has a volume value of '0'!");
                return;
            }
            
            if (clip.volumeRange.InRange(0.0f))
            {
                Debug.LogWarning($"Audio Clip \"{clip.audioClip.name}\" volume value contains '0' as a possible random value! This may result in the audio being silent in some cases.");
            }
        }
        
        for (int i = 0; i < _audioClips.Length; i++)
        {
            ref Clip clip = ref _audioClips[i];
            CheckPitchValue(ref clip);
            CheckVolumeValue(ref clip);
        }
#endif // UNITY_EDITOR
        
        // NOTE(WSWhitehouse): No need to perform the weighting calculations if there is only one clip in the array...
        if (_audioClips.Length == 1) return _audioClips[0];

        // Selecting a clip using The Alias method for random sampling with probability distribution (different weightings)
        // https://en.wikipedia.org/wiki/Alias_method
        // https://stackoverflow.com/questions/872563/efficient-algorithm-to-randomly-select-items-with-frequency
        
        float maxWeighting = 0.0f;
        for (int i = 0; i < _audioClips.Length; i++)
        {
            maxWeighting += _audioClips[i].clipWeighting;
        }
        
        float randomFreq = UnityEngine.Random.Range(0.0f, maxWeighting);

        float freqSum = 0.0f;
        for (int i = 0; i < _audioClips.Length; i++)
        {
            freqSum += _audioClips[i].clipWeighting;
            if (freqSum > randomFreq)
            {
                return _audioClips[i];
            }
        }
        
#if UNITY_EDITOR
        Debug.LogError($"Unable to randomly select clip based on weightings from '{name}'! This is likely because all clips have a weighting of 0! Returning the first clip for now.");
#endif // UNITY_EDITOR
        
        return _audioClips[0];
    }

#if UNITY_EDITOR
    private int EDITOR_audioClipsLength = -1; 
    
    private void OnValidate()
    {
        // NOTE(WSWhitehouse): Ensure we initialise the length to the actual value when the 
        // first OnValidate call is called. This ensures we don't overwrite actual data.
        if (EDITOR_audioClipsLength == -1)
        {
            EDITOR_audioClipsLength = _audioClips.Length;
        }
        
        // A new element has been added...
        if (EDITOR_audioClipsLength < _audioClips.Length)
        {
            // NOTE(WSWhitehouse): Calling "new Clip()" here sets the newly added clip to
            // its default values rather than copy the previous element (which is the default
            // behaviour in Unity).
            _audioClips[^1] = new Clip();
        }
        
        EDITOR_audioClipsLength = _audioClips.Length;
    }
#endif // UNITY_EDITOR
}
