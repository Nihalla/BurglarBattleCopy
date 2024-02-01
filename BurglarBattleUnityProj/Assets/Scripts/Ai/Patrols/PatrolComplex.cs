using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class PatrolComplex : PatrolPoint
{
    [Space] [Header("Complex Options")] 
    [Header("The Guard will between these points [X,Y,Z,W], relative to the patrol point." +
            "\nWhere W is the speed of the guard.")]
    [SerializeField]
    private List<Vector4> MovementPoints = new List<Vector4>();

    private PatrolPoint _previous = null;
    private bool _set = false;

    private void Start()
    {
        base.Init();
        Type = PatrolPointType.COMPLEX;
        if(!Application.isPlaying) return;
        
    }
    
    
    public Vector3 CompelxLogic(GameObject currentAgent)
    {
        PatrolComponent pc = currentAgent.GetComponent<PatrolComponent>();
        NavMeshAgent nma = currentAgent.GetComponent<NavMeshAgent>();

        if (pc.complexIndex == -1)
        {
            pc.complexIndex = 0;
            pc.speedCheck = nma.speed;
        }
        
        if (pc.complexPosition == Vector4.zero)
        {
            pc.complexPosition = MovementPoints[pc.complexIndex];
        }
        
        NavMeshHit currentHit;
        NavMesh.SamplePosition(currentAgent.transform.position, out currentHit, 10f, NavMesh.AllAreas);
        
        NavMeshHit potHit;
        NavMesh.SamplePosition(transform.TransformPoint(pc.complexPosition), out potHit, 1f, NavMesh.AllAreas);
         
        if (Vector3.Distance(currentHit.position,potHit.position) <= pc.TriggerDistance )
        {
            pc.complexIndex++;
            if (pc.complexIndex + 1 > MovementPoints.Count)
            {
                pc.currentPatrolPoint = nextPatrolPoint;
                pc.complexIndex = -1;
                nma.speed = pc.speedCheck;
                pc.complexPosition = Vector4.zero;
            }
            else
            {
                nma.speed = MovementPoints[pc.complexIndex].w;
                pc.complexPosition = MovementPoints[pc.complexIndex];
            }
        }
        
        return transform.TransformPoint(pc.complexPosition);
    }

    public void Reset()
    {
        _set = false;
    }
    
#if UNITY_EDITOR
    private NavMeshPath _examplePath = null;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (nextPatrolPoint)
        {
            Gizmos.color = patrolRouteColour;
        }
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
        Gizmos.color = Color.white;
        foreach (var pnt in MovementPoints)
        {
            Gizmos.DrawCube(transform.TransformPoint(new Vector3(pnt.x,pnt.y,pnt.z)), new Vector3(0.5f, 0.5f, 0.5f));
        }
        
    }
    public void Update()
    {
        
        
        
        var oldpnt = transform.InverseTransformPoint(transform.position);
        foreach (var pnt in MovementPoints)
        {
            _examplePath = new NavMeshPath();
            NavMesh.CalculatePath(transform.TransformPoint(oldpnt), transform.TransformPoint(pnt),
                NavMesh.AllAreas, _examplePath);
            for (int i = 0; i < _examplePath.corners.Length - 1; i++)
            {
                Debug.DrawLine(_examplePath.corners[i], _examplePath.corners[i + 1], Color.white);
            }
            
            oldpnt = pnt;
        }

        if (nextPatrolPoint)
        {
            Debug.DrawLine(transform.TransformPoint(oldpnt), nextPatrolPoint.transform.position, patrolRouteColour);
        }
        
    }
#endif
}
