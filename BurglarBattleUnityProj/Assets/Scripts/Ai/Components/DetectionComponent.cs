using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlayerControllers;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

//BUG(Felix): Currently, if a targets origin is behind an obstacle but part of the target is visible, the target will be counted as not visible.
/// <summary>
/// A component of the Ai system that allows for detection of players with a radius and angle. Also, handles the vision cone projected onto the floor.
/// </summary>
[ExecuteInEditMode]
public class DetectionComponent : AiComponent
{
    DetectionComponent() : base(AiComponentType.DETECTION) { }

    public float DetectionRadius    { get => _detectionRadius; }
    public float MinDetectionRadius { get => _minDetectionRadius; }
    public float ViewAngle          { get => _viewAngle; }

    [Header("Detection")]
    [SerializeField] private float _detectionRadius = 1;
    [SerializeField] private float     _minDetectionRadius = 0.1f;
    [SerializeField] private float     _viewAngle          = 45;
    [SerializeField] private LayerMask _targetLayerMask;
    [SerializeField] private LayerMask _enviromentLayerMask;
    [SerializeField] private LayerMask _floorLayerMask;

    [Header("Vision Cone")]
    [SerializeField] private Material _aiDetectionConeProjector;
    [SerializeField] private MeshRenderer _coneMeshRenderer;
    [SerializeField] private MeshFilter   _coneMeshFilter;
    [SerializeField] private int          _meshResolution;
    [SerializeField] private int          _edgeSearchDepth;
    [SerializeField] private Vector3      _targetGroundNormal      = Vector3.up;
    [SerializeField] private float        _visionConeHeightPadding = 0.02f;
    [SerializeField] private Color        _disabledColour          = Color.grey;

    private Material _mat;
    private Mesh     _mesh;
    private int      _tintID                 = -1;
    private int      _opacityID              = -1;
    private int      _allowOpacityOverrideID = -1;
    private float    _originToFloor;
    private float    _currentDisabledForSeconds = -1;

    private delegate IEnumerator ColourLerpDel(Color colour, float time);

    private delegate IEnumerator DisableForDel(float seconds);

    private ColourLerpDel ColourLerpFunc;
    private Coroutine     _colourLerpCo;
    private DisableForDel DisableForFunc;
    private Coroutine     _disableForCo;

    private Vector3    _lastPosition;
    private Quaternion _lastRotation;

    private bool _generateVisionCone;
    private bool _isDisabled;

    private Transform[] _playerTransforms = new Transform[4];

    private Vector3[] _vertices;
    private int[]     _triangles;
    private int       _lastVertCount = 0;

    #if UNITY_EDITOR
    [Header("Debug and Custom editor Lists, will disappear in a build")]
    public bool __Disable;
    public float           __ForSeconds    = 2.0f;
    public bool            __targetInView  = false;
    public Transform[] __visibles;
    public List<Vector3>   __MeshBlockers  = new List<Vector3>();
    public List<Vector3>   __MeshBlockers2 = new List<Vector3>();
    #endif
    
    //TODO(Felix): Add check for stunned layer
    //TODO(Felix): Add UVs to mesh
    //BUG(Felix): Fix incorrect Y removal for mesh generation. FIXED

    private void Awake()
    {
        Debug.Assert(_aiDetectionConeProjector != null, $"Ai Detection requires Projector material ({gameObject.name})");
        Debug.Assert(_coneMeshRenderer != null, $"Ai Detection requires a mesh renderer ({gameObject.name})");
        Debug.Assert(_coneMeshFilter != null, $"Ai Detection requires a mesh filter ({gameObject.name})");

        _coneMeshFilter.sharedMesh = new Mesh();

        _mat                       = new Material(_aiDetectionConeProjector);
        _coneMeshRenderer.material = _mat;
        _mesh                      = new Mesh();
        _vertices                  = new Vector3[(_meshResolution * 4) + 2];
        _triangles                 = new int[((_meshResolution * 4) + 2) * 3];

        Shader shader = _mat.shader;
        _tintID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Tint"));
        _opacityID = shader.GetPropertyNameId(shader.FindPropertyIndex("_Opacity"));
        _allowOpacityOverrideID = shader.GetPropertyNameId(shader.FindPropertyIndex("_AllowOpacityOverride"));
        ColourLerpFunc = LerpToColour;
        DisableForFunc = DisableForSecs;
    }


