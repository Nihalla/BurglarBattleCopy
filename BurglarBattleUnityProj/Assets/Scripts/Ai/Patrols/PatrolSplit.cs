using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PatrolSplit : PatrolPoint
{
    [Space]
    [Header("Split Options")]
    [SerializeField]
    private PatrolPoint _nextPatrolPointOther = null;

    private PatrolPoint _previous = null;
    private bool _set = false;
    private void Start()
    {
        Type = PatrolPointType.SPLIT;
        Init();
    }

    public PatrolPoint SplitLogic()
    {
        if (_set) return _previous;
        _set = true;
        int tempNum = Random.Range(0, 2);
        ////Debug.Log(tempNum);
        if (tempNum >= 1)
        {
            _previous = _nextPatrolPointOther;
            return _nextPatrolPointOther;
        }
        else
        {
            _previous = nextPatrolPoint;
            return nextPatrolPoint;
        }
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
        if (this.nextPatrolPoint)
        {
            Gizmos.color = patrolRouteColour;
        }

        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
   
    
    private void Update()
    {
        try
        {
            if (_nextPatrolPointOther is null) return;

            _examplePath = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, _nextPatrolPointOther.gameObject.transform.position,
                NavMesh.AllAreas, _examplePath);
            for (int i = 0; i < _examplePath.corners.Length - 1; i++)
            {
                Debug.DrawLine(_examplePath.corners[i], _examplePath.corners[i + 1], patrolRouteColour);
            }

            if (_nextPatrolPointOther.patrolRouteColour != patrolRouteColour)
            {
                _nextPatrolPointOther.patrolRouteColour = patrolRouteColour;
            }
        }
        catch
        {
            
        }

        //base.Update();
    }
    
    
#endif
}
