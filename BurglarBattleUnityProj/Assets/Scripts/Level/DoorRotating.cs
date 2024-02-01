// Author: Zack Collins

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;

// NOTE(Zack): this script makes the assumption that the visual aspect of the door is offset slightly so that the rotation looks like a door opening.
// See the prefab "/Prefabricated Objects/Environment/Doors/DoorRotating" for and example of how to set this up.

/// <summary>
/// This class is a simple rotating door, that will linearly interpolate between <see cref="_openAngle"/> and it's starting rotation.
/// To use this script either get a reference to this script in code, or it can be dragged into a UnityEvent GUI and the relevant function can be called from the UnityEvent.
/// </summary>
public class DoorRotating : MonoBehaviour
{
    public enum RotateDirection
    {
        POSITIVE,
        NEGATIVE
    }
    
    [Header("Prefab References")]
    [SerializeField] private Transform _doorTransform;

    [Header("Open/Closed Settings")]
    [SerializeField] private bool _startOpen = false;
    [SerializeField] [Min(float.Epsilon)] private float _openAngle = 90f;

    [Header("Animation Settings")]
    [SerializeField] private float _openDuration  = 1f;
    [SerializeField] private float _closeDuration = 1f;

    [Header("Sound Effects")]
    [SerializeField] private Audio _openSoundEffect;
    [SerializeField] private Audio _closeSoundEffect;

    private Coroutine _movementCoroutine;
    private bool _open = false;
        
    private quaternion _openOrientationPos;
    private quaternion _openOrientationNeg;
    private quaternion _closedOrientation;
    private float _fullAngle;

    private void Awake()
    {
        Debug.Assert(_doorTransform != null, "Door Transform is null, Please set in the inspector", this);
    }

    private void Start()
    {
        // NOTE(Zack): Main rotating door logic from -> Seb
        Quaternion rotation = _doorTransform.rotation;
        _closedOrientation  = rotation;
        _openOrientationPos = Quaternion.Euler(rotation.eulerAngles + (Vector3.up * _openAngle));
        _openOrientationNeg = Quaternion.Euler(rotation.eulerAngles + (Vector3.up * (-_openAngle)));
        _open = _startOpen;

        // NOTE(Zack): pre-calculating the maximum value of the angle between the two orientations
        float dot = math.dot(_openOrientationPos, _closedOrientation);
        _fullAngle = math.acos(dot) * 2f;

        if (_startOpen)
        {
            _doorTransform.rotation = _openOrientationPos;
        }
    }

    /// <summary>
    /// Checks whether the door is open or not.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsOpen()
    {
        return _open;
    }


    /// <summary>
    /// When called will interpolate the Door to its open rotation set by <see cref="_openAngle"/>
    /// </summary>
    public void OpenDoor() => OpenDoor(RotateDirection.POSITIVE);

    public void OpenDoorPositive() => OpenDoor(RotateDirection.POSITIVE);
    public void OpenDoorNegative() => OpenDoor(RotateDirection.NEGATIVE);

    /// <summary>
    /// When called will interpolate the Door to its open rotation set by <see cref="_openAngle"/>
    /// </summary>
    public void OpenDoor(RotateDirection rotateDirection)
    {
        if (_open) return;
        _open = true;

        AudioManager.PlayScreenSpace(_openSoundEffect);
        quaternion open    = rotateDirection == RotateDirection.POSITIVE ? _openOrientationPos : _openOrientationNeg;
        quaternion current = _doorTransform.rotation;

        float currentDot = math.dot(current, open);
        float currentAngle = math.acos(currentDot) * 2f;
        float duration = _openDuration * NormalizeValue(currentAngle, _fullAngle);

        if (_movementCoroutine != null) 
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(Lerp.ToRotationFunc(_doorTransform, current, open, duration));
    }

    /// <summary>
    /// When called will interpolate the Door to its closed rotation set by its starting rotation
    /// </summary>
    public void CloseDoor()
    {
        if (!_open) return;
        _open = false;

        AudioManager.PlayScreenSpace(_closeSoundEffect);
        quaternion closed  = _closedOrientation;
        quaternion current = _doorTransform.rotation;

        float currentDot = math.dot(current, closed);
        float currentAngle = math.acos(currentDot) * 2f;
        float duration = _closeDuration * NormalizeValue(currentAngle, _fullAngle);

        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(Lerp.ToRotationFunc(_doorTransform, current, closed, duration));
    }
    
    /// <summary>
    /// When called will interpolate the Door between it's open and closed rotations set by both <see cref="_openAngle"/> and its starting rotation
    /// </summary>
    public void ToggleDoor() => ToggleDoor(RotateDirection.POSITIVE);

    public void ToggleDoorPositive() => ToggleDoor(RotateDirection.POSITIVE);
    public void ToggleDoorNegative() => ToggleDoor(RotateDirection.NEGATIVE);

    /// <summary>
    /// When called will interpolate the Door between it's open and closed rotations set by both <see cref="_openAngle"/> and its starting rotation
    /// </summary>
    /// <param name="rotateDirection">Direction to rotate the door to its open state (only used if toggle calls open)</param>
    public void ToggleDoor(RotateDirection rotateDirection)
    {
        if (_open)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor(rotateDirection);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NormalizeValue(float val, float maxrange, float minrange = 0, float normalizerange = 1) 
    {
        return ((val - minrange) / (maxrange - minrange)) * normalizerange;
    }
}
