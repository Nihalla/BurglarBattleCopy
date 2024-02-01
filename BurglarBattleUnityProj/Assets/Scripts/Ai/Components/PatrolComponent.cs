using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

[ExecuteInEditMode]
///<summary>
/// Patrol component for the AI System.
/// </summary>
public class PatrolComponent : AiComponent
{
    PatrolComponent() : base(AiComponentType.PATROL) { }
    
    public PatrolPoint currentPatrolPoint = null;
    [Header("This is case-sensitive with a routes colour.")]
    public Color patrolColour = Color.black;

    public float TriggerDistance = 3;

    public Vector4 complexPosition = Vector4.zero;
    public int complexIndex = -1;
    public float speedCheck = 0;

    private bool _noPatrolCheck = false;
    
    private const int INVALID_PATROL_INDEX = -1;

    /// <summary>
    /// Retruns the current patrol target, based upon the patrol route colour. Will update with the next target position. Will return the position of the closest path if an unassigned colour is used or a path cannot be found.
    /// </summary>
    /// <param name="currentAgent">The agent's gameobject.</param>
    /// <returns>The current patrol target</returns>
    public Vector3 GetTarget(GameObject currentAgent) 
    {
        // BUG(Zack): dependant on the position of guard ai in the scene, it can potentially be random as to the specific patrol point that, is chosen;
        // this means that ai that are supposed to patrol within a specific PatrolArea may end up getting the position of a patrol
        // point that is on the floor below them. a hack has implemented in "PatrolArea" as a temporary fix.
        if (currentPatrolPoint is null)
        {
            currentPatrolPoint = GetNearestPatrolPoint(currentAgent.gameObject.transform.position);
            if (currentPatrolPoint is null)
            {
                if (!_noPatrolCheck)
                {
                    Debug.LogError(
                        "There are no patrols in this scene. Please make some before using the guard system.");
                }

                _noPatrolCheck = true;

                return currentAgent.transform.position;
            }
        }
        
        switch (currentPatrolPoint.Type)
        {
            case PatrolPoint.PatrolPointType.POINT:
                NavMeshHit currentHit;
                NavMesh.SamplePosition(currentAgent.gameObject.transform.position, out currentHit, 10f, NavMesh.AllAreas);
                NavMeshHit potHit;
                NavMesh.SamplePosition(currentPatrolPoint.nextPatrolPoint.gameObject.transform.position, out potHit, 1f, NavMesh.AllAreas);
                if (Vector3.Distance(currentHit.position,potHit.position) <= TriggerDistance )
                {
                    currentPatrolPoint = currentPatrolPoint.nextPatrolPoint;
                }
                return currentPatrolPoint.nextPatrolPoint.gameObject.transform.position;
            case PatrolPoint.PatrolPointType.AREA:
                PatrolArea Area = (PatrolArea)currentPatrolPoint;
                return Area.AreaLogic(this);
            
            case PatrolPoint.PatrolPointType.SPLIT:
                PatrolSplit Split = (PatrolSplit)currentPatrolPoint;
                
                NavMeshHit currentHitB;
                NavMesh.SamplePosition(currentAgent.transform.position, out currentHitB, 10f, NavMesh.AllAreas);
                NavMeshHit potHitB;
                NavMesh.SamplePosition(Split.SplitLogic().transform.position, out potHitB, 1f, NavMesh.AllAreas);
                if (Vector3.Distance(currentHitB.position,potHitB.position) <= TriggerDistance )
                {
                    currentPatrolPoint = Split.SplitLogic();
                    Split.Reset();
                }
                return Split.SplitLogic().gameObject.transform.position;
            case PatrolPoint.PatrolPointType.COMPLEX:
                PatrolComplex Comp = (PatrolComplex)currentPatrolPoint;
                return Comp.CompelxLogic(currentAgent);
            default:
                Debug.LogError("This state should not of happend.");
                break;
        }
        return Vector3.zero;
        
       
    }

    /// <summary>
    /// Returns the nearest patrol point with the priority of closest matching colour, then closest if not matching colour is found.
    /// </summary>
    /// <param name="currentAgentPosition">The position of the current agent.</param>
    /// <returns>The new target patrol point.</returns>
    private PatrolPoint GetNearestPatrolPoint(Vector3 currentAgentPosition)
    {
        int closestIndex = INVALID_PATROL_INDEX;
        int bestIndex = INVALID_PATROL_INDEX;
        float minDist = float.MaxValue;
        for (int i = 0; i < GuardManager.s_patrolPoints.Count; ++i)
        {
            PatrolPoint point = GuardManager.s_patrolPoints[i];
            
            // using [distancesq] as we do not need the improved accuracy of the regular [distance] function
            float dist = math.distancesq(point.transform.position, currentAgentPosition);
            if (dist >= minDist) continue;

            closestIndex = i;
            minDist = dist;

            if (point.patrolRouteColour != patrolColour) continue;
            bestIndex = closestIndex;
        }

        PatrolPoint patrol = null;
        if (bestIndex != INVALID_PATROL_INDEX)
        {
            patrol = GuardManager.s_patrolPoints[bestIndex];
        }
        else
        {
            patrol = GuardManager.s_patrolPoints[closestIndex];
        }

        return patrol;
        
    }
}