    /// <summary>
    /// Returns whether the Ai can see a Target, sets a list of Transforms that are visible to the Ai. Sorts the list in ascending order of distance from the Ai.
    /// </summary>
    /// <param name="targets">The list of visible Transforms</param>
    /// <param name="sort">[DEFAULTS = True] Whether or not to sort the list.</param>
    /// <returns>Whether the Ai can see a Target</returns>
    public bool UpdateDetection(out Span<Transform> targets, bool sort = true)
    {
        Transform cachedTransform = this.transform;

        if (cachedTransform.position != _lastPosition || cachedTransform.rotation != _lastRotation)
        {
            _lastPosition = cachedTransform.position;
            _lastRotation = cachedTransform.rotation;
            GenerateVisionConeMesh();
        }

        if (_isDisabled)
        {
            targets = new Span<Transform>(_playerTransforms, 0, 0);
            return false;
        }

        // NOTE(Zack): this sphere overlap has replaced the "Physics.SphereOverlap" so that we do not allocate any memory,
        // and also to increase the speed of computation for the check.
        int playersWithinSphere = 0;
        Span<int> playerIndexes = stackalloc int[4];
        for (int i = 0; i < FourPlayerManager.InstantiatedPlayerCount; ++i)
        {
            if (FourPlayerManager.FPSControllers[i].GetCaught()) continue;

            float3 pos = FourPlayerManager.PlayerTransforms[i].position;
            if (!WithinSphereSqrd(pos, cachedTransform.position, _detectionRadius)) continue;

            playerIndexes[playersWithinSphere] = i;
            playersWithinSphere += 1;
        }

        Vector3 tranPos = new Vector3(cachedTransform.position.x, 0, cachedTransform.position.z);

        int playersDetected = 0;
        if (playersWithinSphere > 0)
        {
            float viewCone = _viewAngle * 0.5f;

            RaycastHit hit;
            for (int i = 0; i < playersWithinSphere; ++i)
            {
                int playerIndex = playerIndexes[i];
                float3 playerPos = FourPlayerManager.PlayerTransforms[playerIndex].position;
                Vector3 pos = new Vector3(playerPos.x, 0, playerPos.z);

                Vector3 diff  = pos - tranPos;
                float   angle = Vector3.Angle(diff, transform.TransformDirection(Vector3.forward));
                // //Debug.Log($"Angle is: {angle}");
                if (angle <= viewCone || diff.magnitude <= _minDetectionRadius)
                {
                    Vector3 rayDiff = (Vector3)playerPos - cachedTransform.position;

                    // Debug.DrawRay(transform.position, rayDiff.normalized * _detectionRadius, Color.yellow);
                    Ray ray = new Ray(cachedTransform.position, rayDiff);
                    if (Physics.Raycast(ray, out hit, _detectionRadius, _enviromentLayerMask | _targetLayerMask))
                    {
                        // NOTE(Zack): removed Log() function as we can do the same functionality with a few bitwise operations
                        if ((_targetLayerMask & (1 << hit.transform.gameObject.layer)) != 0)
                        {
                            // Check if the player is protected via shield.
                            if (!GuardManager.PlayerIgnored(hit.transform.gameObject) && !GuardManager.IsPLayerShielded(hit.transform.gameObject))
                            {
                                _playerTransforms[playersDetected] = FourPlayerManager.PlayerTransforms[playerIndex];
                                playersDetected += 1;
                            }
                        }
                    }
                }
            }

            if (sort)
            {
                // NOTE(Zack): we're sorting in ascending order of distance from this gameobject, e.g. 1, 2, 3, 4... ->
                for (int i = 0; i < playersDetected; ++i)
                {
                    for (int j = i + 1; j < playersDetected; ++i)
                    {
                        float disti = math.distancesq(_playerTransforms[i].position, cachedTransform.position);
                        float distj = math.distancesq(_playerTransforms[j].position, cachedTransform.position);
                        if (disti <= distj) continue;

                        // swap the data
                        Transform temp = _playerTransforms[i];
                        _playerTransforms[j] = _playerTransforms[i];
                        _playerTransforms[i] = temp;
                    }
                }
            }
        }

        targets = new Span<Transform>(_playerTransforms, 0, playersDetected);
        return playersDetected > 0;
    }

