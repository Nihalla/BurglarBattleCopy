using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DetectionComponent))]
[RequireComponent(typeof(SoundMaker))]
[RequireComponent(typeof(CommunicationComponent))]

///<summary>
/// Static Observer for CCTV like behavior.
///
/// Once the Serialized Filelds have been configured this will sweep from left to right searching for players.
/// If a player is found, after a pre-determined ammount of time this will scream, letting nearby guards know that it has detected somthing.
/// </summary>
public class Watcher : GuardBase //(Charles) While GuardBase is usefull it seems expensive on the watcher.
{
    
    public Watcher() : base(GuardType.WATCHER) { }
    
    [Header("Components - Auto Detecting")]
    [SerializeField] private DetectionComponent _detectionComp;
    [SerializeField] private SoundMaker _soundMaker;
    [SerializeField] private CommunicationComponent _commComp;

    [Header("General")]
    [SerializeField] public bool Active = true;
    [SerializeField] private WatchState _wState;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationRange = 45.0f;
    [SerializeField] private float _rotationalSpeed = 1f;

    [Header("Misc.")]
    [SerializeField] private float _delayTillScream = 5.0f;
    [SerializeField] private float _screamDelay;
    [SerializeField] private bool _blinking = false;
    [SerializeField] private float _blinkTimer = 5.0f;
    [SerializeField] private MeshRenderer _screamMeshRenderer;
    
    [Header("Communication")]
    [SerializeField] private CommunicationComponent.MessageType _message = CommunicationComponent.MessageType.NOTHING;
    [SerializeField] private float _supportCallDelay = 3.0f;
    [SerializeField] private float _endAlarmDelay = 10.0f;

    [Header("Sound Effects")]
    [SerializeField] private Audio _screamEffect;
    // -- Norbert --

    private delegate IEnumerator BlinkingWatcher();
    private BlinkingWatcher _blinkingWatcherFunc;
    private Coroutine _blinkingCoroutine;
    private WaitForSeconds _blinkDelay;

    // -------------

    private Transform _targetTransform;

    private delegate IEnumerator CallSupportCR();
    private CallSupportCR _callSupportFunc;
    private delegate IEnumerator EndAlarmCR();
    private EndAlarmCR _endAlarmFunc;


    // Rotation 2
    private Quaternion _originRotation;
    private Quaternion _minRotation;
    private Quaternion _maxRotation;
    private float _rotationTime;
    bool _rotationDirection;

    private enum WatchState
    {
        SWEEP = 0,
        STARE = 1,
        SCREAM = 2
    }
    
    protected override IEnumerator GuardStun(float seconds)
    {
        _detectionComp.Disable(seconds);
        yield break;
    }

    private void Awake()
    {
        if (_detectionComp == null){_detectionComp = this.GetComponent<DetectionComponent>();}
        if (_soundMaker == null) { _soundMaker = this.GetComponent<SoundMaker>(); }
        if (_commComp == null) { _commComp = gameObject.GetComponent<CommunicationComponent>(); }
        Debug.Assert(_screamMeshRenderer != null, $"Scream Effect Mesh Renderer is not set on {gameObject.name}");
        _wState = WatchState.SWEEP;
        

        _originRotation = transform.rotation;
        _minRotation = Quaternion.Inverse(Quaternion.AngleAxis(_rotationRange / 2, Vector3.up)) * _originRotation;
        _maxRotation = Quaternion.AngleAxis(_rotationRange / 2, Vector3.up) * _originRotation;
        
        _endAlarmFunc = EndAlarm;

        _blinkDelay = new WaitForSeconds(_blinkTimer);
        _blinkingWatcherFunc = BlinkCamera;
    }

    private void Start()
    {
        GuardManager.Register(this);
    }

    private void Update()
    {
        if (!Active) return;
        _detectionComp.UpdateDetection(out Span<Transform> visibleTargets);

        switch (_wState)
        {
            case WatchState.SWEEP:
            {
                SweepLogic(visibleTargets);
            }
                break;
            case WatchState.STARE:
            {
                StareLogic(visibleTargets);
            }
                break;
            case WatchState.SCREAM:
            {
                ScreamLogic(visibleTargets);
                break;
            }
            default:
                Debug.LogError("Undefined Watcher state. This shouldn't have happened...");
                break;
        }
    }

