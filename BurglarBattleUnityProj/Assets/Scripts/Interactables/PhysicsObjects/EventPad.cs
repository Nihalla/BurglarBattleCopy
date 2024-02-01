// Author: Vlad Trakiyski

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventPad : MonoBehaviour
{
    [SerializeField] private LayerMask _triggerMask;

    [SerializeField] private UnityEvent onEnterEvent;
    [SerializeField] private UnityEvent onExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (((_triggerMask.value & (1 << other.gameObject.layer)) == 0))
        {
            return;
        }
        onEnterEvent.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onExitEvent.Invoke();
    }
}
