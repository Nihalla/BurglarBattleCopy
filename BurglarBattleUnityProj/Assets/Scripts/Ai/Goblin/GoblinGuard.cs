// Author: Zack Collins

using PlayerControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

/// <summary>
/// This script manages the stealing behaviour of the Goblin Thief.
/// This script can be attached directly to a moving agent, or as a manager type style that has a 
/// reference to the various scripts.
/// </summary>
public class GoblinGuard : MonoBehaviour
{
    [Header("Goblin References")]
    [SerializeField] private GoblinAgent _goblinAgent;
    [SerializeField] private CatchComponent _catchComponent;
    [SerializeField] private DetectionComponent _detectionComponent;

    [Header("Environment References")]
    [SerializeField] private Transform _hiddenHole;
    [SerializeField] private GoblinLootStash _lootStash;

    [Header("Prefab References")]
    [SerializeField] private CoinController _lootBagPrefab;

    [Header("Behaviour Settings")]
    [SerializeField] private float _goblinStunnedDuration = 1f;
    [SerializeField] private float _catchRange = 1.5f;
    [SerializeField] private int _lootValueToSteal = 5;
    [SerializeField] private int _floorNumber = 0;
    [SerializeField] private float _maxDistanceFromHiddenPosition = 25f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource3D _laughSFX;

    private Transform _targetPlayer;
    private int _stolenGold;
    private GuardBase.GuardState _state;

    private delegate IEnumerator ChaseCoroutineDel(Loot playerLoot);
    private ChaseCoroutineDel ChasePlayerFunc;
    
    private delegate IEnumerator EmptyCoroutineDel();
    private EmptyCoroutineDel DetectPlayerFunc;
    private EmptyCoroutineDel MoveToDepositGoldFunc;
    private EmptyCoroutineDel MoveToHiddenHoleFunc;
    private Coroutine _currentStateCo;

    private delegate IEnumerator GuardStunDel(float seconds);
    private GuardStunDel GuardStunFunc;
    private Coroutine _stunCo;

    // base class getters for better clarity of what the components, that we're accessing are.
    private NavMeshMovementComponent _NavMeshMovementComp => _goblinAgent._nav;
    private NavMeshAgent _NavMeshAgent                    => _goblinAgent._navAgent;
    private Animator _Animator                            => _goblinAgent._animator;

    // animation string hashing
    private static readonly int _movementHash = Animator.StringToHash("Speed");
    private static readonly int _attackHash   = Animator.StringToHash("Attack");
    private static readonly int _stunnedHash  = Animator.StringToHash("Stunned");

    private void Awake()
    {
        _goblinAgent.transform.position = _hiddenHole.position;
        _goblinAgent.onGoblinStunned += OnGoblinStunned;

        // NOTE(Zack): pre-allocation of function delegates
        DetectPlayerFunc = DetectPlayer;
        ChasePlayerFunc = ChasePlayer;
        MoveToDepositGoldFunc = MoveToDepositGold;
        MoveToHiddenHoleFunc = MoveToHiddenHole;
        GuardStunFunc = GuardStun;
    }

    private IEnumerator Start()
    {
        StartCoroutine(DetectPlayerFunc());
        yield return null; 
        // HACK(Zack): we wait for the next update loop, before we setup the loot stash to allow for the _lootStash,
        // Start() to be called;
        _lootStash.ResetGold();
    }

    private void OnDestroy()
    {
        _goblinAgent.onGoblinStunned -= OnGoblinStunned;
    }

    private void Update()
    {
        _Animator.SetFloat(_movementHash, _NavMeshAgent.velocity.magnitude);
    }

    private IEnumerator DetectPlayer()
    {
        _state = GuardBase.GuardState.SEARCHING;

        for (;;)
        {
            if (_detectionComponent.UpdateDetection(out Span<Transform> visibleTargets)) 
            {
                // we're currently only caring about the closest player at the moment
                _targetPlayer = visibleTargets[0];

                // NOTE(Zack): we're getting the component in the parent as the component we want may not
                // be on the same level in the hierarchy as the player capsule, or the ToolHolder collider
                Loot playerLoot = _targetPlayer.GetComponentInParent<Loot>();
                _currentStateCo = StartCoroutine(ChasePlayerFunc(playerLoot));
                break; // break from infinite loop
            }

            yield return null; // wait for update
        }

        yield break;
    }

