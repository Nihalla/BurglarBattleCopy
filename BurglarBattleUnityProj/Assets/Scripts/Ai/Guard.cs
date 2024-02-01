// Author: Norbert Kupeczki - 19040948

using System;
using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic Guard logic AI, it uses a component based approach to define the behaviour
/// of a guard. It has five states:
/// PATROLING:
/// Follows a path, and looks out for the players. If a guard has a visual on any players
/// it starts switches to chase mode, if it hears one, it moves to alert mode and moves
/// to the position of the audio source.
/// 
/// ALERT:
/// Moves to the last known position of an audio or visual stimulus. If on the way there
/// the guard sees a player, it starts a chase, or updates its destination if hears any
/// player noise. If reaches the destination, it swithces to searching mode.
/// 
/// CHASING:
/// Actively chases a player, ignoring anything else. If loses sight of its target, the
/// guard switches to alert mode. If it catches the target, it applies the logic defined
/// in the catch component.
/// 
/// SEARCHING:
/// Stays in an area, looking for players. If the guard sees one, starts a chase, if it hears
/// noise, switch to alert mode and moves to that location. If the search time is up and there
/// were no stimuly from the players, the guard switches back to patrol mode.
/// 
/// STUNNED:
/// The guard is stunned for several seconds, then once the stun timer is up, it switches back to
/// its state that was active before the guard was stunned.
/// 
/// </summary>
 
[RequireComponent(typeof(NavMeshAgent))]
public class Guard : GuardBase
{
    public Guard() : base(GuardType.GUARD) { }

    [Header("Components")]
    [SerializeField] private DetectionComponent _detectionComp;
    [SerializeField] private PatrolComponent _patrolComp;
    [SerializeField] private CommunicationComponent _commComp;
    [SerializeField] private CatchComponent _catchComp;
    [Space]
    [Header("Search settings")]
    private float _searchRotationSpeed = 0.0f; // This value will be calculated based on _searchTimer in Awake()
    [SerializeField] private float _searchTimer = 3.0f;
    [SerializeField] private float _chaseTime = 3.0f;
    [Space]
    [Header("Guard state")]
    [SerializeField] private GameObject _lockedTarget;
    [SerializeField] private GameObject _exclamationMarkState;
    [SerializeField] private GameObject _questionMarkState;
    [SerializeField] private GameObject _stunnedMarkState;


    private float _searchCountdownTimer = 0.0f;
    [Space]
    [Header ("Target data")]
    [SerializeField] private Transform _targetTransform = null;
    [SerializeField] private Transform _lastTargetTransform = null;
    [SerializeField] private Vector3 _targetPosition;
    [Space]
    [Header("Communication component")]
    [SerializeField] private CommunicationComponent.MessageType _message = CommunicationComponent.MessageType.NOTHING;
    [SerializeField] private float _supportCallDelay = 3.0f;
    [Space]
    [Header("Interaction")]
    [SerializeField] private Collider _triggerZone;
    [SerializeField] private float _attackRange = 1.5f;
    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource3D _audioSource3D;
    [SerializeField] private Audio _walkSound;
    [SerializeField] private float _walkSoundEverySecs;
    [SerializeField] private Audio _alertSound;
    [SerializeField] private Audio _catchSound;

    private NavMeshAgent _agent;
    //private Renderer _indicatorRenderer;

    private Animator _Animator => base._animator;

    #region Coroutine delegates and members
    private delegate IEnumerator CallSupportCR();
    private CallSupportCR _callSupportFunc;
    private Coroutine _callSupportCoroutine;

    private delegate IEnumerator DoorOpeningCR(Lockable lockable);
    private DoorOpeningCR _openDoorFunc;

    private delegate IEnumerator StopChasingCR();
    private StopChasingCR _stopChasingFunc;
    private Coroutine _stopChasingCR;
    private WaitForSeconds _chaseCountdown;

