using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;

public class StunTrapTrigger : MonoBehaviour
{
    //REVIEW(Sebadam2010): Potentially add a stun cooldown to prevent the player from being stunned multiple times in a row if they quickly leave and re enter the trap.

    [SerializeField] private bool _enableTrap = true;
    [SerializeField] private LayerMask _playerLayerMask;
    [Tooltip("How long the stun will last (in seconds)")]
    [SerializeField] private float _stunDuration = 2f;
    
    public bool EnableTrap => _enableTrap;
    
    private FirstPersonController _playerCharacterController;
    private float _timer = 0;
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (((_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) || !_enableTrap)
        {
            return;
        }
        // NOTE(Zack): we're now getting the parent of the collider that we have "collided" with because we're now using a Rigidbody on the player, and this changes the dynamics of the collisions
        _playerCharacterController = other.transform.parent.gameObject.GetComponent<FirstPersonController>();
        if (_playerCharacterController == null)
        {
            Debug.LogError($"Failed to get CharacterController on {this}");
            return;
        }

        StartCoroutine(StunPlayer(_playerCharacterController));
    }

    private IEnumerator StunPlayer(FirstPersonController _characterController)
    {
        _characterController.SetPlayerCanMove(false);
        while (_timer < _stunDuration)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        _characterController.SetPlayerCanMove(true);
        _timer = 0f;
    }
}
