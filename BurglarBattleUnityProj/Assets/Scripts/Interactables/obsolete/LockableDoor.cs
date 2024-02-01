using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This is obsolete, please use <see cref="Lockable"/> and <see cref="DoorRotating"/>/<see cref="DoorSliding"/>.
/// This script is staying for now as some of its logic could be useful in the near future.  
/// </summary>
public class LockableDoor : MonoBehaviour, IInteractable
{
    [Header("Door Parameters")] 
    [SerializeField] private float _openDuration = 0.5f;
    [SerializeField] private float _openAngle = -90f;
    
    [Header("Door Locking Paramaters")] 
    [SerializeField] private bool _isLocked = false;
    [Tooltip("How long it takes to hold until door locks/unlocks")] 
    [SerializeField] private float _lockingDuration = 1.5f;
    
    [Header("Testing Door Material Parameters")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [Space]
    [SerializeField] private Material _openMat;
    [SerializeField] private Material _lockedMat;

    //Coroutines
    private Coroutine _openDoorRoutine = null;
    private Coroutine _doorLockingRoutine = null;
    private Coroutine _holdTimerRoutine = null;

    //Quaternions for door's open and closed positions
    private Quaternion _doorClosedRotation;
    private Quaternion _doorOpenRotation;


    private bool _isOpen = false;
    private float _rotationTimer;
    private float _lockingTimer;
    private bool _holdingInteract;


    //Everything related to checking if player is wanting to lock or open door.
    private float _interactTimer = 0f;
    private float _interactDuration = 0.5f;

    private MeshRenderer[] _meshRenderers = new MeshRenderer[1];

    private void Awake()
    {
        _meshRenderers[0] = _meshRenderer;

        switchMaterial(_isLocked);
        _doorClosedRotation = transform.rotation;
        _doorOpenRotation = Quaternion.Euler(transform.rotation.eulerAngles + Vector3.up * _openAngle);
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteractHoldStarted(PlayerInteraction playerInteraction)
    {
        _holdingInteract = true;
        _doorLockingRoutine = StartCoroutine(HoldTimer());
    }

    public void OnInteractHoldEnded(PlayerInteraction playerInteraction)
    {
        _holdingInteract = false;
    }

    private IEnumerator HoldTimer()
    {
        //This checks if player is holding interact button for a certain amount of time in order to decide whether they want to lock/unlock it or close/open it.
        while (_holdingInteract && _interactTimer < _interactDuration)
        {
            _interactTimer += Time.deltaTime;
            yield return null;
        }

        //If player holding for long enough, then will move onto the Door Locking process.
        if (_interactTimer >= _interactDuration && !_isOpen)
        {
            _doorLockingRoutine = StartCoroutine(DoorLocking());
        }
        //Otherwise, just open/close the door.
        else if (_interactTimer < _interactDuration && !_isOpen && !_isLocked && _openDoorRoutine == null)
        {
            _openDoorRoutine = StartCoroutine(OpenDoor());
        }
        else if (_interactTimer < _interactDuration && _isOpen && !_isLocked && _openDoorRoutine == null)
        {
            _openDoorRoutine = StartCoroutine(CloseDoor());
        }

        _interactTimer = 0f;
        _holdTimerRoutine = null;
    }

    private IEnumerator DoorLocking()
    {
        while (_holdingInteract && _lockingTimer < _lockingDuration)
        {
            _lockingTimer += Time.deltaTime;
            yield return null;
        }

        if (_lockingTimer >= _lockingDuration)
        {
            _isLocked = !_isLocked;
            switchMaterial(_isLocked);
        }

        _lockingTimer = 0f;
        _doorLockingRoutine = null;
    }

    private IEnumerator OpenDoor()
    {
        _rotationTimer = float.Epsilon;
        _isOpen = true;

        while (_rotationTimer <= _openDuration)
        {
            _rotationTimer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(_doorClosedRotation, _doorOpenRotation, _rotationTimer / _openDuration);
            yield return null;
        }

        _rotationTimer = 0f;
        _openDoorRoutine = null;
    }

    private IEnumerator CloseDoor()
    {
        _rotationTimer = float.Epsilon;
        _isOpen = false;

        while (_rotationTimer <= _openDuration)
        {
            _rotationTimer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(_doorOpenRotation, _doorClosedRotation, _rotationTimer / _openDuration);
            yield return null;
        }

        _rotationTimer = 0f;
        _openDoorRoutine = null;
    }


    //NOTE(Sebadam2010): This is for testing to understand when the door is locked
    private void switchMaterial(bool isLocked)
    {
        if (isLocked)
        {
            _meshRenderer.material = _lockedMat;
        }
        else
        {
            _meshRenderer.material = _openMat;
        }
    }
}