    private const float REVERSE_DISTANCE_MOD = 2.25f;
    private WaitForSeconds _guardWaitAtDoor = new (0.5f);
    private WaitForSeconds _doorCloseDelay = new (2.0f);
    #endregion

    private const float SPEED_PATROL = 2.5f;
    private const float SPEED_CHASE = 4.0f;

    private void Awake()
    {
        base.Awake();

        Debug.Assert(_detectionComp != null, $"Detection Component is not set on {gameObject.name}");
        Debug.Assert(_patrolComp != null, $"Patrol Component is not set on {gameObject.name}");
        Debug.Assert(_catchComp != null, $"Catch Component is not set on {gameObject.name}");

        _state = GuardState.PATROLING;
        
        UpdateOverHeadIndicator();
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _callSupportFunc = CallSupport;
        _openDoorFunc = OpenDoor;
        _stopChasingFunc = ChaseCountDown;
        
        _chaseCountdown = new WaitForSeconds(_chaseTime);
        _targetTransform = null;
        _targetPosition = Vector3.zero;
        _searchRotationSpeed = 360.0f / _searchTimer;
    }

    private void Start()
    {
        _commComp.SetCommunicationLevel(_guardNativeLevel);

        // To initialise all the visual information to be available for frame zero.
        VisualDetection();
        
        _agent.speed = SPEED_PATROL;
        GuardManager.Register(this);
    }

    private void Update()
    {
        VisualDetection();
        CheckForMessages();
        
        switch (_state)
        {
            case GuardState.PATROLING:
                {
                    PatrolLogic();
                }
                break;
            case GuardState.ALERT:
                {
                    AlertLogic();
                }
                break;
            case GuardState.CHASING:
                {
                    ChasingLogic();
                }
                break;
            case GuardState.SEARCHING:
                {
                    SearchingLogic();
                }
                break;
            case GuardState.STUNNED:
                {
                    /// This state is handled by a Coroutine <see cref="GuardStunned(float)"/>
                }
                break;
            default:
                Debug.LogError("Undefined guard state. This shouldn't have happened...");
                break;
        }
        MoveGuard(_state);

        _Animator.SetFloat("Speed", _agent.velocity.magnitude);
        
        
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        // If the trigger hit by the guards interaction box and it is door interactable
         
        if (collider.gameObject.TryGetComponent(out InteractionObject door))
        {
            _triggerZone.enabled = false;
            Lockable lockable = door.GetComponentInParent<Lockable>();
            StartCoroutine(_openDoorFunc(lockable));
        }
    }

    private void UpdateOverHeadIndicator() 
    {
        switch (_state)
        {
            case GuardState.PATROLING:
                _exclamationMarkState.SetActive(false);
                _questionMarkState.SetActive(false);
                _stunnedMarkState.SetActive(false);
                break;
            case GuardState.ALERT:
                _exclamationMarkState.SetActive(false);
                _questionMarkState.SetActive(true);
                _stunnedMarkState.SetActive(false);
                break;
            case GuardState.CHASING:
                _exclamationMarkState.SetActive(true);
                _questionMarkState.SetActive(false);
                _stunnedMarkState.SetActive(false);
                break;
            case GuardState.SEARCHING:
                _exclamationMarkState.SetActive(false);
                _questionMarkState.SetActive(true);
                _stunnedMarkState.SetActive(false);
                break;
            case GuardState.STUNNED:
                _exclamationMarkState.SetActive(false);
                _questionMarkState.SetActive(false);
                _stunnedMarkState.SetActive(true);
                break;
            default:
                break;
        }
    }
//#endif

    #region >> Logic execution functions
    private void PatrolLogic()
    {
        if (_targetTransform != null)
        {
            StartChase();
        }
        else if (_message == CommunicationComponent.MessageType.ALERT)
        {
            AnswerSupportRequest();
        }
        else if (_message == CommunicationComponent.MessageType.STAND_DOWN)
        {
            // ignores the stand down messages
            IgnoreCurrentMessage();
        }
        
        //TODO: Check whether the guard hears any noise, and put the Guard to alert state
    }


