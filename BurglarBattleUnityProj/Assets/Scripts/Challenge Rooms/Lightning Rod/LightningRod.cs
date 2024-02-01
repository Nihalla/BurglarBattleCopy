using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LightningRod : MonoBehaviour
{
    public bool isPowered;

    [Header("Lightning Strike")]
    [SerializeField] private float _minStrikeTime;
    [SerializeField] private float _maxStrikeTime;
    [SerializeField] private float _powerDuration = 15f;
    private float _strikeTime;
    private float _lightningTimer;
    [Space]

    [Header("Visual")]
    [SerializeField] private Material _unpoweredMaterial;
    [SerializeField] private Material _poweredMaterial;
    [Space]
    [SerializeField] private VisualEffect _lightningVFX;
    [Space]

    [Header("Sound")]
    [SerializeField] private Audio _thunderSound;
    [SerializeField] private Audio _lightningSound;
    [SerializeField] private float _timeBetweenSounds;
    private bool _thunderSoundPlayed;
    [Space]

    [Header("References")]
    [SerializeField] private GameObject _cap;
    [SerializeField] private GameObject _pole;
    [SerializeField] private GameObject _outWireObject;

    private IElectricDevice _outWire;
    private Renderer _capRenderer;
    private Renderer _poleRenderer;

    private void Awake()
    {
        SetNewStrikeTime();

        _lightningTimer = 0;
        isPowered = false;

        _outWire = _outWireObject.GetComponent<IElectricDevice>();

        _capRenderer = _cap.GetComponent<Renderer>();
        _poleRenderer = _pole.GetComponent<Renderer>();

        _lightningVFX.enabled = false;

        _thunderSoundPlayed = false;
    }

    private void Update()
    {
        _lightningTimer += Time.deltaTime;

        if (!isPowered)
        {
            if (_lightningTimer >= _strikeTime)
            {
                AudioManager.PlayOneShotWorldSpace(_lightningSound, _cap.transform.position);

                isPowered = true;
                _lightningTimer = 0;

                _capRenderer.sharedMaterial = _poweredMaterial;
                _poleRenderer.sharedMaterial = _poweredMaterial;

                _lightningVFX.enabled = true;

                _outWire.SetPoweredDownstream(true);

                return;
            }

            else if (!_thunderSoundPlayed && _lightningTimer >= _strikeTime - _timeBetweenSounds)
            {
                _thunderSoundPlayed = true;

                AudioManager.PlayOneShotWorldSpace(_thunderSound, _cap.transform.position);
            }
        }

        else
        {
            if (_lightningTimer >= _powerDuration)
            {
                _thunderSoundPlayed = false;

                isPowered = false;
                _lightningTimer = 0;

                _capRenderer.sharedMaterial = _unpoweredMaterial;
                _poleRenderer.sharedMaterial = _unpoweredMaterial;

                _lightningVFX.enabled = false;

                _outWire.SetPoweredDownstream(false);
            }
        }
    }

    private void SetNewStrikeTime() => _strikeTime = Random.Range(_minStrikeTime, _maxStrikeTime);
}

