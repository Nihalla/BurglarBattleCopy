using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ProgressBar : MonoBehaviour
{
    public enum ProgressBarState
    {
        DISABLED,
        ENABLED,
        COMPLETE
    }

    [SerializeField] private GameObject _progressBar;
    [SerializeField] private MeshRenderer _progressBarMeshRenderer;
    [SerializeField] private Material _onMaterial;
    [SerializeField] private Material _offMaterial;
    [SerializeField] private Material _completedMaterial;
    
   
    
    private ProgressBarState _progressBarState;

    private Vector3 _maxScale;
    private Vector3 _initialScale;

    private void Awake()
    {
        //This is hardcoded to end just before the end of the background.
        //REVIEW(Sebadam2010): Should this be dynamic? As ideally people would only change the progressBarBackground and not the progressBar.
        _maxScale = new Vector3(0.9f, 0, 0);
        
        //This should be near 0.
        _initialScale = _progressBar.transform.localScale;
        _progressBarState = ProgressBarState.DISABLED;
    }

    public void ChangeState(ProgressBarState progressBarState)
    {
        
        _progressBarState = progressBarState;
        
        switch (_progressBarState)
        {
            case ProgressBarState.DISABLED:
                _progressBarMeshRenderer.material = _offMaterial;
                break;
            case ProgressBarState.ENABLED:
                _progressBarMeshRenderer.material = _onMaterial;
                break;
            case ProgressBarState.COMPLETE:
                _progressBarMeshRenderer.material = _completedMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void UpdateProgressBar(float amountToIncreaseBy, int maxAmount)
    {
        float percentage = amountToIncreaseBy / maxAmount;
        _progressBar.transform.localScale = new Vector3(_maxScale.x * percentage, _initialScale.y, _initialScale.z);
    }
}