    private IEnumerator ChasePlayer(Loot playerLoot) 
    {
        _state = GuardBase.GuardState.CHASING;

        // NOTE(Zack): we're getting the component in the parent as the component we want may not
        // be on the same level in the hierarchy as the player capsule, or the ToolHolder collider
        FirstPersonController playerController = _targetPlayer.GetComponentInParent<FirstPersonController>();

        for (;;)
        {
            // REVIEW(Zack): don't do this recalculation every frame?
            _NavMeshMovementComp.SetNewDestination(_NavMeshAgent, _targetPlayer.position);

            if (WithinRange(_goblinAgent.transform.position, _targetPlayer.position, _catchRange))
            {
                _laughSFX.Play();
                _Animator.SetTrigger(_attackHash);
                _catchComponent.CaughtPlayer(playerController, _goblinAgent._guardNativeLevel, CatchComponent.CaughtPlayerOptions.RemoveTool | CatchComponent.CaughtPlayerOptions.RemovePickup);

                // we check if the player has any loot to steal, and if they don't then we just return to the hidden hole
                if (playerLoot.currentLoot <= 0)
                {
                    _currentStateCo = StartCoroutine(MoveToHiddenHoleFunc());
                }
                else
                {
                    _stolenGold = playerLoot.TakeLoot(_lootValueToSteal);
                    _currentStateCo = StartCoroutine(MoveToDepositGoldFunc());
                }
                break;
            }


            if (!WithinRange(_goblinAgent.transform.position, _hiddenHole.position, _maxDistanceFromHiddenPosition))
            {
                _currentStateCo = StartCoroutine(MoveToHiddenHoleFunc());
                break;
            }

            yield return null; // wait for update
        }

        yield break;
    }

    private IEnumerator MoveToDepositGold()
    {
        _NavMeshMovementComp.SetNewDestination(_NavMeshAgent, _lootStash.Position);

        for (;;)
        {
            if (WithinRange(_goblinAgent.transform.position, _lootStash.Position, 0.75f)) 
            {
                DepositGold();
                break;
            }

            yield return null; // wait for update
        }

        _currentStateCo = StartCoroutine(MoveToHiddenHoleFunc());
        yield break;
    }

    private IEnumerator MoveToHiddenHole()
    {
        // set the goblin back to its hidden hole
        _NavMeshMovementComp.SetNewDestination(_NavMeshAgent, _hiddenHole.position);
        
        for (;;)
        {
            if (WithinRange(_goblinAgent.transform.position, _hiddenHole.position, 0.75f)) 
            {
                // REVIEW(Zack): add a cooldown before it starts detecting again?
                _goblinAgent.transform.position = _hiddenHole.position;
                _goblinAgent.transform.rotation = _hiddenHole.rotation;
                _currentStateCo = StartCoroutine(DetectPlayerFunc());
                break;
            }

            yield return null;
        }

        yield break;
    }

    private void DepositGold()
    {
        _lootStash.AddGold(_stolenGold);
        _stolenGold = 0;
    }

    private void OnGoblinStunned(float seconds)
    {
        // REVIEW(Zack): if the goblin is hiding we don't care about stunning it?
        if (_state != GuardBase.GuardState.CHASING) return;

        // stop the goblin from running
        if (_currentStateCo != null)
        {
            StopCoroutine(_currentStateCo);
        }

        // same stun logic as base component
        _Animator.SetTrigger(_stunnedHash);
        if (_stunCo != null)
        {
            if (_goblinAgent._currentStunLength < seconds)
            {
                StopCoroutine(_stunCo);
            }
        }

        DropGold();
        _goblinAgent._currentStunLength = seconds;
        _stunCo = StartCoroutine(GuardStunFunc(seconds));
    }

    // NOTE(Zack): we're not using the base stun as we need to restart the goblin logic after stun duration has finished
    private IEnumerator GuardStun(float seconds)
    {
        _NavMeshMovementComp.StopAgentPath(_NavMeshAgent, false);
        
        // NOTE(Zack): we're using this over WaitForSeconds so that we don't allocate unnecessary memory
        float timer = float.Epsilon;
        while (timer < seconds)
        {
            timer += Time.deltaTime;
            yield return null; // wait for update
        }

        // restart the cycle for the loot goblin to return to it's hole waiting for the player
        _currentStateCo = StartCoroutine(MoveToHiddenHoleFunc());

        _stunCo = null;
        _goblinAgent._currentStunLength = 0;
        yield break;
    }

    private void DropGold()
    {
        // TODO(Zack): remove the need to instantiate the gold, and instead get the object from an object pool
        if (_stolenGold <= 0) return;
        CoinController bag = Instantiate(_lootBagPrefab, this.transform.position, quaternion.identity);
        bag.Value = _stolenGold;
        _stolenGold = 0;
    }

    // NOTE(Zack): this function uses [distancesq()] instead of [distance()] as we do not get
    // a benefit from the sqrt that is called within [distance()]
    // it does mean that the argument [range] is squared inside of this function so that the
    // distance comparison works correctly
    private static bool WithinRange(float3 current, float3 target, float range)
    {
        range *= range;
        float dist  = math.distancesq(target, current);
        bool within = math.abs(dist) < range;
        return within;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_hiddenHole == null) return;
        Gizmos.color = Color.yellow;
        float radius = _maxDistanceFromHiddenPosition;
        Gizmos.DrawWireSphere(_hiddenHole.position, radius);
    }
#endif
}
