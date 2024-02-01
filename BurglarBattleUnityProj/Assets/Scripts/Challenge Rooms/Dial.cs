//Author: Norbert Kupeczki - 19040948

using System;
using UnityEngine;

public class Dial : MonoBehaviour, IInteractable
{
    [SerializeField] private int _dialID;
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _holdMat;

    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
    private MeshRenderer _meshRenderer => _meshRenderers[0];


    public Action<int> DialInteraction;

    Span<MeshRenderer> IInteractable.GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        DialInteraction?.Invoke(_dialID);
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteractHoverStarted()
    {
        _meshRenderer.sharedMaterial = _hoverMat;
    }

    public void OnInteractHoverEnded()
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }
}
