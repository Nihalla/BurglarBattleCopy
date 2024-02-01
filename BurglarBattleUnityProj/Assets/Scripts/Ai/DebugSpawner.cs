using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class DebugSpawner : MonoBehaviour
{
    public bool SpawnSomeGuards = false;
    public bool SpawnPatrolPoint = false;
    public Color patrolColor;

    void Update()
    {
        if(!Application.isPlaying) return;
        
        if (SpawnSomeGuards)
        {
            
            GuardManager.SpawnGuards(5, patrolColor);
            SpawnSomeGuards = false;
        }

        if (SpawnPatrolPoint)
        {
            GuardManager.InsertPatrolPoint(transform.position, patrolColor);
            SpawnPatrolPoint = false;
        }
        
    }
    
    
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = patrolColor;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
   
#endif
}
