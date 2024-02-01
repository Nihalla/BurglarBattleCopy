using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class AutomatonGuard : GuardBase
{
    public AutomatonGuard() : base(GuardType.AUTOMATON) { }

    [Header("Components")]
    [SerializeField] private DetectionComponent       _detection;
    [SerializeField] private PatrolComponent          _patrol;
    [SerializeField] private SoundDetectionComponent  _sound;
    [SerializeField] private CatchComponent           _catch;
    
    [Header("References")]
    [SerializeField] private Transform _playerCagePoint;
    [SerializeField] private Transform _lootCache;

    [Header("Guard state")]
    [SerializeField] private GameObject _exclamationMarkState;
    [SerializeField] private GameObject _questionMarkState;
    [SerializeField] private GameObject _stunnedMarkState;

    [Header("Values")]
    [SerializeField] private LayerMask _caughtPlayerLayer;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _detectionTime     = 1.5f;
    [SerializeField] private float _loseDetectionTime = 2.5f;
    [SerializeField] private float _patrolSpeed       = 1.5f;
    [SerializeField] private float _chaseSpeed;
    [SerializeField] private float _catchDistance = 2.0f;
    [SerializeField] private float _lootDrainPerSecond = 1.0f;

    [Header("Audio")]
    [SerializeField] private AudioSource3D _audioSource3D;
    [SerializeField] private Audio     _walkSound;
    [SerializeField] private float         _walkSoundEverySecs;
    [SerializeField] private Audio     _alertSound;
    [SerializeField] private Audio     _catchSound;
    
    private delegate IEnumerator LerpPlayerToCageDel(FirstPersonController player, float time);
    private LerpPlayerToCageDel LerpPlayerToCageFunc;

    private delegate IEnumerator WaitThenSetPlayerLayerDel(Transform player, FirstPersonController fpc);
    private WaitThenSetPlayerLayerDel SetPlayerLayerFunc;

    private Transform  _chaseTarget;
    private Transform  _caughtPlayer;
    private Quaternion targetRotation = Quaternion.identity;
    
    private float _timeAlert      = 0;
    private float _timeNotVisible = 0;
    private float _collectedLoot  = 0;
    
    private float _walkSoundTimer = 0;
    private bool _playedAlertSound = false;

    private Animator _Animator => base._animator;

    private void Awake()
    {
       base.Awake();
        
        Debug.Assert(_detection != null, $"Detection Component is not set on {gameObject.name}");
        Debug.Assert(_patrol != null, $"Patrol Component is not set on {gameObject.name}");
        Debug.Assert(_catch != null, $"Catch Component is not set on {gameObject.name}");

        LerpPlayerToCageFunc = LerpPlayerToCage;
        SetPlayerLayerFunc = WaitToSetPlayerLayer;
        
       _navAgent.speed = _patrolSpeed;
       _lootCache.gameObject.SetActive(false);
    }
 
    private void Update()
    {
        
        /*if (__DEBUG_Stun)
        {
            __DEBUG_Stun = false;
            StunGuard(5);
        }*/
        _walkSoundTimer += Time.deltaTime;
        
        bool detected = _detection.UpdateDetection(out Span<Transform> visibleTargets);
        switch (_state)
        {
            case GuardState.PATROLING:
                Patrol(detected, visibleTargets);
                if (_walkSoundTimer >= _walkSoundEverySecs)
                {
                    _walkSoundTimer = 0;
                    _audioSource3D.AudioClip = _walkSound;
                    _audioSource3D.Play();
                }
                break;
            case GuardState.ALERT:
                Alert(detected, visibleTargets);
                break;
            case GuardState.CHASING:
                Chase(detected, visibleTargets);
                if (_walkSoundTimer >= _walkSoundEverySecs)
                {
                    _walkSoundTimer = 0;
                    _audioSource3D.AudioClip = _walkSound;
                    _audioSource3D.Play();
                }
                break;
            case GuardState.SEARCHING: break;
            case GuardState.STUNNED:   break;
        }

        _Animator.SetFloat("Speed", _navAgent.velocity.magnitude);
        
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

    public void OnPlayerTakeLoot(PlayerInteraction playerInteraction)
    {
        Loot playerLoot = playerInteraction.PlayerProfile.playerController.GetComponent<Loot>();
        playerLoot.SetCurrentLoot(playerLoot.GetCurrentLoot() + _collectedLoot);
        _collectedLoot = 0;
        _lootCache.gameObject.SetActive(false);
    }    

    private void Chase(bool detected, Span<Transform> visibleTargets)
    {
        void Follow()
        {
            _navAgent.speed = _chaseSpeed;
            _nav.SetNewDestination(_navAgent, _chaseTarget.position);
        }

        if (detected)
        {
            _chaseTarget = visibleTargets[0];
            Follow();
            Vector3 directionToTarget = _chaseTarget.position - transform.position;
            if (directionToTarget.magnitude <= _catchDistance)
            {
                FirstPersonController player = _chaseTarget.GetComponent<FirstPersonController>();
                StartCoroutine(LerpPlayerToCageFunc(player, 0.5f));
                _caughtPlayer = _chaseTarget;
                _audioSource3D.AudioClip = _catchSound;
                _audioSource3D.Play();
                SwitchPatrol();
            }
        }
        else
        {
            _timeNotVisible += Time.deltaTime;
            if (_timeNotVisible >= _loseDetectionTime)
            {
                SwitchPatrol();
                return;
            }

            _detection.SetVisionConeColour(Color.Lerp(Color.white, Color.red, _timeNotVisible / _loseDetectionTime));
            Follow();
        }
    }
    
    private IEnumerator LerpPlayerToCage(FirstPersonController player, float moveTime)
    {
        _Animator.SetTrigger("Attack");
        player.SetStunnedState(true);
        player.gameObject.layer = (int)Mathf.Log(_caughtPlayerLayer, 2);
        player.transform.SetParent(_playerCagePoint);
        player.rb.useGravity = false;
        player.rb.isKinematic = true;
        float time = 0;
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;
        while (time < moveTime)
        {
            time += Time.deltaTime;
            if (_state == GuardState.STUNNED)
            {
                DropPlayer();
                yield break;
            }
            player.transform.position = Vector3.Lerp(startPos, _playerCagePoint.position, Quadratic(time / moveTime));
            player.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(Vector3.zero), Quadratic(time / moveTime));
            yield return null; //wait for update
        }

        player.transform.position = _playerCagePoint.position;
        player.transform.localRotation = Quaternion.Euler(Vector3.zero);
        
        //TODO: drain moneys
        Loot playerLoot = player.GetComponent<Loot>();
        // playerLoot.SetCurrentLoot(500);
        time = 0;
        if (playerLoot.currentLoot > float.Epsilon)
        {
            _lootCache.gameObject.SetActive(true);
        }
        
        while (true)
        {
            if (playerLoot.GetCurrentLoot() <= 0)
            {
                break;
            }

            time += Time.deltaTime;
            if (time > 1)
            {
                float lootToRemove = _lootDrainPerSecond;
                if ((playerLoot.GetCurrentLoot() - _lootDrainPerSecond) < 0 )
                {
                    lootToRemove = playerLoot.GetCurrentLoot();
                }
                
                playerLoot.SetCurrentLoot(playerLoot.GetCurrentLoot() - lootToRemove);
                time = 0;
                _collectedLoot += _lootDrainPerSecond;
            }

            if (_state == GuardState.STUNNED)
            {
                DropPlayer();
                yield break;
            }

            yield return null;
        }
        DropPlayer(false);
        // _caughtPlayer.SetParent(null);
        // var fpc = _caughtPlayer.GetComponent<FirstPersonController>();
        // player.rb.useGravity  = true;
        // player.rb.isKinematic = false;
        // player.SetStunnedState(false);
        // player.gameObject.layer = (int)Mathf.Log(_caughtPlayerLayer, 2);
        //_catch.CoughtPlayer(player, false);

        _catch.CaughtPlayer(player, _guardNativeLevel, CatchComponent.CaughtPlayerOptions.RemovePickup | CatchComponent.CaughtPlayerOptions.RemoveTool);
    }
    
    public static float Quadratic(float val) {
        if ((val *= 2f) < 1f) return 0.5f * val * val;
        return -0.5f * ((val -= 1f) * (val - 2f) - 1f);
    }
    
    
    private void Alert(bool detected, Span<Transform> visibleTargets)
    {
        if (_timeAlert < _detectionTime)
        {
            if (detected)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, (_timeAlert / _detectionTime) * 2);
                _detection.SetVisionConeColour(Color.Lerp(Color.red, Color.white, _timeAlert / _detectionTime));
                _timeAlert += Time.deltaTime;
            }
            else
            {
                SwitchPatrol();
            }
        }
        else
        {
            _timeAlert   = 0;
            _state       = GuardState.CHASING;
            _chaseTarget = visibleTargets[0];
            if (_caughtPlayer != null)
            {
                //we need to drop 'em
                DropPlayer();
            }
        }
    }
    
    private void Patrol(bool detected, Span<Transform> visibleTargets)
    {
        Vector3 target = _patrol.GetTarget(gameObject);
        _nav.SetNewDestination(_navAgent, target);
        if (detected)
        {
            Vector3 diff = visibleTargets[0].position - transform.position;
            diff.y         = 0;
            targetRotation = Quaternion.LookRotation(diff, Vector3.up);
            SwitchAlert();
        }
    }

    void SwitchAlert()
    {
        UpdateOverHeadIndicator();
        _nav.StopAgentPath(_navAgent, false);
        _navAgent.speed     = _patrolSpeed;
        _state              = GuardState.ALERT;
        _timeAlert          = 0;
        // _timeStunned        = 0;
        // _timeToBeStunnedFor = 0;
        _timeNotVisible     = 0;
        _audioSource3D.AudioClip = _alertSound;
        _audioSource3D.Play();
    }

    void SwitchPatrol()
    {
        UpdateOverHeadIndicator();
        _detection.LerpVisionConeColour(Color.red, 0.5f);
        _navAgent.speed     = _patrolSpeed;
        _state              = GuardState.PATROLING;
        _timeAlert          = 0;
        // _timeStunned        = 0;
        // _timeToBeStunnedFor = 0;
        _timeNotVisible     = 0;
    }

    void DropPlayer(bool stun = true)
    {
        if (_caughtPlayer == null)
        {
            return;
        }
        _caughtPlayer.SetParent(null);
        UpdateOverHeadIndicator();
        
        var fpc = _caughtPlayer.GetComponent<FirstPersonController>();
        fpc.rb.useGravity = true;
        fpc.rb.isKinematic = false;
        fpc.SetStunnedState(false);
        if (stun)
        {
            StartCoroutine(SetPlayerLayerFunc(_caughtPlayer, fpc));
        }
        _caughtPlayer = null;
    }

    private IEnumerator WaitToSetPlayerLayer(Transform player, FirstPersonController fpc)
    {
        fpc.StunPlayerForTimer(3);
        yield return new WaitForSeconds(2);
        player.gameObject.layer = (int)Mathf.Log(_playerLayer, 2);
    }

    protected override IEnumerator GuardStun(float seconds)
    {
        UpdateOverHeadIndicator();
        DropPlayer(false);
        _Animator.SetBool("UnStunned", false);
        _Animator.SetTrigger("Stunned");
        var oldState = _state;
        _state = GuardState.STUNNED;
        _nav.StopAgentPath(_navAgent, true);
        yield return new WaitForSeconds(seconds);
        _Animator.SetBool("UnStunned", true);
        _state = oldState;
        _nav.LoadLastSavedPath(_navAgent);
        _guardStunCo = null;
        _currentStunLength = 0;
    }
}