    private void AlertLogic()
    {
        if (_targetTransform != null)
        {
            StartChase();
        }
        else if (_message == CommunicationComponent.MessageType.ALERT)
        {
            AnswerSupportRequest();
        }
        else if (_message == CommunicationComponent.MessageType.STAND_DOWN)
        {
            StandingDown();
        }
        else if (Vector3.Distance(gameObject.transform.position, _targetPosition) < 1.5f)
        {
            _state = GuardState.SEARCHING;

            UpdateOverHeadIndicator();
            //if (consoleDebug == true) 
            //{
            //    //Debug.Log("This is calling the set searching");
            //}
            _searchCountdownTimer = _searchTimer;
        }
    }

    private void ChasingLogic()
    {
        if (_message == CommunicationComponent.MessageType.STAND_DOWN)
        {
            StandingDown();
        }

        if (_targetTransform == null)
        {
            return;
        }

        if (TargetIsInAttackRange(_attackRange))
        {
            Attack(_targetTransform.gameObject);
            StopChaseCountdown();
            StopChase(GuardState.PATROLING); 
        }
    }

    private void SearchingLogic()
    {
        if (_message == CommunicationComponent.MessageType.ALERT)
        {
            AnswerSupportRequest();
            return;
        }
        else if (_message == CommunicationComponent.MessageType.STAND_DOWN)
        {
            StandingDown();
            return;
        }

        if (_searchCountdownTimer > 0 && _targetTransform == null)
        {   
            _searchCountdownTimer -= Time.deltaTime;
            SearchForPlayers();
        }
        else if (_targetTransform != null)
        {
            StartChase();
        }
        else
        {
            _state = GuardState.PATROLING;

            UpdateOverHeadIndicator();
        }
    }
    #endregion
    
    #region >> Helper functions
    
