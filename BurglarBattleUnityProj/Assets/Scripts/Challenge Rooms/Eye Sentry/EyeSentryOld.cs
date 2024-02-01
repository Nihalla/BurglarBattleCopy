using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Eye Sentry
/// 
/// Class for controlling Eye Sentry Challenge behaviours.
/// </summary>
public class EyeSentryOld : MonoBehaviour
{
    [HideInInspector] public bool eyeOpen;

    [Header("Difficulty Tuning")]
    [SerializeField] private float _minOpenTime;
    [SerializeField] private float _maxOpenTime;
    [SerializeField] private float _minClosedTime;
    [SerializeField] private float _maxClosedTime;
    [Space]

    [Tooltip("Specifies a time, in seconds, the player can move without being detected after the eye turns red.")]
    [SerializeField] private float _detectionGracePeriod;
    [Space]

    [Tooltip("Specifies how long, in seconds, the eye should be stunned for when hit with a tool.")]
    [SerializeField] private float _stunDuration;

    [Header("Setup")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _respawnCheckHeight;
    [Space]

    [Header("Required References")]
    [SerializeField] private GameObject _pupilObject;
    [SerializeField] private List<Transform> _respawnPoints;
    [SerializeField] private DetectionComponent _detectionComponent;

    private float _eyeTimer = 0f;
    private float _eyeTimerThreshold;

    private bool _stunned;

    private Material _material;

    private List<FirstPersonController> _players;

    private float _playerColliderRadius;

    [Header("Collider")]
    [SerializeField] private Vector3 _colliderOffset = new Vector3(0, 0, -1);
    private BoxCollider _collider;
    [SerializeField] private LayerMask _environmentLayer;

    private void Awake()
    {
        eyeOpen = false;
        _stunned = false;

        _material = GetComponent<MeshRenderer>().material;
        _material.color = Color.red;

        _players = new List<FirstPersonController>();

        _collider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        ChangeCollider();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Stun(float duration)
    {
        _stunned = true;

        CloseEye();
        _eyeTimerThreshold = duration;
        StartCoroutine(LerpToColour(Color.blue));
    }

    private void Update()
    {
        _eyeTimer += Time.deltaTime;

        if (_eyeTimer >= _eyeTimerThreshold)
        {
            if (eyeOpen)
            {
                CloseEye();
            }

            else
            {
                StartCoroutine(LerpToColour(Color.red));

                // Allow a grace period for the player to react before enabling the trigger
                if (_eyeTimer >= _eyeTimerThreshold + _detectionGracePeriod)
                {
                    OpenEye();
                }
            }
        }

        if (eyeOpen)
        {
            // If the eye is open, check if players inside the trigger volume are moving
            for (int i = 0; i < _players.Count; ++i)
            {
                if (_players[i] == null)
                {
                    continue;
                }

                if (!_players[i].GetIsVisible())
                {
                    continue;
                }

                if(_players[i].rb.velocity.magnitude > 1f)
                {
                    TeleportPlayer(_players[i]);
                    _players.Remove(_players[i].GetComponent<FirstPersonController>());
                }
            }
        }

        if (_players.Count > 0 && !_stunned)
        {
            _pupilObject.transform.LookAt(_players[0].gameObject.transform.position);
        }
    }

    public void ChangeCollider()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + _colliderOffset, GetRaycastDirection(), out hit, Mathf.Infinity, _environmentLayer))
        {
            float distance = hit.distance;
            Vector3 hitPoint = hit.point;
            Vector3 centerPoint = transform.position + _colliderOffset;
            Vector3 newCenter = Vector3.Lerp(centerPoint, hitPoint, 0.5f);

            // Calculate the new center point based on the object's rotation
            Vector3 localCenter = transform.InverseTransformPoint(newCenter);
            localCenter.y = _collider.center.y;
            _collider.center = localCenter;
            _collider.size = new Vector3(_collider.size.x, 4, distance);
        }
    }

    private Vector3 GetRaycastDirection()
    {
        // Calculate the direction of the raycast based on the object's rotation
        Vector3 direction = transform.TransformDirection(-transform.forward);

        if(Mathf.Abs(transform.rotation.eulerAngles.y - 180) < 0.1f)
        {
            ////Debug.Log("180");
            direction = transform.TransformDirection(transform.forward);
        }

        return direction;
    }

    private void OpenEye()
    {
        eyeOpen = true;
        _eyeTimer = 0f;
        _eyeTimerThreshold = Random.Range(_minOpenTime, _maxOpenTime);

        _stunned = false;
    }

    private void CloseEye()
    {
        eyeOpen = false;
        _eyeTimer = 0f;
        _eyeTimerThreshold = Random.Range(_minClosedTime, _maxClosedTime);
        StartCoroutine(LerpToColour(Color.green));
    }

    /// <summary>
    /// Attempts to generate a random respawn position within a radius of
    /// <see cref="_respawnRadius"/> from point <see cref="_respawnPoint"/>
    /// and teleports <paramref name="targetPlayer"/> object to it. 
    /// If a valid respawn cannot be found after <see cref="_maxSpawnAttempts"/>
    /// iterations, the player is simply moved to <see cref="_respawnPoint"/>.
    /// </summary>
    /// <param name="targetPlayer"> The <see cref="FirstPersonController"/> script attached to the player.</param>
    private void TeleportPlayer(FirstPersonController targetPlayer)
    {
        Transform respawnPosition = FindNearestRespawn(targetPlayer);

        // Assign size of player collider if it hasn't already been.
        if (_playerColliderRadius == 0)
        {
            SetPlayerColliderSize(targetPlayer);
        }

        // (NYI): Attempt to spawn player in a random valid location within range of the respawn point.
        // If a valid location cannot be found, spawn them directly on top of the respawn point.

        // TODO: Troubleshoot why this is not working

        /*
        for (int i = 0; i < _maxSpawnAttempts; ++i)
        {
            positionToCheck = _respawnPoint.position + new Vector3
                (Random.Range(-_respawnRadius, _respawnRadius), 0f, Random.Range(-_respawnRadius, _respawnRadius));

            if (Physics.Raycast(positionToCheck, Vector3.down, out RaycastHit hit, _respawnCheckHeight, _groundLayerMask))
            {
                if (hit.collider.gameObject.layer == _groundLayer)
                {
                    if (Physics.OverlapCapsule(_playerColliderBottom, _playerColliderTop, _playerColliderRadius).Length == 0)
                    {
                        respawnPosition = positionToCheck;
                        break;
                    }
                }
            }
        }
        */

        targetPlayer.SetPlayerPosition(respawnPosition);
    }

    private Transform FindNearestRespawn(FirstPersonController targetPlayer)
    {
        Transform player_transform = targetPlayer.gameObject.transform;

        float closest_distance = Mathf.Infinity;
        Transform closest_respawn_point = player_transform;

        int number_of_respawn_points = _respawnPoints.Count;

        if (number_of_respawn_points == 0)
        {
            return player_transform;
            throw new System.Exception("No respawn point set for Eye Sentry. Please set one in the inspector.");
        }

        else if (number_of_respawn_points == 1)
        {
            return _respawnPoints[0];
        }

        else
        {
            for (int i = 0; i < number_of_respawn_points; ++i)
            {
                float distance_to_current = Vector3.Distance(player_transform.position, _respawnPoints[i].position);

                if (distance_to_current < closest_distance)
                {
                    closest_distance = distance_to_current;
                    closest_respawn_point = _respawnPoints[i];
                }
            }
        }

        return closest_respawn_point;
    }

    private void SetPlayerColliderSize(FirstPersonController playerScript)
    {
        _playerColliderRadius = playerScript.GetPlayerColliderRadius();

        float colliderHeight = playerScript.GetPlayerColliderHeight();

        //_playerColliderBottom = new Vector3(0f, _playerColliderRadius, 0f);
        //_playerColliderTop = new Vector3(0f, colliderHeight - _playerColliderRadius, 0f);
    }

    private IEnumerator LerpToColour(Color endColor)
    {
        float elapsed_time = 0f;
        Color start_color = _material.color;

        while (elapsed_time < _detectionGracePeriod / 2)
        {
            _material.color = Color.Lerp(start_color, endColor, elapsed_time);
            elapsed_time += Time.deltaTime;

            yield return null;
        }

        yield return null;
    }
}