    /// <summary>
    /// Disables the Detection component. If no time is passed the component is disabled until explicitly enabled with Enable(), otherwise it disables for the amount of time given.
    /// If the component is already disabled, the current total disable time is compared to the given time and if long the disable time is extended.
    /// </summary>
    /// <param name="seconds">[DEFAULTS = 0] The time to disable for.</param>
    /// <returns>
    /// -1 if already disabled and given time is shorter than the current total disable time.
    /// 0 if the disable time was extended to the new time.
    /// 1 if wasn't disabled and now is disabled.
    /// </returns>
    public int Disable(float seconds = 0)
    {
        //TODO(Felix): Return val could just be enum...
        SetOpacityOverride(true);
        if (seconds <= float.Epsilon)
        {
            if (_isDisabled)
            {
                return -1;
            }

            _isDisabled = true;
            LerpVisionConeColour(_disabledColour, 0.25f);
            return 1;
        }

        if (_isDisabled)
        {
            if (_currentDisabledForSeconds >= seconds)
            {
                return -1;
            }

            StopCoroutine(_disableForCo);
            _isDisabled = true;
            LerpVisionConeColour(_disabledColour, 0.25f);
            _disableForCo = StartCoroutine(DisableForFunc(seconds));
            return 0;
        }

        _isDisabled = true;
        LerpVisionConeColour(_disabledColour, 0.25f);
        _disableForCo = StartCoroutine(DisableForFunc(seconds));
        return 1;
    }

    /// <summary>
    /// Enables the detection component.
    /// </summary>
    /// <returns>
    /// -1 if already enabled,
    /// 1 if disabled and was enabled.
    /// </returns>
    public int Enable()
    {
        if (!_isDisabled)
        {
            return -1;
        }

        _isDisabled = false;
        SetOpacityOverride(false);
        LerpVisionConeColour(Color.red, 0.25f);
        return 1;
    }

    private IEnumerator DisableForSecs(float seconds)
    {
        _currentDisabledForSeconds = seconds;
        yield return new WaitForSeconds(seconds);
        _currentDisabledForSeconds = -1;
        Enable();
    }

