using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovementComponent : AiComponent
{


    NavMeshMovementComponent() : base(AiComponentType.MOVEMENT) { }

    private Vector3 _latestPoint;

    private static readonly Vector3 ERROR_VECTOR = new Vector3(-99999, -99999, -99999);

    /// <summary>
    /// Set a new destination for the Agent
    /// </summary>
    /// <param name="newDestination">Give the Coordinate of the Destination the agent needs to go to</param>
    /// <returns>True if the path was set correctly and was valid, False if the path is not valid and wasn't set</returns>
    public bool SetNewDestination(NavMeshAgent _navMesh, Vector3 newDestination) 
    {

        _latestPoint = IsPathValid(_navMesh,newDestination);
        //_navMesh.destination = newDestination;
        if (_latestPoint != ERROR_VECTOR)
        {
            _navMesh.destination = newDestination;
            return true;
        }
        else
        {
            // //Debug.Log($"There has been an issue with the given Destination point: {newDestination}");
            return false;
        }
    }


    /// <summary>
    /// Checks if the position given is a reachable position in the navMesh
    /// </summary>
    /// <param name="destination">The point in the world</param>
    /// <returns>Returns true if the position is valid on the mesh, False if it isn't in on the mesh and returns Vector(-99999, -99999, -99999)</returns>
    public Vector3 IsPathValid(NavMeshAgent _navMesh, Vector3 destination) 
    {
        NavMeshPath path = new NavMeshPath();
       
        if (_navMesh.CalculatePath(destination, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return destination;
            }
            else
            {
                return ERROR_VECTOR;
            }
        }
        else { return ERROR_VECTOR; } 
    }


    /// <summary>
    /// Checks if the current agent has is on a path
    /// </summary>
    /// <returns>Returns true if a path is active, False if there is no path in this agent</returns>
    public bool IsCurrentlyOnPath(NavMeshAgent _navMesh)
    {
        if (_navMesh.hasPath)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// Stop the agent on its spot
    /// </summary>
    /// <param name="_navMesh"></param>
    /// <param name="saveDeletedPath">True to save the path being delete for later use, false to not save the path</param>
    public void StopAgentPath(NavMeshAgent _navMesh, bool saveDeletedPath) 
    {
        if (saveDeletedPath) 
        {
            _latestPoint = GetDestination(_navMesh);
            _navMesh.isStopped = true; // This was missing from stopping the agent - Norbert
        }
        else { _latestPoint = ERROR_VECTOR; }
 
        _navMesh.ResetPath();
    } 


    /// <summary>
    /// Load the last saved NavMesh Path
    /// </summary>
    public void LoadLastSavedPath(NavMeshAgent _navMesh)
    {
        if (_latestPoint != ERROR_VECTOR) 
        {
            _navMesh.destination = _latestPoint;
            _navMesh.isStopped = false; // This was missing from restarting the agent - Norbert
        }
        else 
        {
            // //Debug.Log($"The latest path was not valid");
        }
    }



    /// <summary>
    /// Returns the destination that the current navMesh agent is going to 
    /// </summary>
    /// <returns>Returns coordinate of destination. If there is no destination it will return Vector(-99999, -99999, -99999)</returns>
    public Vector3 GetDestination(NavMeshAgent _navMesh) 
    {
        if (IsCurrentlyOnPath(_navMesh)) 
        {
            return _navMesh.destination;
        }
        else 
        {
            return ERROR_VECTOR;
        }
    }


    /// <summary>
    /// Returns the distance that is remaining in the current active path
    /// </summary>
    /// <returns>Returns the Remaining distance of the path. If there is no destination it will return -1</returns>
    public float DistanceLeftInPath(NavMeshAgent _navMesh) 
    {
        if (IsCurrentlyOnPath(_navMesh))
        { 
            return _navMesh.remainingDistance;
        }
        else
        {
            return -1;
        }
    }


}
