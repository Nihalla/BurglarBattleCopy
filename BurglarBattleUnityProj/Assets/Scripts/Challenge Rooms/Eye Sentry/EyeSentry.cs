using System;
using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// Eye Sentry
/// 
/// Class for controlling Eye Sentry Challenge behaviours.
/// </summary>
public class EyeSentry : MonoBehaviour
{
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
    [Space]
    [SerializeField] private Color _openColor;
    [SerializeField] private Color _closedColor;

    [Header("Required References")]
    [SerializeField] private GameObject _pupilObject;
    [SerializeField] private List<Transform> _respawnPoints;

    private DetectionComponent _detectionComponent;
    private Material _material;
    
    private delegate IEnumerator TeleportPayerDel(FirstPersonController controller);
    private TeleportPayerDel _teleportPayerFunc;
    private WaitForSeconds _wait;

    private bool _eyeOpen;
    private bool _stunned;

    private float _eyeTimer = 0f;
    private float _eyeTimerThreshold;

    private List<Transform> _playerTransforms;

    #region DEBUG_UTILITIES
    [ContextMenu("DEBUG: Stun Eye Sentry")]
    private void DebugStunEye()
    {
        Stun();
    }
    #endregion //DEBUG_UTILITIES

    private void Awake()
    {
        _eyeOpen = false;
        _stunned = false;

        _detectionComponent = GetComponentInParent<DetectionComponent>();
        _playerTransforms = new List<Transform>();
        _teleportPayerFunc = TeleportPlayer;
        _wait = new WaitForSeconds(0.5f);
    }

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        _material.color = Color.red;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void Stun()
    {
        _stunned = true;

        _eyeOpen = false;
        _eyeTimer = 0f;
        _eyeTimerThreshold = _stunDuration;

        StopAllCoroutines();

        _material.color = Color.blue;
        _detectionComponent.SetVisionConeColour(Color.green);
    }

    private void Update()
    {
        _eyeTimer += Time.deltaTime;

        if (_eyeTimer >= _eyeTimerThreshold)
        {
            if (_eyeOpen)
            {
                CloseEye();
            }

            else
            {
                StartCoroutine(LerpToColour(Color.red));
                _detectionComponent.LerpVisionConeColour(Color.red, _detectionGracePeriod);


                // Allow a grace period for the player to react before enabling the trigger
                if (_eyeTimer >= _eyeTimerThreshold + _detectionGracePeriod)
                {
                    OpenEye();
                }
            }
        }

        if (_eyeOpen)
        {
            bool playersDetected = _detectionComponent.UpdateDetection(out Span<Transform> playerTransforms, true);

            if (playersDetected)
            {
                _pupilObject.transform.LookAt(playerTransforms[0].position);

                for (int i = 0; i < playerTransforms.Length; ++i)
                {
                    FirstPersonController playerController = playerTransforms[i].gameObject.GetComponent<FirstPersonController>();

                    if (playerController.GetIsVisible())
                    {
                        // Note (Christy): Should find a way to cache rigidbody references instead of getting them every frame
                        if (playerController.rb.velocity.magnitude > 1f)
                        {
                            StartCoroutine(_teleportPayerFunc(playerController));
                            // _playerTransforms.Remove(_playerTransforms[i]);
                        }
                    }
                }
            }
        }
    }

    private void OpenEye()
    {
        _eyeOpen = true;
        _eyeTimer = 0f;
        _eyeTimerThreshold = UnityEngine.Random.Range(_minOpenTime, _maxOpenTime);

        _stunned = false;
    }

    private void CloseEye()
    {
        _eyeOpen = false;
        _eyeTimer = 0f;
        _eyeTimerThreshold = UnityEngine.Random.Range(_minClosedTime, _maxClosedTime);
        StopAllCoroutines();
        StartCoroutine(LerpToColour(Color.green));

        _detectionComponent.LerpVisionConeColour(Color.green, _detectionGracePeriod);
    }

    private IEnumerator TeleportPlayer(FirstPersonController targetPlayer)
    {
        //NOTE(Felix): This makes the player catch fade work for the eye sentry, if you don't want this just remove this line.
        PlayerCatchUI.Catch(targetPlayer.playerID, 0.5f);
        yield return _wait;
        Transform respawnPosition = FindNearestRespawn(targetPlayer);
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

    [BurstCompile]
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
