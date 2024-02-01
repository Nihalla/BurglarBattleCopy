// Author: Sebastian Adamatzky

using System;
using UnityEngine;

public class MultiHoldButton : MonoBehaviour, IInteractable
{
    [Header("Object References")]
    [Tooltip("Any Mesh Renderer components included in this array will have the hover effect applied.")]
    [SerializeField] private MeshRenderer[] _onHoverMeshRenderers = Array.Empty<MeshRenderer>();
    [SerializeField] private MeshRenderer _meshRenderer;
    
    [SerializeField] private Material _materialOff;
    [SerializeField] private Material _materialOn;
    
    [Header("Testing Settings")]
    [Tooltip("If on, the button will stay enabled once the scene starts")]
    //NOTE(Sebadam2010): This is for testing purposes while we don't have more than one playable character in a scene 
    public bool invokeOnButtonStateChange = false;

    public delegate void MultiHoldButtonDel(bool isOn);
    public MultiHoldButtonDel onButtonStateChangeEvent; 
    
    public bool IsOn { get; private set; } = false;
    
    private void Awake()
    {
        onButtonStateChangeEvent += SwitchMaterial;
    }

    private void OnDestroy()
    {
        onButtonStateChangeEvent -= SwitchMaterial;
    }

    private void Start()
    {
        if (invokeOnButtonStateChange)
        {
            onButtonStateChangeEvent?.Invoke(true);
        }
    }
    
    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _onHoverMeshRenderers.AsSpan();
    }
    
    public void OnInteractHoldStarted(PlayerInteraction playerInteraction)
    {
        IsOn = true;
        
        onButtonStateChangeEvent?.Invoke(IsOn);
    }

    public void OnInteractHoldEnded(PlayerInteraction playerInteraction)
    {
        IsOn = false;
        
        onButtonStateChangeEvent?.Invoke(IsOn); 
    }

    private void SwitchMaterial(bool isButtonOn)
    {
        if (isButtonOn)
        {
            _meshRenderer.material = _materialOn;
        }
        else
        {
            _meshRenderer.material = _materialOff;
        }
    }
}