    private void ScreamLogic(Span<Transform> visibleTargets)
    {
        
        if (visibleTargets.Length != 0)
        {
            /*
             * Moved to the starelogic function to happen once just before entering the screaming state
             * instead of running at every frame. - Norbert
             * 
            StopAllCoroutines();
            _blinkingCoroutine = null;
            */
            gameObject.transform.LookAt(new Vector3(_targetTransform.position.x, gameObject.transform.position.y, _targetTransform.position.z));
            MessageCallReinforcements();
        }
        else
        {
            StartCoroutine(_endAlarmFunc());
            _wState = WatchState.STARE;
        }
    }

    private void StareLogic(Span<Transform> visibleTargets)
    {
        if (visibleTargets.Length == 0)
        {
            _screamDelay = 0.0f;
            transform.rotation = _originRotation;
            _detectionComp.LerpVisionConeColour(Color.red, _delayTillScream/100);
            _wState = WatchState.SWEEP;
            _screamMeshRenderer.enabled = false;
        }
        else
        {
            if (_delayTillScream > _screamDelay)
            {
                StopAllCoroutines();
                _blinkingCoroutine = null;
                _screamDelay = 0.0f;
                _targetTransform = visibleTargets[0];
                _wState = WatchState.SCREAM;
                _screamMeshRenderer.enabled = true;
                AudioManager.PlayScreenSpace(_screamEffect);
            }
            _screamDelay += 5f * Time.deltaTime;
            _detectionComp.LerpVisionConeColour(Color.magenta, _delayTillScream/100);
            
            gameObject.transform.LookAt(new Vector3(_targetTransform.position.x, gameObject.transform.position.y, _targetTransform.position.z));
        }

    }

    private void SweepLogic(Span<Transform> visibleTargets)
    {
        // _detectionComp.LerpVisionConeColour(Color.red, 2);
        

        if (_blinking && _blinkingCoroutine == null)
        {
            _blinkingCoroutine = StartCoroutine(_blinkingWatcherFunc());
        }

        if (visibleTargets.Length > 0)
        {
            _wState = WatchState.STARE;
        }

        if (_rotationTime > 1)
        {
            _rotationDirection = false;
        }
        else if (_rotationTime < 0)
        {
            _rotationDirection = true;
        }

        if (_rotationDirection)
        {
            _rotationTime += Time.deltaTime * _rotationalSpeed;
            if (_rotationTime <= 0.5f)
            {
                transform.rotation = Quaternion.Lerp(_minRotation, _originRotation, Cubic(_rotationTime * 2));
            }
            else
            {
                transform.rotation = Quaternion.Lerp(_originRotation, _maxRotation, Cubic((_rotationTime - 0.5f) * 2));
            }
        }
        else
        {
            _rotationTime -= Time.deltaTime * _rotationalSpeed;
            if (_rotationTime <= 0.5f)
            {
                transform.rotation = Quaternion.Lerp(_minRotation, _originRotation, Cubic(_rotationTime * 2));
            }
            else
            {
                transform.rotation = Quaternion.Lerp(_originRotation, _maxRotation, Cubic((_rotationTime - 0.5f) * 2));
            }
        }
    }
    
    private static float Cubic(float val) {
        if ((val *= 2f) < 1f) return 0.5f * val * val * val;
        return 0.5f * ((val -= 2f) * val * val + 2f);
    }
    private void MessageCallReinforcements()
    {
        _commComp.SendCommunication(CommunicationComponent.MessageType.ALERT, _targetTransform.position);
    }

    private void MessageAllClear()
    {
        _commComp.SendCommunication(CommunicationComponent.MessageType.STAND_DOWN, Vector3.zero);
    }

    private IEnumerator EndAlarm()
    {
        yield return new WaitForSeconds(_endAlarmDelay);
        MessageAllClear();
    }

    private IEnumerator BlinkCamera() // Norbert
    {
        while(true)
        {
            yield return _blinkDelay;
            _detectionComp.Disable();
            yield return _blinkDelay;
            _detectionComp.Enable();
        }
    }
}
