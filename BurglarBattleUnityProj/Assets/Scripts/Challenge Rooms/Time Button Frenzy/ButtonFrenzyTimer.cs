using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFrenzyTimer : MonoBehaviour
{
    private enum TimerState        
    {
        OFF,
        ON,
        COMPLETED
    }

    [SerializeField] private GameObject _timerParentObject;
    [SerializeField] private GameObject _timerObject;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _completedMaterial;
    
    [SerializeField] private float _timeLimit = 30f;

    public delegate void TimerDel();
    public event TimerDel OnTimerFinishedEvent;

    private bool _timerRunning = false;

    private float _timeRemaining;

    private TimerState _timerState;

    private Vector3 _initialScale;
    
    private MeshRenderer _timerMeshRenderer;
    
    private Coroutine _startingTimerCoroutine;

    private void Awake()
    {
        _timerMeshRenderer = _timerObject.GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        _timerState = TimerState.OFF;
        _timeRemaining = _timeLimit;
        
        _initialScale = _timerParentObject.transform.localScale;
    }

    public void StartTimer()
    {
        if (_timerRunning)
        {
            return;
        }
        if (_startingTimerCoroutine != null)
        {
            StopCoroutine(_startingTimerCoroutine);
            
        }
        ChangeColour(TimerState.ON);
        _timeRemaining = _timeLimit;
        _timerRunning = true;
        _startingTimerCoroutine = StartCoroutine(StartingTimer());
    }

    private IEnumerator StartingTimer()
    {
        while (_timeRemaining > 0 && _timerRunning)
        {
            _timeRemaining -= Time.deltaTime;
            _timerParentObject.transform.localScale = new Vector3(_initialScale.x * (_timeRemaining / _timeLimit), _timerParentObject.transform.localScale.y, _timerParentObject.transform.localScale.z);
            yield return null;
        }
        if (_timeRemaining <= 0)
        {
            OnTimerFinishedEvent?.Invoke();
        }
    }

    public void StopTimer()
    {
        _timerRunning = false;
        ChangeColour(TimerState.OFF);
    }

    public void RestartTimer()
    {
        _timeRemaining = _timeLimit;
        _timerParentObject.transform.localScale = _initialScale;
    }

    public void CompleteTimer()
    {
        _timerRunning = false;
        ChangeColour(TimerState.COMPLETED);
    }
    
    private void ChangeColour(TimerState timerState )
    {
        switch (timerState)
        {
            case TimerState.OFF:
                _timerMeshRenderer.material = _offMaterial;
                break;
            case TimerState.ON:
                _timerMeshRenderer.material = _onMaterial;
                break;
            case TimerState.COMPLETED:
                _timerMeshRenderer.material = _completedMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(timerState), timerState, null);
        }
    }
    
    
}
