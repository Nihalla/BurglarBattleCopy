using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.Mathematics;
using System;
using System.Runtime.CompilerServices;
using PlayerControllers;

public class InteractableLock : MonoBehaviour, IInteractable
{
    public bool onChest = false;
    private int _playerInputID = -1;
    private int _interactingPlayerID = -1;
    [HideInInspector] public bool requiresPlayerRef = true;
    private Transform _playerInteractionPosition;

    [Header("Interaction Components")]
    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();

    [Header("Lock Components")]
    [SerializeField] private GameObject _playerPicklocking;
    [Space]
    [SerializeField] private GameObject _innerLock, _outerLock;
    [Space]
    [SerializeField] private int _maxUnlockRange = 0;
    [SerializeField] private int _maxUnlockSize = 0;
    [Space]
    [SerializeField] private ParticleSystem _innerUnlockParticleEffect = null;
    [SerializeField] private ParticleSystem _outerUnlockParticleEffect = null;

    [SerializeField] private Transform _lockPos;

    [Header("Chest Re-Locking Settings")]
    [SerializeField] private bool _waitToReLock = false;
    [SerializeField] private float _waitToReLockDuration = 5f;

    private PlayerInput _playerInput;

    private int _unlockInnerFrom = 0, _unlockInnerTo = 0;
    private int _unlockOuterFrom = 0, _unlockOuterTo = 0;
    private float _innerLockAngle = 0, _outerLockAngle = 0f;
    private bool _innerUnlocked, _outerUnlocked, _lockCompleted;
    private bool _interactingWithLock;

    private quaternion _innerStartRot;
    private quaternion _outerStartRot;
    
    private Action<InputAction.CallbackContext> _onInnerLockInteractionPerformedFunc = null;
    private Action<InputAction.CallbackContext> _onOuterLockInteractionPerformedFunc = null;

    // relocking based delegates
    private delegate IEnumerator WaitLockDel(ChestController chest, float waitDuration);
    private WaitLockDel WaitAndLockChestFunc;
    private Coroutine _waitAndLockCo;

    private void Awake()
    {
        _interactingWithLock = false;
        _playerInteractionPosition = null;

        _unlockInnerFrom = UnityEngine.Random.Range(-_maxUnlockRange, _maxUnlockRange);
        _unlockOuterFrom = UnityEngine.Random.Range(-_maxUnlockRange, _maxUnlockRange);

        _unlockInnerTo = _unlockInnerFrom + _maxUnlockSize;
        _unlockOuterTo = _unlockOuterFrom + _maxUnlockSize;

        _lockCompleted = false;
        
        // NOTE(WSWhitehouse): Preallocating delegate functions here so we don't do it at runtime...
        _onInnerLockInteractionPerformedFunc = OnInnerLockInteractPerformed;
        _onOuterLockInteractionPerformedFunc = OnOuterLockInteractPerformed;
        WaitAndLockChestFunc = WaitAndLockChest;
    }

    private void Start()
    {
        SetUpDevice();

        _innerStartRot = _innerLock.transform.localRotation;
        _outerStartRot = _outerLock.transform.localRotation;
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }
    
    public bool CanInteract()
    {
        return !_lockCompleted;
    }

    public void OnInteract(PlayerInteraction invokingPlayerInteraction)
    {
        if (_lockCompleted) return;

        // The player's ID who started the interaction
        _playerInputID = invokingPlayerInteraction.PlayerProfile.GetPlayerID();
        _playerInput = invokingPlayerInteraction.PlayerProfile.GetComponent<PlayerInput>();

        if (!_interactingWithLock)
        {
            _interactingPlayerID = _playerInputID;
            _interactingWithLock = true;

            GlobalEvents.OnPlayerPuzzleInteract(_interactingPlayerID, _lockPos);

            ////Debug.Log("Interacting With Lock");
            SwitchToLockControls(_interactingPlayerID);
            
            return;
        }
        
        if (_interactingPlayerID == _playerInputID)
        {
            _interactingWithLock = false;
            
            // Using the below event, the player will regain control once the camera lerp is completed through a callback action.
            GlobalEvents.OnPlayerPuzzleExit(_interactingPlayerID, SwitchToPlayerControls);
        }
    }

    private void OnInnerLockInteractPerformed(InputAction.CallbackContext context)
    {
        if (PauseMenu._MainGameIsPaused)
        {
            return;
        }

        Vector2 stickDirection = context.ReadValue<Vector2>().normalized;

        RotateInnerLock(stickDirection);
    }

    private void OnOuterLockInteractPerformed(InputAction.CallbackContext context)
    {
        if(PauseMenu._MainGameIsPaused)
        {
            return;
        }

        Vector2 stickDirection = context.ReadValue<Vector2>().normalized;

        RotateOuterLock(stickDirection);
    }

    /// <summary>
    /// Calculates the rotation of the lock by converting the angle (radians) into degrees (x rotation).
    /// </summary>
    private void RotateInnerLock(Vector2 direction)
    {
        _innerLockAngle = Mathf.Atan2(direction.y, direction.x) * -1 * Mathf.Rad2Deg;
        _innerLock.transform.localRotation = Quaternion.Euler(new Vector3(_innerLockAngle, -90, -90));
    }

    private void RotateOuterLock(Vector2 direction)
    {
        _outerLockAngle = Mathf.Atan2(direction.y, direction.x) * -1 * Mathf.Rad2Deg;
        _outerLock.transform.localRotation = Quaternion.Euler(new Vector3(_outerLockAngle, -90, -90));
    }

