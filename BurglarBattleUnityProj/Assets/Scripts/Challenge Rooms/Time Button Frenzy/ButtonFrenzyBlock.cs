using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ButtonFrenzyBlock : MonoBehaviour, IInteractable
{
    public enum BlockState
    {
        OFF,
        ON,
        PRESSED
    }
    
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _pressedMaterial;
    [SerializeField] private Audio _onInteractAudioClip;
    [SerializeField] private Audio _onCorrectInteractAudioClip;
    
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


    public delegate void BlockPressedDel();
    public event BlockPressedDel OnBlockPressedEvent;

    private MeshRenderer[] _meshRenderers;
    
    private BlockState _blockState;
    
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void ChangeState(BlockState blockState)
    {
        switch (blockState)
        {
            case BlockState.OFF:
                _blockState = BlockState.OFF;
                break;
            case BlockState.ON:
                _blockState = BlockState.ON;
                break;
            case BlockState.PRESSED:
                _blockState = BlockState.PRESSED;
                OnBlockPressedEvent?.Invoke();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(blockState), blockState, null);
        }
        ChangeColor(_blockState);
    }

    private void ChangeColor(BlockState blockState)
    {
        switch (blockState)
        {
            case BlockState.OFF:
                _meshRenderer.material = _offMaterial;
                break;
            case BlockState.ON:
                _meshRenderer.material = _onMaterial;
                break;
            case BlockState.PRESSED:
                _meshRenderer.material = _pressedMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(blockState), blockState, null);
        }
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (_blockState == BlockState.ON)
        {
            AudioManager.PlayOneShotWorldSpace(_onCorrectInteractAudioClip, transform.position);
            
            if (_rumbleEnabled)
            {
                InputDevices.Devices[playerInteraction.PlayerProfile.GetPlayerID()].RumblePulse(_rumbleLeft, _rumbleRight, _rumbleDuration, this, false);
            }
            ChangeState(BlockState.PRESSED);
        }
        else
        {
            AudioManager.PlayOneShotWorldSpace(_onInteractAudioClip, transform.position);   
        }
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
}
