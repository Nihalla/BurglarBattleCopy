using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhackAMoleBlock : MonoBehaviour, IInteractable
{
    [SerializeField] private Material _disabledMaterial;
    [SerializeField] private Material _enabledMaterial;
    [SerializeField] private Material _completedMaterial;
    [SerializeField] private Audio _onCorrectInteractAudioClip;
    [SerializeField] private Audio _OnInteractAudioClip;
    
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

    public delegate void BlockPressedDel(WhackAMoleBlock whackAMoleBlock);
    public event BlockPressedDel OnBlockPressedEvent;
    
    private ChangeButtonColour _changeButtonColour;

    private MeshRenderer _meshRenderer;
    private MeshRenderer[] _meshRenderers;
    
    private ButtonState _buttonState;

    private Coroutine _flashBlockCoroutine;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void ChangeState(ButtonState buttonState)
    {
        _buttonState = buttonState;

        switch (_buttonState)
        {
            case ButtonState.DISABLED:
                ChangeColor(_buttonState);
                break;
            case ButtonState.ENABLED:
                ChangeColor(_buttonState);
                break;
            case ButtonState.PRESSED:
                ChangeState(ButtonState.DISABLED);
                OnBlockPressedEvent?.Invoke(this);
                break;
            case ButtonState.COMPLETED:
                ChangeColor(_buttonState);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
        }
    }

    private void ChangeColor(ButtonState buttonState)
    {
            switch (buttonState)
        {
            case ButtonState.DISABLED:
                _meshRenderer.material = _disabledMaterial;
                break;
            case ButtonState.ENABLED:
                _meshRenderer.material = _enabledMaterial;
                break;
            case ButtonState.COMPLETED:
                _meshRenderer.material = _completedMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
        }
    }

    public void ChangeColor(Material material)
    {
        _meshRenderer.material = material;
    }

    public void StopFlash()
    {
        if (_flashBlockCoroutine != null)
        {
            StopCoroutine(_flashBlockCoroutine);
            _flashBlockCoroutine = null;
        }
    }

    //This function is used to wait for the flash to finish before continuing.
    public bool ReturnWhenFlashBlockFinished()
    {
        while (_flashBlockCoroutine != null)
        {
        }
        return true;
    }
    
    public void Flash(float flashLength)
    {
        if (_flashBlockCoroutine != null)
        {
            StopCoroutine(_flashBlockCoroutine);
            _flashBlockCoroutine = null;
        }
        
        _flashBlockCoroutine = StartCoroutine(FlashBlock(flashLength));
    }

    private IEnumerator FlashBlock(float flashLength)
    {
        float timer = 0f;
        
        ChangeState(ButtonState.ENABLED);
        while (timer < flashLength)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        ChangeState(ButtonState.DISABLED);
        _flashBlockCoroutine = null;
    }
    
    public ButtonState GetButtonState()
    {
        return _buttonState;
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (_buttonState == ButtonState.ENABLED)
        {
            AudioManager.PlayOneShotWorldSpace(_onCorrectInteractAudioClip, transform.position);
            StopFlash();
            
            if (_rumbleEnabled)
            {
                InputDevices.Devices[playerInteraction.PlayerProfile.GetPlayerID()].RumblePulse(_rumbleLeft, _rumbleRight, _rumbleDuration, this, false);
            }
            
            ChangeState(ButtonState.PRESSED);
        }
        else
        {
            AudioManager.PlayOneShotWorldSpace(_OnInteractAudioClip, transform.position);
        }
    }

    public float GetInteractionDistance()
    {
        return IInteractable.DEFAULT_INTERACTABLE_DISTANCE * 2.0f;
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
}
