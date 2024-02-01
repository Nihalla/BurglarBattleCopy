// Author: Zack Collins

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// This class is a simple sliding door, that will linearly interpolate between <see cref="_openTransform"/> and <see cref="_closedTransform"/>.
/// To use this script either get a reference to this script in code, or it can be dragged into a UnityEvent GUI and the relevant function can be called from the UnityEvent.
/// </summary>
public class DoorSliding : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private Transform _doorTransform;
    [SerializeField] private Transform _openTransform;
    [SerializeField] private Transform _closedTransform;

    [Header("Open/Closed Settings")]
    [SerializeField] private bool _startOpen = false;

    [Header("Animation Settings")]
    [SerializeField] private float _openDuration  = 1f;
    [SerializeField] private float _closeDuration = 1f;

    private Coroutine _movementCoroutine;
    private bool _open = false;
    
    public bool IsOpen => _open;

    private void Awake() 
    {
        Debug.Assert(_doorTransform   != null, "Door Transform is null, Please set in the inspector",  this);
        Debug.Assert(_openTransform   != null, "Open Transform is null, Please set in the inspector",  this);
        Debug.Assert(_closedTransform != null, "Close Transform is null, Please set in the inspector", this);
    }

    private void Start()
    {
        _open = _startOpen;

        if (_startOpen)
        {
            _doorTransform.localPosition = _openTransform.localPosition;
        } 
        else
        {
            _doorTransform.localPosition = _closedTransform.localPosition;
        }
    }

    /// <summary>
    /// When called will interpolate the Door to its open position set by <see cref="_openTransform"/>
    /// </summary>
    public void OpenDoor()
    {
        if (_open) return;
        _open = true;

        float3 open    = _openTransform.localPosition;
        float3 closed  = _closedTransform.localPosition;
        float3 current = _doorTransform.localPosition;

        // TODO(Zack): remove the use of sqrt
        float currentDist = math.distance(current, open);
        float fullDist    = math.distance(closed, open);
        float duration    = _openDuration * NormalizeValue(currentDist, fullDist);

        if (_movementCoroutine != null) 
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(Lerp.ToPositionLocalFunc(_doorTransform, current, open, duration));
    }

    /// <summary>
    /// When called will interpolate the Door to its closed position set by <see cref="_closedTransform"/>
    /// </summary>
    public void CloseDoor()
    {
        if (!_open) return;
        _open = false;

        float3 open    = _openTransform.localPosition;
        float3 closed  = _closedTransform.localPosition;
        float3 current = _doorTransform.localPosition;

        // TODO(Zack): remove the use of sqrt
        float currentDist = math.distance(current, closed);
        float fullDist    = math.distance(open, closed); 
        float duration    = _closeDuration * NormalizeValue(currentDist, fullDist);

        if (_movementCoroutine != null) 
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(Lerp.ToPositionLocalFunc(_doorTransform, current, closed, duration));
    }

    /// <summary>
    /// When called will interpolate the Door between it's open and closed positions set by both <see cref="_openTransform"/> and <see cref="_closedTransform"/>
    /// </summary>
    public void ToggleDoor()
    {
        if (_open)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NormalizeValue(float val, float maxrange, float minrange = 0, float normalizerange = 1) 
    {
        return ((val - minrange) / (maxrange - minrange)) * normalizerange;
    }
}
