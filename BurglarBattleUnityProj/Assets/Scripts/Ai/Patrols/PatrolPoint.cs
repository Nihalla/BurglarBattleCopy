using System;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
///<summary>
/// Patrol Point system for the AI System.
/// 
/// 
/// <h3>Creating Basic Patrol Paths</h3>
/// <ol>
/// <li>Create three empty game objects and attach this script to them.</li>
/// <li>Connect them by setting the next patrol point in each respective inspector.</li>
/// <li>Change the colour of the patrol by changing the colour of the first patrol point.</li>
/// <li>Place a guard and assign their colour to the same as the patrol path, this is case-sensitive.</li>
/// <li>Assign the guard's first patrol point and hit play the guard will now follow the created path.</li>
/// </ol>
/// 
/// <h3>Using Dynamic Link</h3>
/// <ol>
/// <li>Create three empty game objects and attach this script to them.</li>
/// <li>Connect them by setting the next patrol point in each respective inspector, do not set this for the last patrol point, instead enable dynamic link</li>
/// <li>Dynamic link will attempt to connect to the closest PatrolPoint of the same colour, if this fails then it will target the nearest patrol point. It will attempt to do this upon game start.</li>
/// </ol>
///
///
/// <h2> Point Types </h2>
/// A Subtype of PatrolPoint is the PatrolArea, if more types of point are made in the future please add them to the enum and call Init for Dynamic Link compatability.
/// </summary>
public class PatrolPoint : MonoBehaviour
{
    /// <summary>
    /// An Enum used to define all possible types of PatrolPoint.
    /// </summary>
    public enum PatrolPointType
    {
        NO_TYPE = -1,
        POINT = 0,
        AREA = 1,
        COMPLEX = 2,
        SPLIT = 3
    }

    /// <summary>
    /// Stop the patrol point from finding the next patrol point.
    /// </summary>
    [SerializeField] protected bool noNextPatrolPoint = false;
    
    /// <summary>
    /// The Type of the patrol point, used largely in sub-classes.
    /// </summary>
    public PatrolPointType Type { get; protected set; }

    [SerializeField] private PatrolPoint _nextPatrolPoint = null;
    
    /// <summary>
    /// Public getter for the next patrol point. This should not be set in script.
    /// </summary>
    public PatrolPoint nextPatrolPoint { get => _nextPatrolPoint; }
    [Space] 
    [Header("Dynamic Link finds the next patrol point. \nPlease make sure that the next nearest point is the intended target.")]
    [SerializeField] private bool _dynamicLink = false;
    /// <summary>
    /// Each patrol path has a colour determined by its origin point, this colour is case-sensitive. It is not advisable to set this colour during runtime as it will only have an effect on the origin patrol point. 
    /// </summary>
    [Header("Will only take effect on the origin Patrol Point.")]
    [SerializeField] public Color patrolRouteColour = Color.black;

    /// <summary>
    /// Start is kept private as we want our inherited classes to use their own.
    /// </summary>
    private void Start()
    {
        Type = PatrolPointType.POINT;
        Init();
        
    }

    /// <summary>
    /// Init contains continuity checking and the DynamicLink functionality.
    /// Init is used as the start function due to this class being inheritable.
    /// </summary>
    public void Init()
    {
        if (Application.isPlaying && _nextPatrolPoint == null && !_dynamicLink)
        {
            Debug.LogWarning("Patrols are expected to be a closed loop. Attempting to auto resolve with dynamic link, please check this patrol point's configuration.");
            _dynamicLink = true;
        }

        if (Application.isPlaying)
        {
            GuardManager.Register(this);
        }

        if (noNextPatrolPoint)
        {
            return;
        }
        
        if (_dynamicLink && Application.isPlaying)
        {
            PatrolPoint[] potentials = FindObjectsOfType<PatrolPoint>();
            Array.Sort(potentials,
                delegate(PatrolPoint x, PatrolPoint y)
                {
                    return Vector3.Distance(x.gameObject.transform.position, gameObject.transform.position)
                        .CompareTo(Vector3.Distance(y.gameObject.transform.position, gameObject.transform.position));
                });
            foreach (var vPatrolPoint in potentials)
            {
                if ((vPatrolPoint.patrolRouteColour == patrolRouteColour) && (vPatrolPoint != this))
                {
                    _nextPatrolPoint = vPatrolPoint;
                    break;
                }
            }
            
        }
        
        
    }
    
    /// <summary>
    /// Do not use. Potentially unsafe code if used outside of GuardManager context.
    /// </summary>
    public void Relink(PatrolPoint patrolPoint)
    {
        _nextPatrolPoint = patrolPoint;
    }

#if UNITY_EDITOR
    private NavMeshPath _examplePath = null;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        if (_nextPatrolPoint)
        {
            Gizmos.color = patrolRouteColour;
        }

        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    //public void Update()
    //{
    //    try
    //    {
    //        if (_nextPatrolPoint is null) return;

    //        _examplePath = new NavMeshPath();
    //        NavMesh.CalculatePath(transform.position, _nextPatrolPoint.gameObject.transform.position,
    //            NavMesh.AllAreas, _examplePath);

    //        for(int i = 0; i < _examplePath.corners.Length - 1; i++)
    //        {
    //            Debug.DrawLine(_examplePath.corners[i], _examplePath.corners[i + 1], patrolRouteColour);
    //        }

    //        if (_nextPatrolPoint.patrolRouteColour != patrolRouteColour)
    //        {
    //            _nextPatrolPoint.patrolRouteColour = patrolRouteColour;
    //        }
    //    }
    //    catch
    //    {
            
    //    }
    //}
#endif
    
    
}

