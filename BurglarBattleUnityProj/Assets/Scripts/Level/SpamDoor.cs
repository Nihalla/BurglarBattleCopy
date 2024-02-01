// Author: William Whitehouse (WSWhitehouse)

using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A door that players must "spam interact" with in order for it to remain open!
/// Each interaction moves the door closer to its open position by a percentage
/// amount (see <see cref="SpamDoor._openMoveDistance"/>). Optionally can be locked
/// in its open position once fully opened.
/// </summary>
public class SpamDoor : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform _doorTransform;
    [SerializeField] private Transform _openTransform;
    [SerializeField] private Transform _closedTransform;
    
    [Header("Door Settings")]
    [Tooltip("Should the door remain locked open once the open position has been reached?")]
    [SerializeField] private bool _lockDoorOnceOpened = false;
    
    [Tooltip("Duration before the door starts to close after invoking `TryOpen()`")]
    [SerializeField] private float _durationBeforeClose = 0.2f;
    
    [Tooltip("Duration it takes for the door to close from the complete open position.")]
    [SerializeField] private float _closeMovementDuration = 1.0f;
    
    [Tooltip("Duration the door takes to open in one \"movement step\" when invoking `TryOpen()`")]
    [SerializeField] private float _openMovementDuration = 0.2f;
    
    [Tooltip("Distance percentage the door is opened in a single \"movement step\" when invoking `TryOpen()`")]
    [SerializeField] [Range(0.0f, 1.0f)] private float _openMoveDistance = 0.1f;
    
    private Coroutine _openCoroutine  = null;
    private Coroutine _closeCoroutine = null;
    private Coroutine _timerCoroutine = null;
    
    private bool _canClose       = true;
    private bool _doorLockedOpen = false;
    
    private delegate IEnumerator EmptyCoroutineDel();
    private delegate IEnumerator MoveDoorDel(float3 start, float3 end, float duration);
    private EmptyCoroutineDel _closeDoorFunc;
    private EmptyCoroutineDel _waitToCloseFunc;
    private EmptyCoroutineDel _tryOpenFunc;
    private MoveDoorDel _moveDoorFunc;

    private void Awake()
    {
        _closeDoorFunc   = CloseDoor;
        _waitToCloseFunc = WaitToClose;
        _tryOpenFunc     = TryOpenCoroutine;
        _moveDoorFunc    = MoveDoor;
    }

    private void Update()
    {
        if (_openCoroutine  != null) return;
        if (_closeCoroutine != null) return;
        if (_doorLockedOpen)         return;
        if (!_canClose)              return;

        float3 close   = _closedTransform.localPosition;
        float3 current = _doorTransform.localPosition;
        float distance = math.distance(current, close);
        
        if (distance >= float.Epsilon)
        {
            _closeCoroutine = StartCoroutine(_closeDoorFunc());
        }
    }
    
    /// <summary>
    /// Invoking this function will try to open the Spam Door. This is most likely
    /// to be called by a player interaction from the scene. 
    /// </summary>
    public void TryOpen()
    {
        if (_openCoroutine != null) return;
        
        _openCoroutine = StartCoroutine(_tryOpenFunc());
        _canClose = false;
    }
    
    private IEnumerator TryOpenCoroutine()
    {
        if (_doorLockedOpen)
        {
            _openCoroutine = null;
            yield break;
        }
        
        float3 open    = _openTransform.localPosition;
        float3 close   = _closedTransform.localPosition;
        float3 current = _doorTransform.localPosition;
        
        float3 direction  = math.normalizesafe(open - close);
        float3 moveAmount = direction * _openMoveDistance;
        float3 end        = current + moveAmount;
        
        float currentDist = math.distance(open, current);
        float newDist     = math.distance(open, end);
        
        // NOTE(WSWhitehouse): If the new distance is greater than the current distance we know
        // the door is now moving away from the fully open position! So clamp it to the open
        // position as it can't move any further.
        if (newDist > currentDist || currentDist < float.Epsilon)
        {
            end = open;
            
            if (_lockDoorOnceOpened)
            {
                _doorLockedOpen = true;
            }
        }

        if (_closeCoroutine != null)
        {
            StopCoroutine(_closeCoroutine);
            _closeCoroutine = null;
        }
        
        yield return _moveDoorFunc(current, end, _openMovementDuration);
        
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
        
        _timerCoroutine = StartCoroutine(_waitToCloseFunc());
        _openCoroutine  = null;
    }
    
    private IEnumerator WaitToClose()
    {
        float timer = float.Epsilon;
        while (timer < _durationBeforeClose)
        {
            timer += Time.deltaTime;
            yield return null; // Wait for update
        }
        
        _canClose = true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float NormalizeValue(float val, float maxRange, float minRange = 0, float normalizeRange = 1) 
    {
        return ((val - minRange) / (maxRange - minRange)) * normalizeRange;
    }
    
    private IEnumerator CloseDoor()
    {
        float3 open    = _openTransform.localPosition;
        float3 close   = _closedTransform.localPosition;
        float3 current = _doorTransform.localPosition;
        
        float currentDist = math.distance(current, close);
        float fullDist    = math.distance(open, close);
        float duration    = _closeMovementDuration * NormalizeValue(currentDist, fullDist);
        
        if (!_canClose) yield break;
        
        yield return _moveDoorFunc(current, close, duration);
        _closeCoroutine = null;

    }

    private IEnumerator MoveDoor(float3 start, float3 end, float duration)
    {
        _doorTransform.localPosition = start;
        
        float timer = float.Epsilon;
        while (timer < duration)
        {
            _doorTransform.localPosition = math.lerp(start, end, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        _doorTransform.localPosition = end;
    }
}
