using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShaderValues : MonoBehaviour
{
    // Author: Wei
    // inspector accessible variables
    [Header("Add Players' Transforms")]
    [SerializeField] public Transform[] targets;
    [Space]
    [Header("Add Objects' Mesh Renderers")]
    [SerializeField] public MeshRenderer[] objects;
    [Space]
    [Header("Current Active LookAt Object Transform")]
    [SerializeField] public Transform currentLookAtter;
    [Header("Grow Transform Settings")]
    [SerializeField] public float appearSpeed = 10f;
    [SerializeField] public float disappearSpeed = 5f;
    [Space]
    [Header("Repair Transform Settings")]
    [SerializeField] public float radius = 12f;
    [SerializeField] public float radiusRandomRange;
    [SerializeField] public bool keep = false;
    [SerializeField] public float minRangeRandomOffset = -3f;
    [SerializeField] public float maxRangeRandomOffset = 3f;

    // private variables
    private float[] _values;
    private float[] _offsets;
    private float[] _radiusRandomRanges;
    private float[] _sqrLens;
    private Vector3[] _newOffset;
    private MaterialPropertyBlock _props;

    // hashed shader lookups
    private static readonly int _playerPosHash    = Shader.PropertyToID("_PlayerPos");
    private static readonly int _movedHash        = Shader.PropertyToID("_Moved");
    private static readonly int _randomOffsetHash = Shader.PropertyToID("_RandomOffset");


    void Start()
    {
        targets = FourPlayerManager.PlayerTransforms;
        
        _props = new MaterialPropertyBlock();
        _values = new float[objects.Length];
        _offsets = new float[objects.Length];
        _radiusRandomRanges = new float[objects.Length];
        _sqrLens = new float[targets.Length];
        _newOffset = new Vector3[targets.Length];      

        SetRandomOffset();
        MeshBounds(); // hack to stop culling because the object is so far from its origin
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        // NOTE(Zack): we're doing this check so that we don't get spammed with errors 
        // when starting in the scene with no players;
        // and it will also be compiled out of release builds;
        if (FourPlayerManager.InstantiatedPlayerCount == 0) return;
#endif

        //Shader.SetGlobalVector("_PlayerPos", initialtarg.transform.position);
        Shader.SetGlobalVector(_playerPosHash, GetClosestPlayer(targets).position);

        for (int i = 0; i < objects.Length; i++)
        {        
            // GET DISTANCE
            _newOffset[0] = objects[i].transform.position - targets[0].position;
            _newOffset[1] = objects[i].transform.position - targets[1].position;
            _newOffset[2] = objects[i].transform.position - targets[2].position;
            _newOffset[3] = objects[i].transform.position - targets[3].position;

            //SQUARE LENGTH
            _sqrLens[0] = _newOffset[0].sqrMagnitude;
            _sqrLens[1] = _newOffset[1].sqrMagnitude;
            _sqrLens[2] = _newOffset[2].sqrMagnitude;
            _sqrLens[3] = _newOffset[3].sqrMagnitude;


            if (_sqrLens[0] < radius * radius + _radiusRandomRanges[i] || _sqrLens[1] < radius * radius + _radiusRandomRanges[i] ||
                _sqrLens[2] < radius * radius + _radiusRandomRanges[i] || _sqrLens[3] < radius * radius + _radiusRandomRanges[i])
            {
                _values[i] = Mathf.Lerp(_values[i], 1, Time.deltaTime * appearSpeed);// set property float to 1 over time
            }
            else if (!keep)
            {
                _values[i] = Mathf.Lerp(_values[i], 0, Time.deltaTime * disappearSpeed);// set property float to 0 over time if keep is not true
            }
            _props.SetFloat(_movedHash, _values[i]);
            _props.SetFloat(_randomOffsetHash, _offsets[i]);
            objects[i].SetPropertyBlock(_props);
        }
    }

    void SetRandomOffset()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            _offsets[i] = Random.Range(minRangeRandomOffset, maxRangeRandomOffset);
            _radiusRandomRanges[i] = Random.Range(-radiusRandomRange, radiusRandomRange);
        }
    }

    void MeshBounds()
    {

        for (int i = 0; i < objects.Length; i++)
        {
            Mesh mesh = objects[i].GetComponent<MeshFilter>().mesh;
            mesh.bounds = new Bounds(Vector3.zero, 100f * Vector3.one);
        }

    }

    Transform GetClosestPlayer(Transform[] players)
    {
        Transform bestTarget = null;
        float closestDistance = float.MaxValue;
        Vector3 currentPosition = currentLookAtter.transform.position;

        // NOTE(Zack): moved over to using the instantiated player count in a raw for loop so that,
        // we don't get a null reference exceptions when trying to access players that don't exist
        for (int i = 0; i < FourPlayerManager.InstantiatedPlayerCount; ++i)
        {
            Transform currentObject = players[i];

            Vector3 differenceToTarget = currentObject.position - currentPosition;
            float distanceToTarget = differenceToTarget.sqrMagnitude;

            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestTarget = currentObject;
            }
        }

        return bestTarget;
    }
}
