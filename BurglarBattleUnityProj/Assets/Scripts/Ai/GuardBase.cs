using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum GuardType
{
    GUARD     = 0,
    AUTOMATON = 1,
    WATCHER = 2,
    GOBLIN,
}

public enum GuardLevel
{
    LEVEL_1 = 0,
    LEVEL_2 = 1
}

public abstract class GuardBase : MonoBehaviour
{
    public GuardBase(GuardType type) {}
    public AiComponentType Type { get; protected set; }
    
    [Header("Base Components")]
    public NavMeshMovementComponent _nav;
    public NavMeshAgent _navAgent;
    public GuardLevel _guardNativeLevel = GuardLevel.LEVEL_1;
    public Animator _animator;

    public enum GuardState
    {
        PATROLING = 0,
        ALERT     = 1,
        CHASING   = 2,
        SEARCHING = 3,
        STUNNED   = 4,
    }

    [NonSerialized] public float _currentStunLength = 0;
    protected GuardState _state = GuardState.PATROLING;

    protected delegate IEnumerator GuardStunDel(float seconds);
    protected GuardStunDel GuardStunFunc;
    protected Coroutine _guardStunCo;

    protected void Awake()
    {
        Debug.Assert(_nav != null, $"NavMesh Component is not set on {gameObject.name}");
        Debug.Assert(_navAgent != null, $"Nav Mesh Agent is not set on {gameObject.name}");
        GuardStunFunc = GuardStun;
    }
    

    /// <summary>
    /// Stun the Guard for the given number of seconds.
    /// </summary>
    /// <param name="seconds">The time to be stunned for.</param>
    public virtual void StunGuard(float seconds)
    {
        _animator.SetTrigger("Stunned");
        if (_guardStunCo != null)
        {
            if (_currentStunLength < seconds)
            {
                StopCoroutine(_guardStunCo);
            }
        }
        
        _currentStunLength = seconds;
        _guardStunCo = StartCoroutine(GuardStunFunc(seconds));
    }

    protected abstract IEnumerator GuardStun(float seconds);

    /// <summary>
    /// Turns on the threat indicator on the target player that tracks the guard calling this function.
    /// </summary>
    /// <param name="target"></param>
    protected void SignalAggroOn(GameObject target)
    {
        target.GetComponentInChildren<AggroHUD>().SetAggro(true, gameObject.transform);
    }

    /// <summary>
    /// Turns off the threat indicator on the target player that tracks the guard calling this function.
    /// </summary>
    /// <param name="target"></param>
    protected void SignalAggroOff(GameObject target)
    {
        target.GetComponentInChildren<AggroHUD>().SetAggro(false, gameObject.transform);
    }
}
