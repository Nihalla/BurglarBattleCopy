using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPuzzleBlock : MonoBehaviour, IInteractable
{
    public enum BlockState
    {
        OFF,
        ON,
        INCORRECT,
        CORRECT
    }
    
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Material _incorrectMaterial;
    [SerializeField] private Material _correctMaterial;

    public delegate void BlockDel();

    public BlockDel OnCorrectBlockPressed;
    public BlockDel OnWrongBlockPressed;

    private Coroutine _flashBlockCoroutine;

    private bool _isInSequence = false;
    private bool _isPressed = false;
    private bool _isPressable = false;
    
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
            default:
                throw new ArgumentOutOfRangeException(nameof(blockState), blockState, null);
        }
    }

    public void FlashBlock(float flashTime, BlockState blockState)
    {
        if (_flashBlockCoroutine != null)
        {
            StopCoroutine(_flashBlockCoroutine);
        }
        StartCoroutine(FlashBlockCoroutine(flashTime, blockState));
    }

    private IEnumerator FlashBlockCoroutine(float flashTime, BlockState blockState)
    {
        ChangeColour(blockState);
        float timer = 0;
        
        while (timer < flashTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        ChangeColour(BlockState.OFF);
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (!_isPressable)
        {
            return;
        }
        
        _isPressed = true;
        ChangeColour(BlockState.ON);
        
        if (!_isInSequence)
        {
            OnWrongBlockPressed?.Invoke();
        }
        else
        {
            OnCorrectBlockPressed?.Invoke();
        }

        _isPressable = false;
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
}