    /// <summary>
    /// Makes the guard rotate around in search for the player. The direction of the rotation is based
    /// on the private _searchRotationSpeed variable.
    /// </summary>
    private void SearchForPlayers()
    {
        gameObject.transform.Rotate(Vector3.up, _searchRotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Checks whether the guard is within a given distance to its target.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool TargetIsInAttackRange(float range)
    {
        return Vector3.Distance(gameObject.transform.position, _targetPosition) < range;
    }

    /// <summary>
    /// Makes the guard attack a target.
    /// </summary>
    /// <param name="target"></param>
    private void Attack(GameObject target)
    {
        _Animator.SetTrigger("Attack");
        _catchComp.CaughtPlayer(target.GetComponentInParent<FirstPersonController>(), _guardNativeLevel);

        MessageAllClear();
    }

    /// <summary>
    /// Makes guard enter chasing mode.
    /// </summary>
    private void StartChase()
    {
        _state = GuardState.CHASING;
        _agent.speed = SPEED_CHASE;
        
        UpdateOverHeadIndicator();
        SignalAggroOn(_targetTransform.gameObject);

        // Guard calling for support is temporarily disabled
        //_callSupportCoroutine = StartCoroutine(_callSupportFunc());
    }

    /// <summary>
    /// Stops the guard chasing the player, and enters a new state based on passed parameter.
    /// </summary>
    /// <param name="newState"></param>
    private void StopChase(GuardState newState)
    {
        _state = newState;
        _agent.speed = SPEED_PATROL;

        UpdateOverHeadIndicator();
        SignalAggroOff(_targetTransform.gameObject);

        // Guard calling for support is temporarily disabled
        //StopCoroutine(_callSupportCoroutine);
    }

    /// <summary>
    /// Moves the guard based on the data from its components
    /// </summary>
    /// <param name="currentState"></param>
    private void MoveGuard(GuardState currentState)
    {
        if (currentState == GuardState.PATROLING)
        {
            _nav.SetNewDestination(_agent, _patrolComp.GetTarget(gameObject));
        }
        else if (currentState != GuardState.STUNNED)
        {
            _nav.SetNewDestination(_agent, _targetPosition);
        }
    }

    /// <summary>
    /// Updates the target of the guard based on the data coming from the detection component.
    /// </summary>
    private void VisualDetection()
    {
        _detectionComp.UpdateDetection(out Span<Transform> visibleTargets);

        if (visibleTargets.Length > 0)
        {
            _targetTransform = visibleTargets[0].gameObject.transform;
            _targetPosition  = visibleTargets[0].transform.position;

            StopChaseCountdown();
        }
        else if (_state != GuardState.CHASING && _state != GuardState.STUNNED)
        {
            _lastTargetTransform = _targetTransform;
            _targetTransform = null;
        }
        else
        {
            if (_targetTransform != null)
            {
                _targetPosition = _targetTransform.position;
            }

            if (_stopChasingCR == null)
            {
                _stopChasingCR = StartCoroutine(_stopChasingFunc());
            }
        }
    }

    /// <summary>
    /// Adjusts the direction of the rotation of the searching based on the last direction of the player
    /// in relation to the guards forward vector.
    /// </summary>
    /// <returns></returns>
    private float AdjustSearchRotationAngle()
    {
        float signedAngle = Vector3.SignedAngle(gameObject.transform.forward, (_targetPosition - gameObject.transform.position), Vector3.up);

        if (signedAngle > 0)
        {
            if (_searchRotationSpeed > 0)
            {
                return _searchRotationSpeed;
            }
            return _searchRotationSpeed * -1.0f;
        }
        
        if (_searchRotationSpeed > 0)
        {
            return _searchRotationSpeed * -1.0f;
        }
        return _searchRotationSpeed;
    }
    #endregion

    #region >> Message component functions
    /// <summary>
    /// Updating the message from the communication component.
    /// </summary>
    private void CheckForMessages()
    {
        _message = _commComp.GetMessage();
    }

    /// <summary>
    /// Puts the guard in a new state and updating the target's position, then resets the communication component,
    /// allowing new messages to come through.
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="newTargetPosition"></param>
    private void ProcessMessage(GuardState newState, Vector3 newTargetPosition)
    {
        _message = CommunicationComponent.MessageType.NOTHING;
        _commComp.ClearMessage();
        _lastTargetTransform = _targetTransform;
        _targetTransform = null;
        _targetPosition = newTargetPosition;
        _state = newState;


        UpdateOverHeadIndicator();
    }

    /// <summary>
    /// Ignore all messages this frame and reset the communication component.
    /// </summary>
    private void IgnoreCurrentMessage()
    {
        _message = CommunicationComponent.MessageType.NOTHING;
        _commComp.ClearMessage();
    }

    /// <summary>
    /// Sends a message to nearby guards to come to the current target's position.
    /// </summary>
    private void MessageCallReinforcements()
    {
        _commComp.SendCommunication(CommunicationComponent.MessageType.ALERT, _targetPosition);
    }

    /// <summary>
    /// Messages nearby guards to stand down and go back to patrol.
    /// </summary>
    private void MessageAllClear()
    {
        _commComp.SendCommunication(CommunicationComponent.MessageType.STAND_DOWN, Vector3.zero);
    }

    /// <summary>
    /// Sets the guard to alert and sets a target location to investigate.
    /// </summary>
    private void AnswerSupportRequest()
    {
        ProcessMessage(GuardState.ALERT, _commComp.GetAlertLocation());
    }

    /// <summary>
    /// Makes the guard go back to patrol, resetting the target.
    /// </summary>
    private void StandingDown()
    {
        StopChaseCountdown();
        //NOTE(Felix): Had to add a check as _targetTransform is null
        SignalAggroOff(_targetTransform == null ? _lastTargetTransform.gameObject : _targetTransform.gameObject);
        ProcessMessage(GuardState.PATROLING, Vector3.zero);
    }


    /// <summary>
    /// This coroutine calls for support on a regular interval (set by the _supportCallDelay variable).
    /// </summary>
    /// <returns></returns>
    private IEnumerator CallSupport()
    {
        while (true)
        {
            MessageCallReinforcements();
            yield return new WaitForSeconds(_supportCallDelay);
        }
    }
    #endregion

    #region >> Door manipulating functions
    /// <summary>
    /// Runs the door opening logic
    /// </summary>
    /// <param name="door"></param>
    /// <returns>IEnumerator</returns>
    private IEnumerator OpenDoor(Lockable lockable)
    {
        // Establishing the default parameters:
        // Is it a hinged door or a sliding door, and is it locked or not.
        bool wasLocked = false;
        bool isDoor = false;

        if (lockable.TryGetComponent(out DoorRotating door)) // If it has the DoorRotating script attached, it is a hinged door.
        {
            isDoor = true;
        }

        // Stops the guard
        _agent.isStopped = true;

        // Unlocks the door if it was locked
        if (lockable.IsLocked)
        {
            wasLocked = true;
            lockable.ToggleLock();
        }        

        // If the door is rotating, and opens towards the guard...
        if (IsGuardInFrontOfDoor(lockable.gameObject.transform) && isDoor)
        {
            //... it should be opened in a negative direction.
            door.ToggleDoorNegative();
        }
        else if (isDoor)
        {
            // Otherwise the door should be opened in a positive direction.
            door.ToggleDoorPositive();
        }
        else
        {
            // And finally, if it is a sliding door, just open it.
            lockable.TryInteract();
        }

        // Wait for the door to open, then resume following the path.
        yield return _guardWaitAtDoor;
        _agent.isStopped = false;
        yield return _doorCloseDelay;

        // Then the guard closes the door, ...
        lockable.TryInteract();

        // ...and locks it, if it was locked originally.
        if (wasLocked)
        {
            lockable.ToggleLock();
        }
        _triggerZone.enabled = true;
    }

    /// <summary>
    /// Reverses the guard using linear interpolation
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator Reverse()
    {
        Vector3 targetPos = transform.position + transform.forward * -REVERSE_DISTANCE_MOD;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            yield return null;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 6.0f);
        }

        _agent.isStopped = false;
        yield break;
    }

