// Author: Christy Dwyer (ChristyDwyer)

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingWireBase : MonoBehaviour, IInteractable
{
    [Header("Config")]
    [SerializeField] private int[] _rotations;
    [SerializeField] private GameObject _outputWireObject;
    private IElectricDevice _wire;
    private int _currentRotation;

    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
    [Space]
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _holdMat;

    private MeshRenderer _meshRenderer => _meshRenderers[0];
    private int _currentMat = 1;

    #region DEBUG_UTILITIES
    [ContextMenu("DEBUG: Rotate Wire")]
    private void DebugRotateWire()
    {
        RotateWire();
    }
    #endregion //DEBUG_UTILITIES

    private void Awake()
    {
        _currentRotation = 0;

        Vector3 rotation = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(rotation.x, _rotations[_currentRotation], rotation.z);

        _wire = _outputWireObject.GetComponent<IElectricDevice>();
    }


    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    /*
    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _hoverMat;
    }

    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }
    */

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        RotateWire();
    }

    private void RotateWire()
    {
        if (_currentRotation >= _rotations.Length - 1)
        {
            _currentRotation = 0;
        }

        else
        {
            _currentRotation += 1;
        }

        Vector3 rotation = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(rotation.x, _rotations[_currentRotation], rotation.z);

        IElectricDevice powerSource = _wire.GetPowerSource();
        bool isSourcePowered = powerSource.GetPowered();

        _wire.RefreshConnections();
        powerSource.RefreshConnections();

        powerSource.SetPoweredDownstream(isSourcePowered);
    }
}
