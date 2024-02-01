// Author: Zack Collins

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

/// <summary>
/// Attach this script to an object to be able to apply a simple sin wave bobbing motion to the position of the object
/// </summary>
public class ObjectBobPosition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _objectToBob;
    [SerializeField] private Transform _endPoint;

    [Header("Bobbing Settings")]
    [SerializeField] private float _bobbingDuration = 1f;
    [SerializeField] private bool _startAnimOnStart = false;

    private delegate IEnumerator BobbingDel();
    private BobbingDel InfiniteBobFunc;
    private Coroutine _bobbingCoroutine;

    private void Awake()
    {
        Debug.Assert(_endPoint != null, "EndPoint is null. Please set in the inspector.", this);

        // NOTE(Zack): pre-allocation of delegate, to remove as many allocations at runtime as possible
        InfiniteBobFunc = InfiniteBob;
    }

    private void Start()
    {
        if (!_startAnimOnStart) return;
        StartBobbing();
        _bobbingCoroutine = StartCoroutine(InfiniteBobFunc());
    }

    public void SetObjectToBob(Transform obj)
    {
        _objectToBob = obj;
    }

    public void StartBobbing()
    {
        if (_bobbingCoroutine != null) return;
        _bobbingCoroutine = StartCoroutine(InfiniteBobFunc());
    }

    public void StopBobbing()
    {
        if (_bobbingCoroutine == null) return;
        StopCoroutine(_bobbingCoroutine);
        _bobbingCoroutine = null;
    }

    private IEnumerator InfiniteBob()
    {
        // we lerp to the first location
        float3 start = _objectToBob.localPosition;
        float3 end   = _endPoint.localPosition;

        float timer = 0f;
        while (true)
        {
            float t = math.sin(timer);
            _objectToBob.localPosition = math.lerp(start, end, t);

            timer += Time.deltaTime;
            yield return null;
        }

        yield break;
    }
}
