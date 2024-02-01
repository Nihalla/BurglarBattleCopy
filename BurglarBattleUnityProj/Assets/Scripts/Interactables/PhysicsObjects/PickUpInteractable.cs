// Author: Vlad Trakiyski
// Edit: Zack Collins (bug fixing with toggle to pickup and drop, move to using coroutines, code cleanup)

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

/// <summary>
/// Place this on an object that has a Rigidbody to allow it to be picked up by the player.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(StunPlayerOnHit), typeof(SphereCollider))]
public class PickUpInteractable : MonoBehaviour, IInteractable
{
    [Header("Object References")]
    [Tooltip("Any Mesh Renderer components included in this array will have the hover effect applied.")]
    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();

    [Header("Physics Parameters")]
    [SerializeField] private float _pickupForce = 150.0f;
    [SerializeField] private float _throwForce = 2000f;

    [Header("Audio")]
    [SerializeField] private Audio _throwSwoosh;

    public delegate void EmptyEventDel();
    public EmptyEventDel onItemInteraction;

    private Action<InputAction.CallbackContext> ThrowObjectFunc;
    private delegate IEnumerator MovementDel();
    private MovementDel MovePickupFunc;
    private Coroutine _moveCoroutine;

    private readonly WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    private StunPlayerOnHit _stunner;
    private SphereCollider _trigger;
    private Rigidbody _heldObjRB;
    private PlayerInteraction _playerInteraction = null;
    private Transform _HoldArea => _playerInteraction.HoldPointTransform;

    private bool _disablePickup = false;

    private IdolType _idol = null;
    public IdolType Idol => _idol;
    public bool IsIdol   { get; private set; } = false;

    public bool IsHeld { get; private set; } = false;

    private void Awake()
    {
        _heldObjRB = GetComponent<Rigidbody>();
        _stunner   = GetComponent<StunPlayerOnHit>();
        _trigger   = GetComponent<SphereCollider>(); 

        // FIX(Zack): debug assert going off on start of scene
        Debug.Assert(_heldObjRB != null, "Could not get Rigidbody. Please add one in the inspector",       this);
        Debug.Assert(_stunner   != null, "Could not get StunPlayerOnHit. Please add one in the inspector", this);
        Debug.Assert(_trigger   != null, "Could not get SphereCollider. Please add one in the inspector",  this);

        // we're enforcing the sphere collider to instead be a trigger collider
        _trigger.isTrigger = true;
        _trigger.enabled   = true;

        // NOTE(Zack): pre-allocation of function delegate, so that we remove as many allocations at runtime as possible
        MovePickupFunc  = MovePickup;
        ThrowObjectFunc = ThrowObject;

        IsIdol = TryGetComponent<IdolType>(out _idol);
    }

    private void OnDestroy()
    {
        if (_playerInteraction == null) return;
        UnsubscribeFromPlayerInput(_playerInteraction);
    }

    /// <summary>
    /// Forces the object to be dropped if it is currently being held by a Player
    /// </summary>
    public void ForceDrop()
    {
        if (!IsHeld) return;
        _heldObjRB.velocity = float3.zero;
        UnsubscribeFromPlayerInput(_playerInteraction);
        Drop(_playerInteraction);
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (!_disablePickup)
        {
            onItemInteraction?.Invoke();

            if (!IsHeld)
            {
                SubscribeToPlayerInput(playerInteraction);
                Pickup(playerInteraction);
            }
            else
            {
                UnsubscribeFromPlayerInput(playerInteraction);
                Drop(playerInteraction);
            }
        }
     
    }

    public void DisablePickup()
    {
        _disablePickup = true;
    }

