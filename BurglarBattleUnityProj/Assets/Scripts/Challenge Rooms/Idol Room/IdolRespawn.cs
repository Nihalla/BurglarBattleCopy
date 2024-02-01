using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(CachePosition))]
public class IdolRespawn : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private PickUpInteractable _pickupInteractable;
    
    [Header("Respawn Settings")]
    [SerializeField, TimeField] private float _countdownDuration = 20f;

    private delegate IEnumerator CountdownDel(float duration);
    private CountdownDel CountdownFunc;
    private Coroutine _countdownCo;

    // components
    private CachePosition _cachedPosition;
    private Material _objectMaterial;

    private bool _subscribed = false;
    private float _maxLerpValue;
    private static readonly int _highlightValue = Shader.PropertyToID("_Highlight_Value");

    private void Awake()
    {
        _cachedPosition = GetComponent<CachePosition>();
        Debug.Assert(_cachedPosition     != null, "Could not get component CachedPosition. Please add one in the inspector", this);
        Debug.Assert(_pickupInteractable != null, "PickUpInteractable is null. Please assign in the inspector",              this);

        _pickupInteractable.onItemInteraction += OnItemInteraction;
        _subscribed = true;

        // setup the material so that we can use it to be able to lerp the colour of the value
        Renderer renderer = GetComponent<Renderer>();
        Material m = renderer.material;
        Material mat = new Material(m);
        renderer.material = mat;
        _objectMaterial = renderer.material;

        _maxLerpValue = _objectMaterial.GetFloat(_highlightValue);
        
        // NOTE(Zack): pre-allocating function delegates
        CountdownFunc = Countdown;
    }

    private void OnDestroy()
    {
        if (!_subscribed) return;
        _pickupInteractable.onItemInteraction -= OnItemInteraction;
    }

    public void DisableRespawn()
    {
        if (_countdownCo != null)
        {
            StopCoroutine(_countdownCo);
        }

        _pickupInteractable.onItemInteraction -= OnItemInteraction;
        _subscribed = false;
    }

    private void OnItemInteraction()
    {
        if (_countdownCo != null) return;
        _countdownCo = StartCoroutine(CountdownFunc(_countdownDuration));
    }


    private IEnumerator Countdown(float duration)
    {
        const float MIN_SPEED = 1f;
        const float MAX_SPEED = 11f;

        float speedIncreaseMax = duration * 0.9f;
        float timer = float.Epsilon;
        float speed = MIN_SPEED;
        float start = _maxLerpValue;
        float end   = _maxLerpValue / 3f;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            float t = timer / duration;

            speed = math.lerp(MIN_SPEED, MAX_SPEED, t);

            float s = math.sin(timer * speed);
            float value = math.lerp(start, end, s);
            _objectMaterial.SetFloat(_highlightValue, value);
        }

        _pickupInteractable.ForceDrop();
        this.transform.position = _cachedPosition.pos;
        transform.rotation = _cachedPosition.rot;
        
        _objectMaterial.SetFloat(_highlightValue, _maxLerpValue);

        _countdownCo = null;
        yield break;
    }
}
