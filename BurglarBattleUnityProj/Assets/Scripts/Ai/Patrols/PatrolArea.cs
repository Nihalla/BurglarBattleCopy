using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Random = UnityEngine.Random;

///<summary>
/// Patrol Area system for the AI System.
/// 
/// 
/// Use this as you would a PatrolPoint, an attached box collider will act as a region for finding random points to idle in.
///
/// The Inspector contains the number of points to idle at and the delay to wait at each point.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class PatrolArea : PatrolPoint
{
    // HACK(Zack): see Start() for more details
    [Header("Scene References")]
    [SerializeField] private PatrolComponent[] _guardsInPatrolArea;

    [Header("Area Settings")]
    [SerializeField] private BoxCollider _boxTrigger;
    [SerializeField] private int _pointsToIdle = 5;

    [SerializeField] private float _delayAtIdlePoint = 1.0f;
    
    // NOTE(Zack): dictionaries cannot be serialized.
    /* [SerializeField] */ private Dictionary<PatrolComponent, float> _subscibedGuardsDelay = new Dictionary<PatrolComponent, float>();
    /* [SerializeField] */ private Dictionary<PatrolComponent, int> _subscibedGuardsCount = new Dictionary<PatrolComponent, int>();
    /* [SerializeField] */ private Dictionary<PatrolComponent, Vector3> _subscibedGuardsPosition = new Dictionary<PatrolComponent, Vector3>();

    private void Start()
    {
        base.Init();
        Type = PatrolPointType.AREA;
        if(!Application.isPlaying) return;
        _boxTrigger.isTrigger = true;

        // HACK(Zack): this is to enforce the guards in the patrol area to stay within the patrol area. as currently they have
        // a semi random chance to choose a different patrol point based on their positioning in the map
        for (int i = 0; i < _guardsInPatrolArea.Length; ++i)
        {
            _guardsInPatrolArea[i].currentPatrolPoint = this;
        }
    }

    /// <summary>
    /// AreaLogic calculates the next position to move to, and handles the movement delay.
    /// Due to a bug in the PatrolComponent it also handles moving to the next patrol point, this should not effect usage.
    /// </summary>
    /// <param name="pc">The PatrolComponent of calling ai.</param>
    /// <returns>The Vector3 location to move to.</returns>
    public Vector3 AreaLogic(PatrolComponent pc)
    {
        if (!_subscibedGuardsCount.ContainsKey(pc))
        {
            _subscibedGuardsCount[pc] = 0;
            _subscibedGuardsDelay[pc] = _delayAtIdlePoint;
            _subscibedGuardsPosition[pc] = CreatePosition();
        }
        else
        {
            NavMeshHit currentHit1;
            NavMesh.SamplePosition(pc.gameObject.transform.position, out currentHit1, 10f, NavMesh.AllAreas);
            NavMeshHit potHit1;
            NavMesh.SamplePosition(_subscibedGuardsPosition[pc], out potHit1, 1f, NavMesh.AllAreas);

            // NOTE(Zack): we're using this function as the standard "==" operator in Unity has too high of a precision to be useful
            if (Vec3Compare(currentHit1.position, potHit1.position))
            {
                _subscibedGuardsDelay[pc] -= Time.deltaTime;
            }
            
            if (_subscibedGuardsDelay[pc] < 0)
            {
                _subscibedGuardsCount[pc]++;
                _subscibedGuardsPosition[pc] = CreatePosition();
                _subscibedGuardsDelay[pc] = _delayAtIdlePoint;
                if (_subscibedGuardsCount[pc] >= _pointsToIdle)
                {
                    _subscibedGuardsPosition[pc] = nextPatrolPoint.transform.position;
                    
                    NavMeshHit currentHit;
                    NavMesh.SamplePosition(pc.gameObject.transform.position, out currentHit, 10f, NavMesh.AllAreas);
                    NavMeshHit potHit;
                    NavMesh.SamplePosition(_subscibedGuardsPosition[pc], out potHit, 1f, NavMesh.AllAreas);

                    // NOTE(Zack): we're using this function as the standard "==" operator in Unity has too high of a precision to be useful
                    if (Vec3Compare(currentHit.position, potHit.position)) {
                        pc.currentPatrolPoint = nextPatrolPoint;
                        _subscibedGuardsCount[pc] = 0;
                        _subscibedGuardsDelay[pc] = _delayAtIdlePoint;
                        _subscibedGuardsPosition[pc] = CreatePosition();
                    }
                }
            }
        }

        return _subscibedGuardsPosition[pc];
    }

    private Vector3 CreatePosition()
    {
        Vector3 PointGen = new Vector3(
            Random.Range(_boxTrigger.bounds.min.x, _boxTrigger.bounds.max.x),
            _boxTrigger.bounds.max.y,
            Random.Range(_boxTrigger.bounds.min.z, _boxTrigger.bounds.max.z)
        );
        
        RaycastHit hit;
        if (Physics.Raycast(PointGen, transform.TransformDirection(Vector3.down), out hit))
        {
            Debug.DrawRay(PointGen, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            
            NavMeshHit NavHit;
            if (NavMesh.SamplePosition(hit.point, out NavHit, 1.0f, NavMesh.AllAreas))
            {
                return NavHit.position;
            }
            
            return CreatePosition();
        }
        else
        {
            Debug.LogError("Patrol Area is out of bounds.");
            return CreatePosition();
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Vec3Compare(float3 a, float3 b, float epsilon = 0.1f)
    {
        bool x = math.abs(a.x - b.x) < epsilon;
        bool y = math.abs(a.y - b.y) < epsilon;
        bool z = math.abs(a.z - b.z) < epsilon;
        return x && y && z;
    }
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    { 
        Gizmos.color = patrolRouteColour;
        Gizmos.DrawWireCube(_boxTrigger.bounds.center, _boxTrigger.bounds.size);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
#endif
    
}