    private void Pickup(PlayerInteraction playerInteraction)
    {
        // if we still have a reference to a player we ignore this interaction
        if (_playerInteraction != null) return;
        IsHeld = true;

        // we get a reference to the players profile if it is an Idol pickup
        if (IsIdol)
        {
            Idol.profile = playerInteraction.PlayerProfile;
        }

        // NOTE(Zack): we set these variables before we start the coroutine, as we will get a null reference
        // exception from the coroutine otherwise
        _playerInteraction = playerInteraction; // cache the reference to the currently interacting player
        transform.SetParent(playerInteraction.HoldPointTransform);


        // REVIEW(Zack): the coroutine should never actually be valid when entering into this function,
        // this is more so that we can ensure that only a single coroutine has been started on this object
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        // start the coroutine that forces the interactable to follow the center of a players screen
        _moveCoroutine = StartCoroutine(MovePickupFunc());

        _heldObjRB.useGravity = false;
        _heldObjRB.drag = 10;
        _heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;

        // Just to block collisions from allowing the player to prop surf
        Physics.IgnoreCollision(_playerInteraction.GetComponent<Collider>(), GetComponent<Collider>());
    }

    private IEnumerator MovePickup()
    {
        while (true)
        {
            // NOTE(Zack): we're using distance squared as we don't need the accuracy that comes from the [sqrt()]
            // that is part of the normal distance function
            float distanceSquared = math.distancesq(transform.position, _HoldArea.position);
            if (distanceSquared > 0.01f)
            {
                float3 moveDirection = (_HoldArea.position - transform.position);
                _heldObjRB.AddForce(moveDirection * _pickupForce);
            }

            // we wait for the fixed update loop as we're operating on a Rigidbody
            yield return _waitForFixedUpdate;
        }
    }

    private void Drop(PlayerInteraction playerInteraction)
    {
        // NOTE(Zack): should stop other players from being able to force other players to drop this item.
        if (_playerInteraction != playerInteraction) return;
        IsHeld = false;

        // REVIEW(Zack): probably unnecessary, but we're just ensuring that we actually have a running coroutine to stop
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        _heldObjRB.useGravity = true;
        _heldObjRB.drag = 1;
        _heldObjRB.constraints = RigidbodyConstraints.None;

        // Re-enable collisions with the player upon release. 
        Physics.IgnoreCollision(_playerInteraction.GetComponent<Collider>(), GetComponent<Collider>(), false);

        // NOTE(Zack): these are set after we have stopped the coroutine so that we do not get a null reference exception
        _playerInteraction = null; // remove the reference to the currently interacting player
        transform.SetParent(null);
    }


    // NOTE(Zack): the duplicated code has been removed and replaced with calling the Drop function,
    // the additional functionality for throwing the object has been kept
    /// <summary>
    /// Launches the object held by the player in their facing direction
    /// </summary>
    /// 
    // Louis Phillips and Ryan Sewell
    private void Launch(PlayerInteraction playerInteraction)
    {
        if (_playerInteraction != playerInteraction) return;
        Drop(playerInteraction);

        _stunner.SetToStun(true);

        AudioManager.PlayScreenSpace(_throwSwoosh);
        _heldObjRB.AddForce((playerInteraction.PlayerProfile.GetPlayer().GetPlayerCamera().transform.forward * _throwForce));
        //_trigger.enabled = true;
    }

    // NOTE(Zack): the FixedUpdate loop has been replaced with using the event based input,
    // this is due to FixedUpdate being called multiple times per frame, upward of 2,
    // for __each__ object that has this script on it. Meaning that we are doing multiple checks per frame
    // for __every__ object in the scene even if the player is on the opposite side of the map.
    private void ThrowObject(InputAction.CallbackContext context)
    {
        if (_playerInteraction == null) return;

        UnsubscribeFromPlayerInput(_playerInteraction);
        Launch(_playerInteraction);
    }

    private void SubscribeToPlayerInput(PlayerInteraction playerInteraction)
    {
        if (_playerInteraction != null) return;

        int id = playerInteraction.PlayerProfile.GetPlayerID();
        InputDevices.Devices[id].Actions.PlayerController.Throw.performed += ThrowObjectFunc;
    }

    private void UnsubscribeFromPlayerInput(PlayerInteraction playerInteraction)
    {
        if (_playerInteraction != playerInteraction) return;

        int id = playerInteraction.PlayerProfile.GetPlayerID();
        InputDevices.Devices[id].Actions.PlayerController.Throw.performed -= ThrowObjectFunc;
    }

}