    /// <summary>
    /// A few simple if statements to check whether the locks are in the right place to be unlocked.
    /// </summary>
    private void UnlockCheck()
    {
        if ((_innerLockAngle >= _unlockInnerFrom && _innerLockAngle <= _unlockInnerTo))
        {
            _innerUnlocked = true;
            _innerUnlockParticleEffect.Play();
        }
        else
        {
            _innerUnlocked = false;
            _innerUnlockParticleEffect.Stop();
        }

        if ((_outerLockAngle >= _unlockOuterFrom && _outerLockAngle <= _unlockOuterTo))
        {
            _outerUnlocked = true;
            _outerUnlockParticleEffect.Play();
        }
        else
        {
            _outerUnlocked = false;
            _outerUnlockParticleEffect.Stop();
        }

        if (_innerUnlocked && _outerUnlocked)
        {
            _interactingWithLock = false;
            _lockCompleted = true;
        
            // Using the below event, the player will regain control once the camera lerp is completed.
            GlobalEvents.OnPlayerPuzzleExit(_interactingPlayerID, SwitchToPlayerControls);
        
            if (onChest)
            {
                ChestController chest = GetComponentInParent<ChestController>();
                chest.UnlockChest();
        
                if (_waitToReLock)
                {
                    _waitAndLockCo = StartCoroutine(WaitAndLockChestFunc(chest, _waitToReLockDuration));
                }
            }
            else
            {
                GetComponentInParent<DoorSliding>().OpenDoor();
            }
        }
    }

    public void ForceUnlock()
    {
        _lockCompleted = true;
        _innerUnlocked = true;
        _outerUnlocked = true;
        _interactingWithLock = true;

        if(onChest)
        {
            GetComponentInParent<ChestController>().UnlockChest();
        }
        else
        {
            GetComponentInParent<DoorSliding>().OpenDoor();
        }
    }

    public bool CheckUnlockStatus()
    {
        return _lockCompleted;
    }

    private void SetLockpickPlayerID(int ID)
    {
        _playerInputID = ID;
        // //Debug.Log(ID);
    }

    private void SetUpDevice()
    {
        for (int i = 0; i < InputDevices.CurrentDeviceCount; i++)
        {
            int id = i - 1; // NOTE(WSWhitehouse): Capturing this variable in callback
            InputDevices.Devices[i].Actions.PlayerInteraction.Interact.performed += ctx => SetLockpickPlayerID(id);
        }
    }

    private void Update()
    {
        if (_interactingWithLock)
        {
            UnlockCheck();
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float GetNormalisedLockValue(float lockAngle, float unlockFrom)
            {
                float relativeLockAngle = math.abs(lockAngle - unlockFrom);
                return 1.0f - (relativeLockAngle / 180);
            }
            
            ref DeviceData data = ref InputDevices.Devices[_interactingPlayerID];
            float innerLockNormalised = GetNormalisedLockValue(_innerLockAngle, _unlockInnerFrom);
            float outerLockNormalised = GetNormalisedLockValue(_outerLockAngle, _unlockOuterFrom);
            data.Rumble(outerLockNormalised, innerLockNormalised);
        }

        if (_innerUnlocked && _interactingWithLock)
        {
            _innerLockAngle = _unlockInnerTo;
            _playerInput.actions.FindAction("MoveInnerLock").Disable();
        }
        
        if (_outerUnlocked && _interactingWithLock)
        {
            _playerInput.actions.FindAction("MoveOuterLock").Disable();
        }
    }

    public bool GetPlayerRefRequirement()
    {
        return requiresPlayerRef;
    }

    private void SwitchToLockControls(int currentPlayerID)
    {
        InputDevices.Devices[currentPlayerID].Actions.PlayerController.Disable();
        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.Enable();

        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.MoveInnerLock.performed += _onInnerLockInteractionPerformedFunc;
        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.MoveOuterLock.performed += _onOuterLockInteractionPerformedFunc;
    }

    private void SwitchToPlayerControls(int currentPlayerID)
    {
        InputDevices.Devices[currentPlayerID].Actions.PlayerController.Enable();
        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.Disable();

        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.MoveInnerLock.performed -= _onInnerLockInteractionPerformedFunc;
        InputDevices.Devices[currentPlayerID].Actions.PlayerLockpick.MoveOuterLock.performed -= _onOuterLockInteractionPerformedFunc;
        
        ref DeviceData data = ref InputDevices.Devices[_interactingPlayerID];
        data.RumbleReset();
        
        _innerUnlockParticleEffect.Stop();
        _outerUnlockParticleEffect.Stop();

        _interactingPlayerID = -1;
    }

    private IEnumerator WaitAndLockChest(ChestController chest, float waitDuration)
    {
        float timer = float.Epsilon;
        while (timer < waitDuration)
        {
            timer += Time.deltaTime;
            yield return null; // wait for update
        }

        // we reset the state of the lock, so that it can be interacted with again on a chest
        chest.CloseAndLockChest();
        _lockCompleted = false;
        _innerUnlocked = false;
        _outerUnlocked = false;
        _interactingWithLock = false;
        _innerLock.transform.localRotation = _innerStartRot;
        _outerLock.transform.localRotation = _outerStartRot;

        _innerLockAngle = 0;
        _outerLockAngle = 0;

        _waitAndLockCo = null;
        yield break;
    }
}
