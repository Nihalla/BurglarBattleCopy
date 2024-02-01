// Author: Christy Dwyer (ChristyDwyer)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ElectricController : MonoBehaviour, IElectricDevice
{
    public bool isPowered;

    [Header("Setup")]
    [SerializeField] private LayerMask _electricityLayer;
    [Space]

    [SerializeField] private UnityEvent _poweredOnEvent;
    [SerializeField] private UnityEvent _poweredOffEvent;
    [Space]

    private IElectricDevice _poweredBy;

    [Header("Visuals")]
    [SerializeField] private Material _unpoweredMaterial;
    [SerializeField] private Material _poweredMaterial;
    [Space]

    private Renderer _renderer;

    [Header("Sound")]
    [SerializeField] private Audio _powerOnSound;
    [SerializeField] private Audio _powerOffSound;
    
    private bool _poweringOn;
    private WaitForSeconds _audioDelay;

    private const float AUDIO_DELAY_TIME = 0.4f;

    private delegate IEnumerator PlaySoundDel(Audio sound);
    private PlaySoundDel PlaySoundAfterDelayFunc;

    private void Awake()
    {
        isPowered = false;
        _poweredBy = null;

        _renderer = GetComponent<Renderer>();

        _poweringOn = false;
        // Caching the WaitForSeconds so we aren't reinstancing every time the PlaySoundAfterDelay() couroutine is started
        _audioDelay = new WaitForSeconds(AUDIO_DELAY_TIME);

        // NOTE(Zack): we're pre-allocating the memory for the coroutine, so that we don't allocate memory,
        // every time we start the coroutine
        PlaySoundAfterDelayFunc = PlaySoundAfterDelay;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void RefreshConnections()
    {
        SetPowered(false);
        SetPowerSource(null);
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
            if (!_poweringOn)
            {
                StartCoroutine(PlaySoundAfterDelayFunc(_powerOnSound));
            }

            _renderer.sharedMaterial = _poweredMaterial;
            _poweredOnEvent?.Invoke();
        }

        else
        {
            AudioManager.PlayOneShotWorldSpace(_powerOffSound, transform.position);

            _renderer.sharedMaterial = _unpoweredMaterial;
            _poweredOffEvent?.Invoke();
        }
    }

    public void SetPoweredDownstream(bool power)
    {
        SetPowered(power);
    }

    private IEnumerator PlaySoundAfterDelay(Audio sound)
    {
        _poweringOn = true;
        yield return _audioDelay;

        AudioManager.PlayOneShotWorldSpace(sound, transform.position);
        _poweringOn = false;
    }
}
