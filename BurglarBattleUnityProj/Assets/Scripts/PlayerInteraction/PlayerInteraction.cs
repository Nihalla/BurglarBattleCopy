// Author: William Whitehouse (WSWhitehouse)

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script should be placed on a player, it manages interacting with objects
/// in the scene using the <see cref="IInteractable"/> interface.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerProfile _playerProfile;
    [SerializeField] private Camera _camera;
    [SerializeField] private Material _hoverMaterial;
    [SerializeField] private Transform _holdPointTransform;
    
    [Header("Interact Settings")]
    [SerializeField] private float _interactRadius = 0.25f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask _interactLayerMask;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.UseGlobal;
    
    [Header("Audio")]
    [SerializeField] private Audio _interactSuccessAudioClip;
    [SerializeField] private Audio _interactFailedAudioClip;
    
    [Header("Device Rumble")]
    [SerializeField] private float _onHoverLeftFreq  = 0.5f; 
    [SerializeField] private float _onHoverRightFreq = 0.5f; 
    [SerializeField] private float _onHoverDuration  = 0.15f; 
    
    /// <summary>
    /// The transform component of the hold point for picking up objects.
    /// </summary>
    public Transform HoldPointTransform => _holdPointTransform;
    
    /// <summary>
    /// Get the player profile associated with this PlayerInteraction.
    /// </summary>
    public PlayerProfile PlayerProfile => _playerProfile;
    
    // NOTE(WSWhitehouse): The following variables hold what interactable object the player is
    // looking at in the current frame and from the previous frame. This allows us to perform
    // some checks in update to know when to start interactable events such as hover. They can
    // be null.
    private IInteractable _currentInteractable;
    private IInteractable _prevInteractable;
    
    // NOTE(WSWhitehouse): This tracks if the current interactable is a pickup, as these are
    // handled differently to other interactions...
    private bool _currentInteractableIsPickUp = false;
    
    private bool _currentCanInteract = false;
    private bool _interactingPressed = false;
    
    private int playerID              => _playerProfile.GetPlayerID();
    private DeviceData device         => InputDevices.Devices[playerID];
    private InputActions inputActions => device.Actions;
    
    private delegate IEnumerator EnableInteractionNextFrameDel(int frameCount);
    private EnableInteractionNextFrameDel _enableInteractionNextFrameFunc;
    private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
    
    private delegate IEnumerator EnableInteractionAfterDurationDel(float duration);
    private EnableInteractionAfterDurationDel _enableInteractionAfterDurationFunc;

    private void Awake()
    {
        Debug.Assert(_interactLayerMask.value != 0,    "Interact Layer Mask is set to Nothing! This probably isn't setup correctly, check the PlayerInteraction script attached to the player object...");
        Debug.Assert(_playerProfile           != null, "Player Profile is null on Player Interaction! Please assign one...");
        Debug.Assert(_camera                  != null, "Camera is null on Player Interaction! Please assign one...");
        Debug.Assert(_hoverMaterial           != null, "Hover Material is null on Player Interaction! Please assign one...");
        Debug.Assert(_holdPointTransform      != null, "Hold Point Transform is null on Player Interaction! Please assign one...");
        
        _enableInteractionNextFrameFunc     = EnableInteractionNextFrameCoroutine;
        _enableInteractionAfterDurationFunc = EnableInteractionAfterDurationCoroutine;
    }
    
    private void Start()
    {
        // NOTE(WSWhitehouse): Make sure the player ID is valid before attempting to subscribe to events.
        if (playerID < 0 || playerID >= InputDevices.MAX_DEVICE_COUNT || device == null) return;
        
        inputActions.PlayerInteraction.Enable();
        inputActions.PlayerInteraction.Interact.performed += OnInteractPerformed;
        inputActions.PlayerInteraction.Interact.canceled  += OnInteractCancelled;
        // inputActions.PlayerController.Throw.performed     += ThrowObjectFunc;
    }

    private void OnDestroy()
    {
        // NOTE(WSWhitehouse): Make sure the player ID is valid before attempting to unsubscribe from events.
        if (playerID < 0 || playerID >= InputDevices.MAX_DEVICE_COUNT || device == null) return;
        
        inputActions.PlayerInteraction.Interact.performed -= OnInteractPerformed;
        inputActions.PlayerInteraction.Interact.canceled  -= OnInteractCancelled;
        // inputActions.PlayerController.Throw.performed     -= ThrowObjectFunc;
    }
    
    /// <summary>
    /// Enable the interaction system for this player.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableInteraction() => inputActions.PlayerInteraction.Enable();

    /// <summary>
    /// Enable the interaction system for this player on the next frame. Technically, multiple of
    /// these can be called, and whichever has the shortest frame count takes priority. This will
    /// also overwrite any other calls to Disable in the meantime.
    /// </summary>
    /// <param name="frameCount">How many frames to wait before enabling interaction. Default = 1</param>
    public void EnableInteractionNextFrame(int frameCount = 1)
    {
        StartCoroutine(_enableInteractionNextFrameFunc(frameCount));
    }
    
    /// <summary>
    /// Enable the interaction system for this player after a set duration.Technically, multiple of
    /// these can be called, and whichever has the shortest duration will take priority. This will
    /// also overwrite any other calls to Disable in the meantime.
    /// </summary>
    /// <param name="duration">Duration to wait before enabling interaction.</param>
    public void EnableInteractionAfterDuration(float duration)
    {
        StartCoroutine(_enableInteractionAfterDurationFunc(duration));
    }

    /// <summary>
    /// Disable the interaction system for this player.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DisableInteraction()
    { 
        // NOTE(WSWhitehouse): The pickup interaction uses a toggle on the OnInteract function, so different
        // logic is needed here to ensure the player drops the item when interaction is disabled. This MUST
        // be done before cancelling the interaction to ensure the interaction system is reset properly...
        if (_currentInteractable != null && _currentInteractableIsPickUp)
        {
            PickUpInteractable pickup = (PickUpInteractable)_currentInteractable;
            if (pickup.IsHeld)
            {
                OnInteractPerformed(new InputAction.CallbackContext());
            }
        }
        
        OnInteractCancelled(new InputAction.CallbackContext());
        
        inputActions.PlayerInteraction.Disable();
    }
    
    private IEnumerator EnableInteractionNextFrameCoroutine(int frameCount)
    {
        // NOTE(WSWhitehouse): This ensures we get to the end of the current frame before we begin
        // counting. On some cases this might lead to waiting for an extra frame though...
        yield return _waitForEndOfFrame;
        
        for (int i = 0; i < frameCount; i++)
        {
            yield return null; // Wait for update
        }

        EnableInteraction();
    }
    
    private IEnumerator EnableInteractionAfterDurationCoroutine(float duration)
    {
        float timer = duration;
        while (timer > float.Epsilon)
        {
            timer -= Time.deltaTime;
            yield return null; // Wait for update
        }
        
        EnableInteraction();
    }
    
    /// <summary>
    /// Is the interaction system enabled for this player?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInteractionEnabled() => inputActions.PlayerInteraction.enabled;

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        _interactingPressed = true;
        
        if (_currentInteractable != null && _currentInteractable.CanInteract())
        {
            _currentInteractable.OnInteract(this);
            _currentInteractable.OnInteractHoldStarted(this);
            AudioManager.PlayPlayerSpace(_interactSuccessAudioClip, playerID);
        }
        else
        {
            AudioManager.PlayPlayerSpace(_interactFailedAudioClip, playerID);
        }
    }
    
    private void OnInteractCancelled(InputAction.CallbackContext context)
    {
        _interactingPressed = false;
        
        if (_currentInteractable != null && _currentInteractable.CanInteract())
        {
            _currentInteractable.OnInteractHoldEnded(this);
        }
    }

    private void Update()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void StartInteractionManagement(IInteractable interactable)
        { 
            // NOTE(WSWhitehouse): We don't start holding or interacting if the button was pressed before 
            // the player started looking at this object.
            interactable.OnInteractHoverStarted(this);
            StartHoverHighlight(interactable);
            device.RumblePulse(_onHoverLeftFreq, _onHoverRightFreq, _onHoverDuration, this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void StopInteractionManagement(IInteractable interactable)
        { 
            if (_interactingPressed)
            {
                interactable.OnInteractHoldEnded(this);
            }

            StopHoverHighlight(interactable);
            interactable.OnInteractHoverEnded(this);
        }
        
        // NOTE(WSWhitehouse): The interaction system shouldn't be running if the player is invalid.
        if (playerID < 0 || playerID >= InputDevices.MAX_DEVICE_COUNT || device == null) return;
        
        // NOTE(WSWhitehouse): Performing the "copy" first to allow early returns below, this 
        // wont make a difference to how the following checks are performed.
        _prevInteractable = _currentInteractable;

        InteractRaycast();

        // NOTE(WSWhitehouse): If the player wasn't previously interacting with something
        // and not doing so this frame, return as the next checks are redundant.
        if (_currentInteractable == null && _prevInteractable == null) return;

        // NOTE(WSWhitehouse): If there has been no change in interactable.
        if (_currentInteractable == _prevInteractable)
        {
            // NOTE(WSWhitehouse): Check that the CanInteract state has changed, if so update the
            // interaction state of the IInteractable so it always stays up to date!
            if (_currentCanInteract != _currentInteractable.CanInteract())
            {
                if (_currentCanInteract)
                {
                    StopInteractionManagement(_currentInteractable);
                }
                else
                {
                    StartInteractionManagement(_currentInteractable);
                }

                _currentCanInteract = _currentInteractable.CanInteract();
            }
            
            return;
        }

        // From now on we know the interaction has changed...
        
        if (_prevInteractable != null && _prevInteractable.CanInteract())
        {
            StopInteractionManagement(_prevInteractable);
        }

        if (_currentInteractable != null)
        {
            _currentCanInteract = _currentInteractable.CanInteract();

            if (_currentCanInteract)
            {
                StartInteractionManagement(_currentInteractable);
            }
        }
    }
    
    /// <summary>
    /// Performs the interaction raycast and sets the <see cref="_currentInteractable"/> variable.
    /// </summary>
    private void InteractRaycast()
    {
        if (!IsInteractionEnabled())
        {
            _currentInteractable = null;
            return;
        }

        if (_currentInteractable != null && _currentInteractableIsPickUp)
        {
            // NOTE(WSWhitehouse): Shouldn't search for any other interactables while holding a pickup...
            PickUpInteractable pickup = (PickUpInteractable)_currentInteractable;
            if (pickup.IsHeld) return;
        }
        
        Transform camTransform = _camera.transform;
        Vector3 origin         = camTransform.position;
        Vector3 dir            = camTransform.forward;
        
        // NOTE(WSWhitehouse): Using the Max Distance here so we can pick up all the interactables within
        // the maximum allowed distance and later ensure it's within it's local interaction distance... 
        RaycastHit raycastHit;
        bool hit = Physics.SphereCast(origin, _interactRadius, dir, out raycastHit, 
                                      IInteractable.MAX_INTERACTABLE_DISTANCE,
                                      _interactLayerMask.value, _triggerInteraction);
        
        if (hit)
        {
            IInteractable interactable = raycastHit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                // NOTE(WSWhitehouse): Check that the interactable is within the specified distance...
                float interactDistance = interactable.GetInteractionDistance();
                
                Debug.Assert(interactDistance <= IInteractable.MAX_INTERACTABLE_DISTANCE, 
                    $"The interaction distance on the IInteractable attached to '{raycastHit.collider.gameObject.name}' is greater "   +
                    $"than `IInteractable.MAX_INTERACTABLE_DISTANCE` ({IInteractable.MAX_INTERACTABLE_DISTANCE})! Please ensure this " +
                    $"value is below the max, it will not be detected above this value!");
                
                if (raycastHit.distance <= interactDistance)
                {
                    _currentInteractable         = interactable;
                    _currentInteractableIsPickUp = _currentInteractable is PickUpInteractable;
                    return;
                }
            }
        }

        _currentInteractable         = null;
        _currentInteractableIsPickUp = false;
    }
    
    /// <summary>
    /// Starts the hover highlight effect on the passed in interactable param.
    /// </summary>
    private void StartHoverHighlight(IInteractable interactable)
    {
        if (interactable == null) return;
        
        Span<MeshRenderer> meshRenderers = interactable.GetInteractionMeshRenderers();
        if (meshRenderers == null)     return;
        if (meshRenderers.Length <= 0) return;

        // NOTE(WSWhitehouse): Unfortunately, we've got to allocate an array here to assign
        // or remove any materials... This makes this very messy but it should work!
        for (int i = 0; i < meshRenderers.Length; i++)
        {
           Material[] materials = meshRenderers[i].sharedMaterials;
           
           Material[] newMaterials = new Material[materials.Length + 1];
           Array.Copy(materials, newMaterials, materials.Length);
           newMaterials[^1] = _hoverMaterial;
           
           meshRenderers[i].sharedMaterials = newMaterials;
        }
    }
    
    /// <summary>
    /// Stops the hover highlight effect on the passed in interactable param.
    /// </summary>
    private void StopHoverHighlight(IInteractable interactable)
    {
        if (interactable == null) return;

        // NOTE(WSWhitehouse): Using a try catch here if an object gets destroyed while looking at it
        // it will cause a missing ref exception because it no longer exists. But there is no way to
        // query that nicely - so we're just ignoring that error.
        try
        {
            Span<MeshRenderer> meshRenderers = interactable.GetInteractionMeshRenderers();
            if (meshRenderers == null)     return;
            if (meshRenderers.Length <= 0) return;

            // NOTE(WSWhitehouse): Unfortunately, we've got to allocate an array here to assign
            // or remove any materials... This makes this very messy but it should work!
            for (int i = 0; i < meshRenderers.Length; i++)
            {
               Material[] materials = meshRenderers[i].sharedMaterials;
               
               Material[] newMaterials = new Material[materials.Length - 1];
               Array.Copy(materials, newMaterials, materials.Length - 1);
               
               meshRenderers[i].sharedMaterials = newMaterials;
            }
        }
        catch (MissingReferenceException missingRefException)
        { }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_camera == null) return;

        Transform camTransform = _camera.transform;
        Vector3 origin         = camTransform.position;
        Vector3 dir            = camTransform.forward;
        Vector3 end            = origin + (dir * IInteractable.DEFAULT_INTERACTABLE_DISTANCE);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(end, _interactRadius);
    }
#endif
}
