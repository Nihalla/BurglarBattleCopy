using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// <summary>
/// Local search component for the AI System.
///
///
/// 
/// </summary>
public class LocalSearchComponent : AiComponent
{
    LocalSearchComponent() : base(AiComponentType.SEARCH) { }
    
    [SerializeField] private int _pointsToIdle = 5;
    [SerializeField] private float _delayAtIdlePoint = 1.0f;
    
    private BoxCollider _boxTrigger;
    private Vector3 _searchBoxLocation;
    private GameObject _searchBoxObject;
    private Vector3 _guardTarget = Vector3.zero;
    private float _delay = 0;
    private int _count = 0;
    
    /// <summary>
    /// Creates a search box based around given parameters.
    /// The size is evenly spaced either side of the search position, please take this into consideration if using the manual search.
    /// </summary>
    /// <param name="searchPosition">The position of the search box</param>
    /// <param name="size">The size of the search box</param>
    public void CreateManualSearch(Vector3 searchPosition, Vector3 size)
    {
        if (_searchBoxObject is not null)
        {
            Destroy(_searchBoxObject);
        }
        
        _searchBoxLocation = searchPosition;
        
        _searchBoxObject = new GameObject("Temp Guard Search Box");
        _searchBoxObject.transform.position = _searchBoxLocation;


        _boxTrigger = _searchBoxObject.AddComponent<BoxCollider>();
        _boxTrigger.size = size;
        
        _boxTrigger.isTrigger = true;
 
       
    }
    /// <summary>
    /// Creates the largest possible search box near the target point. 
    /// </summary>
    /// <param name="searchPosition">The position to create the search box.</param>
    public void CreateDynSearch(Vector3 searchPosition)
    {
        if (_searchBoxObject is not null)
        {
            Destroy(_searchBoxObject);
        }
        
        int layerMask = 1 << 13;

        RaycastHit FrontHit;
        Physics.Raycast(searchPosition, transform.TransformDirection(Vector3.forward), out FrontHit,
            Mathf.Infinity, layerMask);
        Debug.DrawRay(searchPosition, transform.TransformDirection(Vector3.forward) * FrontHit.distance,
            Color.yellow);

        RaycastHit BackHit;
        Physics.Raycast(searchPosition, transform.TransformDirection(Vector3.back), out BackHit, Mathf.Infinity,
            layerMask);
        Debug.DrawRay(searchPosition, transform.TransformDirection(Vector3.back) * BackHit.distance,
            Color.yellow);

        RaycastHit LeftHit;
        Physics.Raycast(searchPosition, transform.TransformDirection(Vector3.left), out LeftHit, Mathf.Infinity,
            layerMask);
        Debug.DrawRay(searchPosition, transform.TransformDirection(Vector3.left) * LeftHit.distance,
            Color.red);

        RaycastHit RightHit;
        Physics.Raycast(searchPosition, transform.TransformDirection(Vector3.right), out RightHit,
            Mathf.Infinity, layerMask);
        Debug.DrawRay(searchPosition, transform.TransformDirection(Vector3.right) * RightHit.distance,
            Color.red);
        
        
        _searchBoxLocation = new Vector3(((RightHit.point + LeftHit.point )/2).x, searchPosition.y ,((FrontHit.point + BackHit.point )/2).z);
        
        _searchBoxObject = new GameObject("Temp Guard Search Box");
        _searchBoxObject.transform.position = _searchBoxLocation;


        _boxTrigger = _searchBoxObject.AddComponent<BoxCollider>();
        _boxTrigger.size = new Vector3(RightHit.distance + LeftHit.distance, 2, FrontHit.distance + BackHit.distance);
        
        _boxTrigger.isTrigger = true;
 
       
    }
    /// <summary>
    /// Returns the target position for the agent. update safe, will not return another value until the agent has been to the target location. 
    /// </summary>
    /// <param name="guardTransform">The navmeshagent's transform</param>
    /// <param name="searchOver">Has the search concluded.</param>
    /// <returns>Target Positions</returns>
    public Vector3 Search(Transform guardTransform, out bool searchOver)
    {
        searchOver = false;
        if (_searchBoxObject == null)
        {
            Debug.LogError("Fatal Error: Please make a search zone first.");
            return guardTransform.position;
        }
        
        if (_guardTarget == Vector3.zero)
        {
            _count = 0;
            _delay = _delayAtIdlePoint;
            _guardTarget = CreatePosition(guardTransform);
        }
        else
        {
            NavMeshHit currentHit1;
            NavMesh.SamplePosition(guardTransform.position, out currentHit1, 10f, NavMesh.AllAreas);
            NavMeshHit potHit1;
            NavMesh.SamplePosition(_guardTarget, out potHit1, 1f, NavMesh.AllAreas);
            if (currentHit1.position == potHit1.position)
            {
                _delay -= Time.deltaTime;
            }
            
            if (_delay < 0)
            {
                _count++;
                _guardTarget = CreatePosition(guardTransform);
                _delay = _delayAtIdlePoint;
                if (_count >= _pointsToIdle)
                {
                    searchOver = true;
                    _guardTarget = Vector3.zero;
                }
            }

        }

        return _guardTarget;
    }
    private Vector3 CreatePosition(Transform guardTransform)
    {
        Vector3 PointGen = new Vector3(
            Random.Range(_boxTrigger.bounds.min.x, _boxTrigger.bounds.max.x),
            _boxTrigger.bounds.max.y,
            Random.Range(_boxTrigger.bounds.min.z, _boxTrigger.bounds.max.z)
        );
        
        RaycastHit hit;
        if (Physics.Raycast(PointGen, guardTransform.TransformDirection(Vector3.down), out hit))
        {
            Debug.DrawRay(PointGen, guardTransform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            
            NavMeshHit NavHit;
            if (NavMesh.SamplePosition(hit.point, out NavHit, 1.0f, NavMesh.AllAreas))
            {
                return NavHit.position;
            }
            
            return CreatePosition(guardTransform);
        }
        else
        {
            Debug.LogError("Search Area is out of bounds.");
            return CreatePosition(guardTransform);
        }
        
    }
}
