// Author: William Whitehouse (WSWhitehouse)
// Single Use & Button Cooldown logic: James Robertson

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;



/// <summary>
/// A button that players can interact with. It includes options for single use and a cooldown.
/// Button events are available in the inspector: <see cref="InteractionButton._onInteractEvent"/>,
/// <see cref="InteractionButton._onHoverStartedEvent"/> and <see cref="InteractionButton._onHoverEndedEvent"/>.
/// Uses the <see cref="IInteractable"/> interface system to manage interaction events.
/// </summary>
public class InteractionButton : MonoBehaviour, IInteractable
{
    [Header("Object References")]
    [Tooltip("Any Mesh Renderer components included in this array will have the hover effect applied.")]
    [SerializeField] private MeshRenderer[] _onHoverMeshRenderers = Array.Empty<MeshRenderer>();
    
    [Header("Button Settings")]
    [Tooltip("Enables/Disables the button interaction.")]
    [SerializeField] private bool _canInteract = true;
    [Tooltip("The interaction distance of this button.")]
    [SerializeField] private float _interactDistance = IInteractable.DEFAULT_INTERACTABLE_DISTANCE;
    [Tooltip("Can the button only be interacted with once?")]
    [SerializeField] private bool _isSingleUse = false;
    [Tooltip("The cooldown duration before a player can interact with the button again")]
    [SerializeField] [TimeField] private float _buttonCooldown = 0.0f;
   
    [Tooltip("Rumble settings")]
    [SerializeField]
    private bool _rumbleEnabled = true;
    [SerializeField] [ToggleField("_rumbleEnabled", true)][Range(0.0f,1.0f)]
    [Tooltip("The strength of the rumble on left/right side of controller")]
    private float _rumbleLeft = 0.1f ,_rumbleRight = 0.1f;
    [SerializeField] [ToggleField("_rumbleEnabled", true)][Range(0.0f,1.0f)]
    [Tooltip("The duration of the rumble")]
    private float _rumbleDuration = 0.5f;
    
    
    [Header("Audio Clips")]
    [SerializeField] private Audio _buttonPressedAudioClip;
    
    [Header("Button Events")]
    public UnityEvent _onInteractEvent     = new UnityEvent();
    public UnityEvent _onHoverStartedEvent = new UnityEvent();
    public UnityEvent _onHoverEndedEvent   = new UnityEvent();
    public UnityEvent _onCooldownEndedEvent   = new UnityEvent();


    
    // NOTE(WSWhitehouse): This is set to true after a player interacts with a button, this is used
    // to stop further interactions if the button is marked as single use in the Button Settings.
    private bool _buttonUsed = false;
    
    private delegate IEnumerator ButtonCooldownDel();


    private ButtonCooldownDel _buttonCooldownFunc;
    private bool _cooldownActive = false;

    private void Awake()
    {
        // NOTE(WSWhitehouse): Allocating the delegate here to reduce memory allocations during runtime.
        _buttonCooldownFunc = ButtonCooldown;
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _onHoverMeshRenderers.AsSpan();
    }

    public bool CanInteract()
    {
        return _canInteract;
    }
    
    public float GetInteractionDistance()
    {
        return _interactDistance;
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        // NOTE(WSWhitehouse): Check if this button is a single use button and a player has already
        // interacted with it. If so, then ignore the input...
        if (_isSingleUse && _buttonUsed) return;
        _buttonUsed = true;
        
        // NOTE(WSWhitehouse): Check that this button has a cooldown timer, checking against epsilon 
        // as this is more stable for floating point comparisons.
        if (_buttonCooldown >= float.Epsilon)
        {
            
            
            if (_cooldownActive) return;
            _cooldownActive = true;
            StartCoroutine(_buttonCooldownFunc());
        }

        if (_rumbleEnabled)
        { 
            InputDevices.Devices[playerInteraction.PlayerProfile.GetPlayerID()].RumblePulse(_rumbleLeft, _rumbleRight, _rumbleDuration, this, false);

        }
       
       
     


        
        AudioManager.PlayOneShotWorldSpace(_buttonPressedAudioClip, transform.position);
        
        _onInteractEvent?.Invoke();
    }



    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        _onHoverStartedEvent?.Invoke();
    }
    
    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        if (_rumbleEnabled)
        {
            InputDevices.Devices[playerInteraction.PlayerProfile.GetPlayerID()].RumbleReset();
        }
        _onHoverEndedEvent?.Invoke();
    }

    

   
    
    private IEnumerator ButtonCooldown()
    {
        // NOTE(WSWhitehouse): Using a manual timer here rather than using `WaitForSeconds` as this is 
        // slightly better for performance and has finer grain control compared to that method. Checking
        // against epsilon here is slightly unnecessary, but if the timer is close enough to 0 the cooldown
        // is ended.
        float timer = _buttonCooldown;
        while (timer >= float.Epsilon)
        {
            timer -= Time.deltaTime;
            yield return null; // Wait for Update
        }
        
        _cooldownActive = false;
        _onCooldownEndedEvent?.Invoke();
    }
}
