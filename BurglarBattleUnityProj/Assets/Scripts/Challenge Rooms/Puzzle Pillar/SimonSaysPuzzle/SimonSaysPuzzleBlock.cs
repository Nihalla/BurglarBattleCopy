using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonSaysPuzzleBlock : MonoBehaviour, IInteractable
{
    public enum BlockState
    {
        OFF,
        ON,
        INCORRECT,
        CORRECT,
        SEQUENCE_SHOW
    }
    
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Material _incorrectMaterial;
    [SerializeField] private Material _correctMaterial;
    [SerializeField] private Material _sequenceShowMaterial;

    [SerializeField] private Audio _onFlashAudioClip;
    [SerializeField] private Audio _onCorrectInteractAudioClip;
    [SerializeField] private Audio _onIncorrectInteractAudioClip;
    
    [Tooltip("Rumble settings")]
    [SerializeField]
    private bool _rumbleEnabled = true;

    [SerializeField]
    [ToggleField("_rumbleEnabled", true)]
    [Range(0.0f, 1.0f)]
    [Tooltip("The strength of the rumble on left/right side of controller")]
    private float _rumbleLeft = 0.2f, _rumbleRight = 0.2f;
    [SerializeField] [ToggleField("_rumbleEnabled", true)][Range(0.0f,1.0f)]
    [Tooltip("The duration of the rumble")]
    private float _rumbleDuration = 0.5f;
    
    public delegate void BlockDel();

    public BlockDel OnCorrectBlockPressed;
    public BlockDel OnWrongBlockPressed;

    private Coroutine _flashBlockCoroutine;

    private bool _isInSequence = false;
    private bool _isPressed = false;
    private bool _isPressable = false;

    private bool _interrupt = false;
    
    public bool IsInSequence
    {
        get => _isInSequence;
        set => _isInSequence = value;
    }
    
    public bool IsPressed
    {
        get => _isPressed;
        set => _isPressed = value;
    }
    
    public bool IsPressable
    {
        get => _isPressable;
        set => _isPressable = value;
    }

    private MeshRenderer[] _meshRenderers;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
    
    

    public void ChangeColour(BlockState blockState)
    {
        switch (blockState)
        {
            case BlockState.OFF:
                _meshRenderer.material = _offMaterial;
                break;
            case BlockState.ON:
                _meshRenderer.material = _onMaterial;
                break;
            case BlockState.INCORRECT:
                _meshRenderer.material = _incorrectMaterial;
                break;
            case BlockState.CORRECT:
                _meshRenderer.material = _correctMaterial;
                break;
            case BlockState.SEQUENCE_SHOW:
                _meshRenderer.material = _sequenceShowMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(blockState), blockState, null);
        }
    }

    public void FlashBlock(float flashTime, BlockState blockState, bool pressableAfterFlash)
    {
         
        if (_flashBlockCoroutine != null)
        {
            StopCoroutine(_flashBlockCoroutine);
        }
        _flashBlockCoroutine = StartCoroutine(FlashBlockCoroutine(flashTime, blockState, pressableAfterFlash));
    }

    public void StopBlockFlash()
    {
        _interrupt = true;
        if (_flashBlockCoroutine != null)
        {
            StopCoroutine(_flashBlockCoroutine);
        }
        
        _interrupt = false;
    }

    private IEnumerator FlashBlockCoroutine(float flashTime, BlockState blockState, bool pressableAfterFlash)
    {
        AudioManager.PlayOneShotWorldSpace(_onFlashAudioClip, transform.position);
        
        _isPressable = false;   
        ChangeColour(blockState);
        float timer = 0;
        
        while (timer < flashTime && !_interrupt)
        {
            timer += Time.deltaTime;
            yield return null;
        }
       
        ChangeColour(BlockState.OFF);
        
        if (pressableAfterFlash)
        {
            _isPressable = true;
        }
        else
        {
            _isPressable = false;
        }
        

    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (!_isPressable)
        {
            return;
        }
        
        
        
        FlashBlock(0.5f, BlockState.ON, true);
        
        if (!_isInSequence)
        {
            OnWrongBlockPressed?.Invoke();
            AudioManager.PlayOneShotWorldSpace(_onIncorrectInteractAudioClip, transform.position);
        }
        else
        {
            if (_rumbleEnabled)
            {
                InputDevices.Devices[playerInteraction.PlayerProfile.GetPlayerID()].RumblePulse(_rumbleLeft, _rumbleRight, _rumbleDuration, this, false);
            }
            
            OnCorrectBlockPressed?.Invoke();
            AudioManager.PlayOneShotWorldSpace(_onCorrectInteractAudioClip, transform.position);
        }

    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
    
}
