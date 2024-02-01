// Author: Zack Collins

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// This script moves the different zones/layers of the vault to their relevant floors,
/// when the <see cref="_floor1Triggers"/> <see cref="_floor2Triggers"/> fire events when players enter into the triggers.
/// </summary>
public class VaultMovement : MonoBehaviour 
{
    [Header("Vault References")]
    [SerializeField] private Transform _zonesParent;
    [SerializeField] private Transform _zonesInner;
    [SerializeField] private Rigidbody _vaultPickup;

    [Header("Floor Position References")]
    [SerializeField] private Transform _floor2Pos;
    [SerializeField] private Transform _floor3Pos;

    [Header("Lerp Settings")]
    [SerializeField] private bool _lerpToFloor2OnStart = true;
    [SerializeField] private float _initialDelay = 1.5f;
    [SerializeField] private float _movementDuration = 10f;

#if UNITY_EDITOR
    [Header("Development Settings")]
    [SerializeField] private bool _startOnFloor2 = false;
    [SerializeField] private bool _startOnFloor3 = false;
#endif

    [Header("Sound Effects")]
    [SerializeField] private Audio _grindingEffect;

    private bool _movedToFloor2 = false;
    private bool _movedToFloor3 = false;

    private Background_Music_Manager _BGMmanager;

    private delegate IEnumerator FloorMoveDel();
    private FloorMoveDel Floor3Func;

    private void Awake() 
    {
        Debug.Assert(_zonesParent != null, "Zones Parent has not been set in the inspector",     this);
        Debug.Assert(_zonesInner  != null, "Zones Inner has not been set in the inspector",      this);
        Debug.Assert(_floor2Pos   != null, "Floor 2 Position has not been set in the inspector", this);
        Debug.Assert(_floor3Pos   != null, "Floor 3 Position has not been set in the inspector", this);
        Debug.Assert(_vaultPickup != null, "Do not have a reference to the vault pickup. Please drag into the inspector", this);

        _BGMmanager = GameObject.FindObjectOfType<Background_Music_Manager>();

        // NOTE(Zack): this should stop the vault pick up from falling onto the floor
        _vaultPickup.isKinematic = true;

        Floor3Func = Floor3;

#if UNITY_EDITOR
        // move both sections to the second floor
        if (_startOnFloor2) {
            _zonesParent.position = _floor2Pos.position;
            _movedToFloor2 = true;
        } 

        // move all platforms to their final positions
        if (_startOnFloor3) {
            _zonesParent.position = _floor2Pos.position;
            _zonesInner.position  = _floor3Pos.position;
            _movedToFloor2 = true;
            _movedToFloor3 = true;
            _vaultPickup.isKinematic = false;
        }
#endif
    }

    private IEnumerator Start()
    {
        if (!_lerpToFloor2OnStart) yield break;

        float timer = float.Epsilon;
        while (timer < _initialDelay) 
        {
            timer += Time.deltaTime;
            yield return null; // wait for update;
        }

        MoveToFloor2();
        yield break;
    }

    public void MoveToFloor2()
    {
        if (_movedToFloor2) return;
        _movedToFloor2 = true;

        float3 start = _zonesParent.position;
        float3 end   = _floor2Pos.position;
        float duration = _movementDuration;

        StartCoroutine(Lerp.ToPositionFunc(_zonesParent, start, end, duration, LerpModifier));

        AudioManager.PlayScreenSpace(_grindingEffect);
    }

    public void MoveToFloor3()
    {
        if (_movedToFloor3) return;
        _movedToFloor3 = true;

        StartCoroutine(Floor3Func());

        _BGMmanager.SetAudioState(2);
        _BGMmanager.UpdateMusic();

        AudioManager.PlayScreenSpace(_grindingEffect);
    }

    private IEnumerator Floor3() 
    {
        float3 start = _zonesInner.position;
        float3 end   = _floor3Pos.position;
        float duration = _movementDuration;

        yield return Lerp.ToPositionFunc(_zonesInner, start, end, duration, LerpModifier);
        
        // set it so that the vault chalice can be picked up
        _vaultPickup.isKinematic = false;

        yield break;
    }

    // NOTE(Zack): interpolation modifier = ease-out back
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LerpModifier(float t)
    {
        const float s = 1.70158f;
        const float c3 = s + 1f;
        return 1f + c3 * math.pow(t - 1f, 3f) + s * math.pow(t - 1f, 2f);
    }
}

