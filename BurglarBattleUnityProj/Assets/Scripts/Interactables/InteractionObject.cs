using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple interaction object component, allows interactions to be built up from
/// this small component. Any complicated interactions should be managed separately.
/// Events exposed in the inspector allows other actions to be invoked when the object
/// this component is attached to is interacted with. See <see cref="_onInteractEvent"/>,
/// <see cref="_onHoldStartedEvent"/>, <see cref="_onHoldEndedEvent"/>,
/// <see cref="_onHoverStartedEvent"/> and <see cref="_onHoverEndedEvent"/>.
/// </summary>
public class InteractionObject : MonoBehaviour, IInteractable
{
    [Header("Object References")]
    [Tooltip("Any Mesh Renderer components included in this array will have the hover effect applied.")]
    [SerializeField] private MeshRenderer[] _onHoverMeshRenderers = Array.Empty<MeshRenderer>();
    
    [Header("Events")]
    public UnityEvent _onInteractEvent     = new UnityEvent();
    public UnityEvent _onHoldStartedEvent  = new UnityEvent();
    public UnityEvent _onHoldEndedEvent    = new UnityEvent();
    public UnityEvent _onHoverStartedEvent = new UnityEvent();
    public UnityEvent _onHoverEndedEvent   = new UnityEvent();
    
    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _onHoverMeshRenderers.AsSpan();
    }
    
    public void OnInteract(PlayerInteraction playerInteraction)
    {
        _onInteractEvent?.Invoke();
    }
    
    public void OnInteractHoldStarted(PlayerInteraction playerInteraction)
    {
        _onHoldStartedEvent?.Invoke();
    }
    
    public void OnInteractHoldEnded(PlayerInteraction playerInteraction)
    {
        _onHoldEndedEvent?.Invoke();
    }
    
    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        _onHoverStartedEvent?.Invoke();
    }
    
    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        _onHoverEndedEvent?.Invoke();
    }
}