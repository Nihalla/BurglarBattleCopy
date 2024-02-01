// Author: Matteo Bolognesi

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private int _activePillars;
    [SerializeField] private int _pillarsToActivate = 4;

    public UnityEvent OnChallengeCompleteEvent;

    private void Awake()
    {
        _activePillars = 0;
    }
    
    public void ChangeActivePillarCount(int value)
    {
        _activePillars += value;
        if (_activePillars != _pillarsToActivate)
        {
            return;
        }
        OnChallengeCompleteEvent?.Invoke();
       // //Debug.Log("Open");
    }

}
