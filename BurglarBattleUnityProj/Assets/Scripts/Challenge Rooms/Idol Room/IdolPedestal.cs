// Author: Zack Collins

using PlayerControllers;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.Mathematics;

/// <summary>
/// Pedestal that uses <see cref="IdolType"/> to check if an object is the correct type to match to it.
/// Script envokes a Unity Event <see cref="onIdolPlacedUnityEvent"/> and C# delegate event
/// <see cref="onIdolPlacedDelegateEvent"/> when an idol is matched to the correct pedestal.
/// </summary>
public class IdolPedestal : MonoBehaviour
{
    [Header("Scene Transition")] // HACK(Zack): this should *not* be done here, but it is the easiest thing to get it to work
    [SceneAttribute, SerializeField] private int _endScene;

    [Header("References")]
    [SerializeField] private BoxCollider _pedestalTrigger;
    [SerializeField] private LayerMaskTrigger _layerTrigger;
    [SerializeField] private Transform _finalIdolTransform;
    [SerializeField] private ObjectBobPosition _bobComponent;

    [Header("Pedestal Settings")]
    [SerializeField] private IdolTypeFlag _typeWanted = 0;
    [SerializeField] private float _positionLerpDuration = 1f;
    [SerializeField] private float _rotationLerpDuration = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource3D _idolPlacedSFX; 
    [Space] // makes a space between this and the UnityEvent GUI

    // NOTE(Zack): to be used as a gui event
    public UnityEvent onIdolPlacedUnityEvent = new UnityEvent();

    // NOTE(Zack): to be used in scripts via reference to this component
    public delegate void EventDel();
    public EventDel onIdolPlacedDelegateEvent;
    
    private delegate IEnumerator DelayAndSignalDel(Transform idol, bool isVault);
    private DelayAndSignalDel DelayAndSignalIdolPlacedFunc;

    private Lerp.LerpModifierDel LerpModifierFunc;
    private bool _subbedToEvents = false;

    private void Awake()
    {
        Debug.Assert(_pedestalTrigger    != null, "_pedestalTrigger is null. Please set in the inspector.",   this);
        Debug.Assert(_layerTrigger       != null, "_layerTrigger is null. Please set in the inspector.",      this);
        Debug.Assert(_finalIdolTransform != null, "_finalIdolTransform is null. Please set in the inspector", this);
        Debug.Assert(_idolPlacedSFX      != null, "_idolPlacedSFX is null. Please set in the inspector",      this);

        // NOTE(Zack): pre-allocating delegates so we remove as many runtime allocations as possible
        DelayAndSignalIdolPlacedFunc = DelayAndSignalIdolPlaced;
        LerpModifierFunc = LerpModifier;

        _layerTrigger.onAnyColliderEnter += OnIdolEntered;
        _subbedToEvents = true;
    }

    private void OnDestroy()
    {
        if (!_subbedToEvents) return;
        _layerTrigger.onAnyColliderEnter -= OnIdolEntered;
    }

    
    private void OnIdolEntered(Collider other)
    {
        IdolType idol = other.gameObject.GetComponent<IdolType>();
        if (idol == null) return;

        // if this idol is not of the type we want we return
        if (_typeWanted != idol.type) return;

        // unsub from events so that we're not doing unnecessary checks
        _layerTrigger.onAnyColliderEnter -= OnIdolEntered;
        _subbedToEvents = false;

        PickUpInteractable pickup = other.gameObject.GetComponent<PickUpInteractable>();
        IdolRespawn respawn       = other.gameObject.GetComponent<IdolRespawn>();
        Rigidbody rb              = other.gameObject.GetComponent<Rigidbody>();

        rb.isKinematic = true; // removes the object from the physics loop
        pickup.ForceDrop();
        respawn.DisableRespawn();
        
        // NOTE(Zack): if this is the vault idol that has been placed then we check the team,
        // and then change that teams gold amount
        bool isVault = idol.type == IdolTypeFlag.VAULT;
        if (isVault)
        {
            // TODO(Zack): allow this value to be changed from the inspector
            const int finalGoldAmount = 300;
            if (pickup.Idol.profile.Team == FirstPersonController.PlayerTeam.TEAM_ONE) 
            {
                GoldTransferToEnd.team1Gold += finalGoldAmount;
            } 
            else
            {
                GoldTransferToEnd.team2Gold += finalGoldAmount;
            }
        }

        // disable the components from this object so that it can no longer be interacted with by the player
        pickup.DisablePickup();
        pickup.enabled = false;
        other.enabled  = false;

        StartCoroutine(DelayAndSignalIdolPlacedFunc(idol.transform, isVault));
    }

    private IEnumerator DelayAndSignalIdolPlaced(Transform idol, bool isVault)
    {   
        // NOTE(Zack): set the idol's parent as the lerps below require local space to function correctly
        idol.parent = this.transform;

        quaternion startRot = idol.localRotation;
        quaternion endRot = _finalIdolTransform.localRotation;
        StartCoroutine(Lerp.ToRotationLocalFunc(idol, startRot, endRot, _rotationLerpDuration, LerpModifierFunc));

        float3 startPos = idol.localPosition;
        float3 endPos = _finalIdolTransform.localPosition;
        StartCoroutine(Lerp.ToPositionLocalFunc(idol, startPos, endPos, _positionLerpDuration, LerpModifierFunc));

        // we play the confirmation for placing the correct idol
        _idolPlacedSFX.Play();

        // HACK(Zack): we delay for the time it takes for the longest lerp and then we send the events
        float duration = math.max(_positionLerpDuration, _rotationLerpDuration);
        float elapsed = float.Epsilon;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        //disabled to make the game run full duration
        //if (isVault) 
        //{
        //    StartCoroutine(TransferToEnd()); 
        //}

        // set the pedestals bob component to know which object it is going to be bobbing up and down
        _bobComponent.SetObjectToBob(idol);
        _bobComponent.StartBobbing();


        onIdolPlacedUnityEvent?.Invoke();
        onIdolPlacedDelegateEvent?.Invoke();
        yield break;
    }

    // NOTE(Zack): interpolation modifier = ease-out back
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float LerpModifier(float t)
    {
        const float s = 1.70158f;
        const float c3 = s + 1f;
        return 1f + (c3 * math.pow(t - 1f, 3f)) + (s * math.pow(t - 1f, 2f));
    }

    private IEnumerator TransferToEnd() 
    {
        if (FadeTransition.instance != null)
        {
            FadeTransition.instance.FadeIn();
        }
        
        float timer = float.Epsilon;
        while (timer < 1f) // fade transtion length
        {
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadSceneAsync(_endScene);
        yield break;
    }
}
