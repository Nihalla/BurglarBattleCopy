// Author: William Whitehouse (WSWhitehouse)

using System;
using UnityEngine;

public class InteractTest : MonoBehaviour, IInteractable
{
    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
    [Space]
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _holdMat;
    
    private MeshRenderer _meshRenderer =>_meshRenderers[0];
    private int _currentMat = 1;

    private void Awake()
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }
    
    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
    
    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        // _meshRenderer.sharedMaterial = _hoverMat;
    }
    
    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        // _meshRenderer.sharedMaterial = _defaultMat;
    }


    public void OnInteractHoldStarted(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _holdMat;
    }
    
    public void OnInteractHoldEnded(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }
}
