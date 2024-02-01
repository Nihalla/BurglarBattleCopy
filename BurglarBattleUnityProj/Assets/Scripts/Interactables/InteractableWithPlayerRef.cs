using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerReferenceEvent : UnityEvent<PlayerInteraction>
{
}

public class InteractableWithPlayerRef : MonoBehaviour, IInteractable
{
    [Header("Object References")]
    [Tooltip("Any Mesh Renderer components included in this array will have the hover effect applied.")]
    [SerializeField] private MeshRenderer[] _onHoverMeshRenderers = Array.Empty<MeshRenderer>();
    
    [Header("Events")]
    public PlayerReferenceEvent _onInteractEvent     = new PlayerReferenceEvent();
    public PlayerReferenceEvent _onHoldStartedEvent  = new PlayerReferenceEvent();
    public PlayerReferenceEvent _onHoldEndedEvent    = new PlayerReferenceEvent();
    public PlayerReferenceEvent _onHoverStartedEvent = new PlayerReferenceEvent();
    public PlayerReferenceEvent _onHoverEndedEvent   = new PlayerReferenceEvent();
    
    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _onHoverMeshRenderers.AsSpan();
    }
    
    public void OnInteract(PlayerInteraction playerInteraction)
    {
        _onInteractEvent?.Invoke(playerInteraction);
    }
    
    public void OnInteractHoldStarted(PlayerInteraction playerInteraction)
    {
        _onHoldStartedEvent?.Invoke(playerInteraction);
    }
    
    public void OnInteractHoldEnded(PlayerInteraction playerInteraction)
    {
        _onHoldEndedEvent?.Invoke(playerInteraction);
    }
    
    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        _onHoverStartedEvent?.Invoke(playerInteraction);
    }
    
    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        _onHoverEndedEvent?.Invoke(playerInteraction);
    }
}