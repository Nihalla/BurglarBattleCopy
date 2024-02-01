// Author: Tane Cotterell-East (Roonstar96)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpening : MonoBehaviour
{
    [Header("Object & Transform settings")]
    [SerializeField] private GameObject _doorObject;
    [SerializeField] private Transform _openPos;
    [SerializeField] private Transform _closedPos;

    [Header("Door Movement settings")]
    [SerializeField] private float _duration;
    public bool isClosed;

    private float _timer;

    private void Awake()
    {
        _doorObject.transform.position = _closedPos.transform.position;
        isClosed = true;
    }

    public void CloseDoor()
    {
        _timer = float.Epsilon;
        isClosed = true;
        while (_timer <= _duration)
        {
            _doorObject.transform.position = Vector3.Lerp(_openPos.transform.position, _closedPos.transform.position, _timer / _duration);
            _timer += Time.deltaTime;
        }
    }

    public void OpenDoor()
    {
        _timer = float.Epsilon;
        isClosed = false;
        while (_timer <= _duration)
        {
            _doorObject.transform.position = Vector3.Lerp(_closedPos.transform.position, _openPos.transform.position, _timer / _duration);
            _timer += Time.deltaTime;
        }
    }
}
