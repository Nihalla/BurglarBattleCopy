using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFloor : MonoBehaviour
{
    [Header("Platform Parameters")]
    [SerializeField] private GameObject _trap;
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _endTransform;
    [SerializeField] private float _durationOnActivate;
    [SerializeField] private float _durationOnDeactivate;
    [SerializeField] private List<InteractionButton> _buttons;
  
    private Coroutine _moveCoroutine;
    
    private float _timer;
    private bool _activated;
    
    private void Start()
    {
        _trap.transform.position = _startTransform.position;
        _moveCoroutine = null;

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i]._onInteractEvent.AddListener(ButtonPressed); 
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i]._onInteractEvent.AddListener(ButtonPressed);
        }
    }

    private void ButtonPressed()
    {
        if (!_activated && _moveCoroutine == null)
        {
            _moveCoroutine = StartCoroutine(DropPlatform());
        }
        else if (_activated && _moveCoroutine == null)
        {
            _moveCoroutine = StartCoroutine(RaisePlatform());
        }
    }
 
    private IEnumerator DropPlatform()
    {
        _timer = float.Epsilon;
        _activated = true;
        while (_timer <= _durationOnActivate)
        {
            _trap.transform.position = Vector3.Lerp(_startTransform.position, _endTransform.position, _timer/_durationOnActivate);
            _trap.transform.rotation = Quaternion.Lerp(_startTransform.rotation, _endTransform.rotation, _timer/_durationOnActivate);
            _timer += Time.deltaTime;
            yield return null;
        }

        _moveCoroutine = null;
        yield break;
    }
    
    private IEnumerator RaisePlatform()
    {
        _timer = float.Epsilon;
        _activated = false;
        while (_timer <= _durationOnDeactivate)
        {
            _trap.transform.position = Vector3.Lerp( _endTransform.position, _startTransform.position,_timer/_durationOnDeactivate);
            _trap.transform.rotation = Quaternion.Lerp(_endTransform.rotation, _startTransform.rotation, _timer/_durationOnDeactivate);
            _timer += Time.deltaTime;
            yield return null;
        }

        _moveCoroutine = null;
        yield break;
        
    }
}
