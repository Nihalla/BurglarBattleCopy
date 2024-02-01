using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    // Author: Wei
    
    [SerializeField] public Transform[] players;
    [SerializeField] public float lerpSpeed = 10.0f; // Adjust this to control the speed of the rotation

    private Transform _currentTarget = null;
    private float _rotationLerpStartTime = 0.0f;
    private float _rotationLerpEndTime = 0.0f;

    private void Start()
    {
        players = FourPlayerManager.PlayerTransforms;
    }

    void Update()
    {
        // Add Desired Activation Method Here

        RotateTowardsTarget();
    }

    void RotateTowardsTarget()
    {
#if UNITY_EDITOR
        // NOTE(Zack): we're doing this check so that we don't get spammed with errors 
        // when starting in the scene with no players;
        // and it will also be compiled out of release builds;
        if (FourPlayerManager.InstantiatedPlayerCount == 0) return;
#endif

        // Get the current closest player
        Transform closestPlayer = GetClosestPlayer(players);

        // Check if the closest player has changed
        if (closestPlayer != _currentTarget)
        {
            _currentTarget = closestPlayer; // Update the current target

            // Start a new rotation lerp
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(_currentTarget.position - transform.position);
            _rotationLerpStartTime = Time.time;
            _rotationLerpEndTime = Time.time + Vector3.Distance(transform.eulerAngles, endRotation.eulerAngles) / lerpSpeed;
        }

        // Calculate the new rotation based on the lerp time
        Quaternion newRotation;

        if (Time.time < _rotationLerpEndTime)
        {
            float t = (Time.time - _rotationLerpStartTime) / (_rotationLerpEndTime - _rotationLerpStartTime);
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = Quaternion.LookRotation(_currentTarget.position - transform.position);
            newRotation = Quaternion.Lerp(startRotation, endRotation, t);
        }
        else
        {
            Quaternion endRotation = Quaternion.LookRotation(_currentTarget.position - transform.position);
            newRotation = endRotation;
        }

        transform.rotation = newRotation;
    }

    Transform GetClosestPlayer(Transform[] players)
    {
        Transform bestTarget = null;
        float closestDistance = float.MaxValue;

        Vector3 currentPosition = transform.position;

        // NOTE(Zack): moved over to using the instantiated player count in a raw for loop so that,
        // we don't get a null reference exceptions when trying to access players that don't exist;
        for (int i = 0; i < FourPlayerManager.InstantiatedPlayerCount; ++i)
        {
            Transform currentObject = players[i];

            Vector3 differenceToTarget = currentObject.position - currentPosition;
            float distanceToTarget = differenceToTarget.sqrMagnitude;

            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestTarget = currentObject;
            }
        }
        return bestTarget;
    }
}


