// Author: William Whitehouse (WSWhitehouse)

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This script detects triggers between gameobjects on certain layers,
/// if the object that collided is not included in the layermask the
/// trigger is ignored. It includes events for the first/last collision
/// inside its collision volume as well as an event for every time the
/// trigger is performed.
/// </summary>
public class LayerMaskTrigger : MonoBehaviour
{
  [SerializeField] private LayerMask _layerMask;
  
  public UnityEvent onFirstEnter = new UnityEvent();
  public UnityEvent onLastExit   = new UnityEvent();
  public UnityEvent onAnyEnter   = new UnityEvent();
  public UnityEvent onAnyExit    = new UnityEvent();
  
  // NOTE(WSWhitehouse): Pass through collider that triggered the event
  public delegate void LayerMaskTriggerEvent(Collider collider);
  
  // NOTE(WSWhitehouse): These events pass through the collider that 
  // triggered the event. These events can only be subscribed through
  // C# code. Use the UnityEvents above for generic events.
  public LayerMaskTriggerEvent onFirstColliderEnter;
  public LayerMaskTriggerEvent onLastColliderExit;
  public LayerMaskTriggerEvent onAnyColliderEnter;
  public LayerMaskTriggerEvent onAnyColliderExit;

  public int ColliderCount { get; private set; } = 0;

  private void OnTriggerEnter(Collider other)
  {
    // NOTE(WSWhitehouse): If colliders layer isn't included in layerMask then ignore it
    if ((_layerMask & (1 << other.gameObject.layer)) == 0) return;
    
    ColliderCount++;
    onAnyEnter?.Invoke();
    onAnyColliderEnter?.Invoke(other);

    if (ColliderCount == 1) // First collider to enter
    {
      onFirstEnter?.Invoke();
      onFirstColliderEnter?.Invoke(other);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    // NOTE(WSWhitehouse): If colliders layer isn't included in layerMask then ignore it
    if ((_layerMask & (1 << other.gameObject.layer)) == 0) return;

    ColliderCount--;
    onAnyExit?.Invoke();
    onAnyColliderExit?.Invoke(other);

    if (ColliderCount <= 0) // Last collider to exit
    {
      onLastExit?.Invoke();
      onLastColliderExit?.Invoke(other);
    }
  }
}
