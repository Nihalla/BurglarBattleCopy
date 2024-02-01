// Author: Zack Collins

using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

/// <summary>
/// This script is a wrapper that will increment an internal counter, and then will signal that the internal counter
/// has reached the <see cref="maxIncrementCount"/>.
/// It has two types of event that can be subscribed to either via Unity GUI <see cref="onCountReachedUnityEvent"/> or via 
/// C# code <see cref="onCountReachedDelegateEvent"/> via reference to this component
/// Can be set to only be actived once, through the use of <see cref="_disableAfterReachedCount"/>
/// </summary>
public class IncrementEvent : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private bool _disableAfterReachedCount = true;

    [field: Header("Increment Settings")]
    [field: SerializeField] public int maxIncrementCount { get; private set; } = 1;
    [Space]

    // NOTE(Zack): subscribe to this via the Unity GUI
    public UnityEvent onCountReachedUnityEvent = new UnityEvent();

    // NOTE(Zack): subscibe to this via C# code by getting a reference to this component in code
    public delegate void EventDel();
    public EventDel onCountReachedDelegateEvent;

    private bool _reached = false;
    private int _currentIncrementCount = 0;

    /// <summary>
    /// Increments the internal counter. Once the internal counter has reached the value of <see cref="maxIncrementCount"/> 
    /// it will envoke both the Unity Event and C# Delegate Event <see cref="onCountReachedUnityEvent"/> <see cref="onCountReachedDelegateEvent"/>
    /// The internal counter can go above the value of <see cref="maxIncrementCount"/>
    /// </summary>
    public void IncrementCount()
    {
        if (_reached && _disableAfterReachedCount) return;

        _currentIncrementCount += 1;
        _reached = _currentIncrementCount >= maxIncrementCount; // REVIEW(Zack): should we clamp the increment count to the max?

        if (!_reached) return;

        onCountReachedUnityEvent?.Invoke();
        onCountReachedDelegateEvent?.Invoke();
    }

    /// <summary>
    /// Decrements the internal counter, and checks if we have gone below the threshold of <see cref="maxIncrementCount"/>.
    /// Internal counter is clamped to a positive range of numbers, will never go below 0
    /// </summary>
    public void DecrementCount()
    {
        if (_reached && _disableAfterReachedCount) return;
        _currentIncrementCount -= 1;
        _currentIncrementCount = math.max(0, _currentIncrementCount); // REVIEW(Zack): we ensure that values never go negative
        _reached = _currentIncrementCount >= maxIncrementCount;
    }
}
