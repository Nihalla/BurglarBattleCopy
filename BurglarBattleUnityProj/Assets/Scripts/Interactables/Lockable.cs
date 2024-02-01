using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A "middle-man" component for the interaction system, allows objects to be
/// marked as lockable. And through the exposed events in the inspector actions
/// can be invoked depending on this components state. See <see cref="onLocked"/>
/// and <see cref="onUnlocked"/> for locking based events. See <see cref="onInteract"/>
/// and <see cref="onFailedInteract"/> that get invoked when there has been an
/// interaction with this component. The <see cref="Lock"/>, <see cref="Unlock"/>
/// and <see cref="ToggleLock"/> functions are designed to be called from other
/// scripts or be subscribed to other events to build up complicated logic.
/// </summary>
public class Lockable : MonoBehaviour
{
    [Header("Locking Settings")]
    [SerializeField] private bool _isLocked = false;
    
    [Header("Locking Events")]
    public UnityEvent onLocked   = new UnityEvent();
    public UnityEvent onUnlocked = new UnityEvent();
    
    [Header("Interact Events")]
    public UnityEvent onInteract       = new UnityEvent();
    public UnityEvent onFailedInteract = new UnityEvent();
    
    /// <summary>
    /// Is the lockable component locked?
    /// </summary>
    public bool IsLocked => _isLocked;
    
    /// <summary>
    /// Try to interact with this component. If unlocked, the <see cref="onInteract"/> event will 
    /// fire and the interaction will be successful. If locked, the <see cref="onFailedInteract"/>
    /// event will fire and the interaction will fail.
    /// </summary>
    public void TryInteract()
    {
        // REVIEW(WSWhitehouse): Might want to perform the lock pick minigame here...
        if (IsLocked)
        {
            onFailedInteract?.Invoke();
            return;
        }
        
        onInteract?.Invoke();
    }
    
    /// <summary>
    /// Lock the lockable component.
    /// </summary>
    public void Lock()
    {
        // NOTE(WSWhitehouse): Shouldn't be performing locking logic when already locked.
        if (IsLocked) return;
        
        _isLocked = true;
        onLocked?.Invoke();
    }
    
    /// <summary>
    /// Unlock the lockable component.
    /// </summary>
    public void Unlock()
    {
        // NOTE(WSWhitehouse): Shouldn't be performing unlocking logic when already unlocked.
        if (!IsLocked) return;
        
        _isLocked = false;
        onUnlocked?.Invoke();
    }
    
    /// <summary>
    /// Toggle the lockable component into its inverse state. For example, when locked it
    /// will transition to unlocked, and when unlocked it will transition to the locked state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ToggleLock()
    {
        if (IsLocked)
        {
            Unlock();
        }
        else
        {
            Lock();
        }
    }
}
