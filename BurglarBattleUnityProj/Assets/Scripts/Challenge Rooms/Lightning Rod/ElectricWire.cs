// Author: Christy Dwyer (ChristyDwyer)

using System.Collections.Generic;
using UnityEngine;

public class ElectricWire : MonoBehaviour, IElectricDevice
{
    public bool isPowered;

    [Header("Setup")]
    [SerializeField] private LayerMask _electricityLayer;
    [Space]

    [SerializeField] private Transform _outputTransform;
    [SerializeField] private float _connectionCheckRadius = 0.25f;

    private IElectricDevice _poweredBy;
    private List<IElectricDevice> _devicesPowering = new();

    [Header("Visuals")]
    [SerializeField] private Material _unpoweredMaterial;
    [SerializeField] private Material _poweredMaterial;

    private Renderer _renderer;

    #region DEBUG_UTILITIES
    [ContextMenu("DEBUG: Toggle Power")]
    private void DebugTogglePower()
    {
        SetPoweredDownstream(!GetPowered());
    }

    [ContextMenu("DEBUG: Refresh Connections")]
    private void DebugRefreshConnections()
    {
        RefreshConnections();
    }
    #endregion //DEBUG_UTILITIES

    private void Awake()
    {
        // NOTE(Zack): we're doing this in Awake() so that when we do the recursive function calls in,
        // RefreshConnections, we don't get null reference exceptions on start of the scene.
        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        RefreshConnections();
    }

    public void RefreshConnections()
    {
        bool wasPowered = isPowered;

        SetPoweredDownstream(false);

        // NOTE(Zack): removed the wrapping if statement on the Count, as the for loop
        // already does this check for us, so if we have a count of 0 the for loop
        // just won't run
        for (int i = 0; i < _devicesPowering.Count; ++i)
        {
            _devicesPowering[i].SetPowerSource(null);
        }

        _devicesPowering.Clear();

        Collider[] deviceColliders = Physics.OverlapSphere(_outputTransform.position, _connectionCheckRadius, _electricityLayer);
        IElectricDevice deviceToCheck;

        for (int i = 0; i < deviceColliders.Length; ++i)
        {
            deviceToCheck = deviceColliders[i].GetComponent<IElectricDevice>();

            // Not sure if this cast to IElectricDevice is necessary
            if (deviceToCheck != null && deviceToCheck != (IElectricDevice)this)
            {
                _devicesPowering.Add(deviceToCheck);
                deviceToCheck.SetPowerSource(this);
            }
        }

        SetPoweredDownstream(wasPowered);
    }

    public IElectricDevice GetPowerSource()
    {
        return _poweredBy;
    }

    public bool GetPowered()
    {
        return isPowered;
    }

    public void SetPowerSource(IElectricDevice powerSource)
    {
        _poweredBy = powerSource;
    }

    public void SetPowered(bool power)
    {
        isPowered = power;

        if (power == true)
        {
            _renderer.sharedMaterial = _poweredMaterial;
        }
        else
        {
            _renderer.sharedMaterial = _unpoweredMaterial;
        }
    }

    public void SetPoweredDownstream(bool power)
    {
        SetPowered(power);

        // NOTE(Zack): removed the wrapping if statement on the Count, as the for loop
        // already does this check for us, so if we have a count of 0 the for loop
        // just won't run
        for (int i = 0; i < _devicesPowering.Count; ++i)
        {
            _devicesPowering[i].SetPoweredDownstream(power);
        }
    }

    // NOTE(Zack): this is a uniyt editor only function, and so should never be compiled into
    // release builds, so we wrap it in a preprocessor if statement to make sure it doesn't
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isPowered ? Color.green : Color.red;
        Gizmos.DrawSphere(_outputTransform.position, _connectionCheckRadius);
    }
#endif
}