    /// <summary>
    /// Checks whether the guard is in front of the door.
    /// </summary>
    /// <param name="door"></param>
    /// <returns>bool</returns>
    private bool IsGuardInFrontOfDoor(Transform door)
    {
        return Vector3.Dot(door.transform.right, transform.forward) < 0;
    }
    #endregion

    /// <summary>
    /// Starts a timer; after the "countdown" is over, it sets the guard back to alert from chasing mode.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChaseCountDown()
    {
        yield return _chaseCountdown;
        StopChase(GuardState.ALERT);
        _searchRotationSpeed = AdjustSearchRotationAngle();
        _stopChasingCR = null;
        yield break;
    }

    /// <summary>
    /// Stops the chase countdown, and resets the coroutine cache's pointer.
    /// </summary>
    private void StopChaseCountdown()
    {
        if (_stopChasingCR == null) return;
        StopCoroutine(_stopChasingCR);
        _stopChasingCR = null;
    }

    /// <summary>
    /// Applies the stun logic on the guard.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    protected override IEnumerator GuardStun(float seconds)
    {
        _Animator.SetTrigger("Stunned");
        _Animator.SetBool("UnStunned",false);
        var oldState = _state;
        _state = GuardState.STUNNED;

        UpdateOverHeadIndicator();
        _nav.StopAgentPath(_navAgent, true);
        yield return new WaitForSeconds(seconds);
        if (oldState != GuardState.CHASING)
        {
            _state = GuardState.SEARCHING;
            _targetPosition = transform.position;
            _searchCountdownTimer = _searchTimer;
        }
        else
        {
            _state = oldState;
        }

        UpdateOverHeadIndicator();

        _Animator.SetBool("UnStunned", true);
        _nav.LoadLastSavedPath(_navAgent);
        _guardStunCo = null;
        _currentStunLength = 0;
    }
}