    /// <summary>
    /// Returns the direction from the Ai agent at the given angle
    /// </summary>
    /// <param name="angle">The angle to convert to direction</param>
    /// <param name="global">[DEFAULTS = False] If the angle given is in world space</param>
    /// <returns>Vector3: The direction (not normalised)</returns>
    public Vector3 DirectionFromAiAngle(float angle, bool global = false)
    {
        if (!global)
        {
            angle += transform.eulerAngles.y; //transform angle to be local to Ai
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    /// <summary>
    /// Changes the colour of the vision cone instantly.
    /// </summary>
    /// <param name="colour">The Colour to change to.</param>
    public void SetVisionConeColour(Color colour) { _mat.SetColor(_tintID, colour); }

    /// <summary>
    /// Gets the current colour of the vision cone.
    /// </summary>
    /// <returns>The current colour of the cone.</returns>
    public Color GetVisionConeColour() { return _mat.GetColor(_tintID); }
    
    private void SetOpacityOverride(bool value) {_mat.SetFloat(_allowOpacityOverrideID, value ? 1.0f : 0.0f);}

    private float GetOpacity() { return _mat.GetFloat(_opacityID);}

    /// <summary>
    /// Interpolates the colour of the vision cone from its current to the given colour over the given amount of time.
    /// </summary>
    /// <param name="colour"></param>
    /// <param name="time"></param>
    public void LerpVisionConeColour(Color colour, float time)
    {
        if (_colourLerpCo != null)
        {
            StopCoroutine(_colourLerpCo);
        }

        if (_mat.GetFloat(_allowOpacityOverrideID) <= float.Epsilon)
        {
            colour = new Color(colour.r, colour.g, colour.b, _mat.GetFloat(_opacityID));
        }
        _colourLerpCo = StartCoroutine(ColourLerpFunc(colour, time));
    }

    private IEnumerator LerpToColour(Color colour, float time)
    {
        float t           = 0;
        Color startColour = _mat.GetColor(_tintID);
        while (t <= time)
        {
            _mat.SetColor(_tintID, Color.Lerp(startColour, colour, Cubic(t / time)));
            t += Time.deltaTime;
            yield return null; //wait for update
        }
        
        _mat.SetColor(_tintID, colour);
        _colourLerpCo = null;
    }

    private static float Cubic(float val)
    {
        if ((val *= 2f) < 1f) return 0.5f * val * val * val;
        return 0.5f * ((val -= 2f) * val * val + 2f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool WithinSphereSqrd(float3 point, float3 center, float radius)
    {
        float distance = math.distancesq(point, center);
        radius = radius * radius;
        return math.abs(distance) <= radius;
    }

    private void GenerateVisionConeMesh()
    {
        //calculate angles
        float angleStep  = _viewAngle / _meshResolution;
        float startAngle = -((_viewAngle * 0.5f));

        for (int i = 0; i < _lastVertCount; i++) //reset everything
        {
            _vertices[i]      = Vector3.zero;
            _triangles[i]     = 0;
            _triangles[i + 1] = 0;
            _triangles[i + 2] = 0;
        }

        int        currentVertCount = 1;
        RaycastHit hit;
        /*__MeshBlockers.Clear();
        __MeshBlockers2.Clear();*/

        bool    hitMeshLast          = false;
        Vector3 lastMeshBlockerPoint = Vector3.zero;

        for (int i = 1; i < _meshResolution + 1; i++)
        {
            Vector3 dirG = DirectionFromAiAngle(startAngle + (angleStep * i - 1), true).normalized; //global dir

            Vector3 meshPoint = dirG * _detectionRadius;
            Vector3 rayDir    = transform.TransformDirection(meshPoint) - new Vector3(0, _originToFloor, 0);

            // Debug.DrawRay(transform.position, rayDir);
            //Fire ray to every mesh edge
            Ray ray = new Ray(transform.position, rayDir);
            if (Physics.Raycast(ray, out hit, _detectionRadius, _enviromentLayerMask))
            {
                Vector3 meshBlockerPoint = lastMeshBlockerPoint = transform.InverseTransformPoint(hit.point); //transform world to local
                // __MeshBlockers2.Add(transform.TransformPoint(meshBlockerPoint));
                if (hitMeshLast || i == 0)
                {
                    //If we have already hit something or its our first mesh point just add the hit point
                    _vertices[currentVertCount] = new Vector3(meshBlockerPoint.x, 0, meshBlockerPoint.z); //remove the y axis       
                }
                else
                {
                    //We need to find the edge
                    Vector3 nearPoint = transform.TransformDirection(meshBlockerPoint) /*- new Vector3(0, transform.position.y + meshBlockerPoint.y, 0)*/;
                    // Debug.DrawRay(transform.position, nearPoint, Color.magenta);
                    Vector3 farPoint = transform.TransformDirection(_vertices[currentVertCount - 1]) - new Vector3(0, transform.position.y, 0);
                    // Debug.DrawRay(transform.position, farPoint, Color.cyan);
                    FindMeshEdge(nearPoint, farPoint, ref currentVertCount);
                    _vertices[currentVertCount] = new Vector3(meshBlockerPoint.x, 0, meshBlockerPoint.z); //remove the y axis
                }

                hitMeshLast = true;
            }
            else
            {
                if (hitMeshLast)
                {
                    //We need to find the edge
                    Vector3 farPoint = transform.TransformDirection(meshPoint) - new Vector3(0, transform.position.y, 0);
                    // Debug.DrawRay(transform.position, farPoint);
                    Vector3 nearPoint = transform.TransformDirection(_vertices[currentVertCount - 1]) + new Vector3(0, lastMeshBlockerPoint.y, 0);
                    // Debug.DrawRay(transform.position, nearPoint);
                    FindMeshEdge(nearPoint, farPoint, ref currentVertCount, true);
                    _vertices[currentVertCount] = new Vector3(meshPoint.x, 0, meshPoint.z);
                }
                else
                {
                    _vertices[currentVertCount] = new Vector3(meshPoint.x, 0, meshPoint.z);
                }

                hitMeshLast = false;
            }

            currentVertCount++;
        }

        int count = 0;
        for (int i = 0; i < currentVertCount * 3; i += 3) //Create triangles assuming vertices are in correct order, this avoids doing normal calculations
        {
            _triangles[i]     = 0;
            _triangles[i + 1] = count + 1;
            _triangles[i + 2] = count + 2;
            count++;
        }

        _lastVertCount = currentVertCount;

        //Apply mesh
        _coneMeshFilter.sharedMesh.SetVertices(_vertices);
        _coneMeshFilter.sharedMesh.SetTriangles(_triangles, 0);

        _mesh.RecalculateNormals();
    }

    private void FindMeshEdge(Vector3 nearPoint, Vector3 farPoint, ref int vertCount, bool flip = false)
    {
        //Debug elements left in, in case i gotta debug it again soon.
        //TODO(Felix): Tidy up.
        Vector3 nearN = nearPoint.normalized;
        Vector3 farN  = farPoint.normalized;

        // Debug.DrawRay(transform.position, nearN * _detectionRadius, Color.green);
        // Debug.DrawRay(transform.position, farN * _detectionRadius, Color.red);

        // __MeshBlockers2.Add(nearN);
        // __MeshBlockers2.Add(farN);   
        RaycastHit hit;
        float      nearDist = 0;
        // return;
        //Binary search for the edge of the ray blocker
        for (int i = 0; i < _edgeSearchDepth; i++)
        {
            Vector3 halfN = new Vector3(nearN.x, 0, nearN.z) - ((new Vector3(nearN.x, 0, nearN.z) - new Vector3(farN.x, 0, farN.z)) / 2);
            Ray     ray   = new Ray(transform.position, halfN);
            // Debug.DrawRay(transform.position, halfN * _detectionRadius, Color.magenta);
            // continue;
            if (Physics.Raycast(ray, out hit, _detectionRadius, _enviromentLayerMask))
            {
                nearDist = (hit.point - new Vector3(transform.position.x, 0, transform.position.z)).magnitude;
                nearN    = (hit.point - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
                // Debug.DrawRay(transform.position, nearN  * nearDist, Color.blue);
            }
            else
            {
                // __MeshBlockers.Add(halfN * _detectionRadius);
                farN = halfN;
                // Debug.DrawRay(transform.position, farN * _detectionRadius, Color.green); 
            }
        }

        //Use the last mesh hit point and an extrapolated version for the near and far points
        Vector3 nearMeshPoint = new Vector3(nearN.x, 0, nearN.z);
        Vector3 farMeshPoint  = new Vector3(nearN.x, 0, nearN.z).normalized;

        // Debug.DrawRay(transform.position, (nearMeshPoint), Color.magenta);
        // Debug.DrawRay(transform.position, (farMeshPoint * _detectionRadius) /*- new Vector3(0, transform.position.y, 0)*/, Color.cyan);

        Vector3 nearMeshPoint_w = transform.position + ((nearMeshPoint * nearDist));
        Vector3 farMeshPoint_w  = transform.position + ((farMeshPoint * _detectionRadius));

        if (flip) //Apply to mesh, flip if we are going from hitting something to not so that triangles are correct
        {
            // __MeshBlockers2.Add(nearMeshPoint_w - new Vector3(0, transform.position.y, 0));
            // __MeshBlockers2.Add(farMeshPoint_w - new Vector3(0, transform.position.y, 0));

            _vertices[vertCount] = transform.InverseTransformPoint(nearMeshPoint_w);
            vertCount++;
            _vertices[vertCount] = transform.InverseTransformPoint(farMeshPoint_w);
            vertCount++;
        }
        else
        {
            // __MeshBlockers2.Add(farMeshPoint_w - new Vector3(0, transform.position.y, 0));
            // __MeshBlockers2.Add(nearMeshPoint_w - new Vector3(0, transform.position.y, 0));

            _vertices[vertCount] = transform.InverseTransformPoint(farMeshPoint_w);
            vertCount++;
            _vertices[vertCount] = transform.InverseTransformPoint(nearMeshPoint_w);
            vertCount++;
        }
    }

    #if UNITY_EDITOR
    private void __EditorOnly()
    {
        // NOTE(Zack): these if statements ensures that we don't get null references exceptions in the editor
        if (_vertices == null || _vertices.Length <= 0)
        {
            _vertices = new Vector3[(_meshResolution * 4) + 2];
        }

        if (_triangles == null || _triangles.Length <= 0)
        {
            _triangles = new int[((_meshResolution * 4) + 2) * 3];
        }

        if (_mesh == null)
        {
            _mesh = new Mesh();
        }
        
        if (_coneMeshFilter.sharedMesh == null)
        {
            _coneMeshFilter.sharedMesh = new Mesh();
        }


        GenerateVisionConeMesh();
        if (_coneMeshRenderer != null)
        {
            RaycastHit hit;
            Ray          ray  = new Ray(transform.position, Vector3.down);
            
            if (Physics.Raycast(transform.position, Vector3.down, out hit, _detectionRadius, _floorLayerMask))
            {
                
                    // float d = Vector3.Dot(hit.normal, _targetGroundNormal);
                    // if (d >= 0.5f)
                    // {
                        Transform trans = _coneMeshRenderer.transform;
                        trans.position = hit.point + (_targetGroundNormal * _visionConeHeightPadding);
                        trans.rotation = Quaternion.Euler(hit.normal.x, trans.rotation.eulerAngles.y, hit.normal.z);
                        _originToFloor = transform.position.y - hit.point.y;
                        // Debug.DrawLine(transform.position, hit.point);
                
                    // }
            }
        }
    }

    private void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            // code to run only in edit mode
            bool last = __targetInView;
            __targetInView = UpdateDetection(out Span<Transform> targets);
            __visibles     = targets.ToArray();
            __EditorOnly();
        }
        else
        {
            if (__Disable)
            {
                __Disable = false;
                Disable(__ForSeconds);
            }
        }
    }
    #endif

    #region TransformDistanceComparer

    private class TransformDistanceComparer : IComparer<Transform>
    {
        private Vector3 point;

        public TransformDistanceComparer(Vector3 point) { this.point = point; }

        public int Compare(Transform x, Transform y)
        {
            float distanceX = Vector3.Distance(x.position, point);
            float distanceY = Vector3.Distance(y.position, point);
            return distanceX.CompareTo(distanceY);
        }
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(DetectionComponent))]
public class DetectionComponentEditor : Editor
{
    private void OnSceneGUI()
    {
        // return;
        DetectionComponent comp = (target as DetectionComponent);
        Handles.color = Color.white * new Color(1, 1, 1, 0.5f);
        Handles.DrawWireArc(comp.transform.position, Vector3.up, Vector3.forward, 360, comp.MinDetectionRadius);
        Handles.DrawWireArc(comp.transform.position, Vector3.up, Vector3.forward, 360, comp.DetectionRadius);
        Handles.DrawWireArc(comp.transform.position, Vector3.forward, Vector3.left, 360, comp.DetectionRadius);
        Handles.color = comp.__targetInView ? Color.green * new Color(1, 1, 1, 0.1f) : Color.red * new Color(1, 1, 1, 0.1f);
        Vector3 xzDir = comp.DirectionFromAiAngle(-(comp.ViewAngle * 0.5f));
        Handles.DrawSolidArc(comp.transform.position, Vector3.up, xzDir, comp.ViewAngle, comp.DetectionRadius);
        // Vector3 zyDir = comp.DirectionFromAiXAngle(-(comp.ViewAngle * 0.5f));
        // Handles.DrawSolidArc(comp.transform.position, Vector3.right, zyDir, comp.ViewAngle, comp.DetectionRadius);
        if (comp.__targetInView)
        {
            Handles.color = Color.blue;
            foreach (Transform transform in comp.__visibles)
            {
                //(Error) This is causing alot of errors for some reason.
                Handles.DrawDottedLine(transform.position, comp.transform.position, 2);
            }
        }

        foreach (var blocker in comp.__MeshBlockers)
        {
            Handles.DrawWireCube(blocker, Vector3.one * 0.1f);
        }

        Handles.color = Color.red;
        foreach (Vector3 blocker in comp.__MeshBlockers2)
        {
            Handles.DrawWireCube(blocker, Vector3.one * 0.1f);
        }
    }
}
#endif // UNITY_EDITOR
