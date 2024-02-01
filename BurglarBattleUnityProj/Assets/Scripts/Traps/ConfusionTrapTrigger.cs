using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ConfusionTrapTrigger : MonoBehaviour
{
    //REVIEW(Sebadam2010): Potentially add a cooldown to prevent the player from being confused multiple times in a row if they quickly leave and re enter the trap.

    [SerializeField] private bool _enableTrap = true;
    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private bool _allowMovementInversion = true;
    [SerializeField] private bool _allowCameraInversion = true;
    [Tooltip("How long the confusion will last (in seconds)")]
    [SerializeField] private float _confuseDuration = 5f;
    
    public bool EnableTrap => _enableTrap;

    private InputActionsInputs _playerCharacterInputActionsInputs;
    
    private Coroutine _confuseCoroutine;
    
    private bool _stunningPlayer = false;
    private float _timer = 0;
    
    private void OnTriggerEnter(Collider other)
    {
       if (((_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) || !_enableTrap)
        {
            return;
        }
        
        // NOTE(Zack): we're now getting the parent of the collider that we have "collided" with because we're now using a Rigidbody on the player, and this changes the dynamics of the collisions
        _playerCharacterInputActionsInputs = other.GetComponent<FirstPersonController>().GetInputActionsInputs();
        if (_playerCharacterInputActionsInputs == null)
        {
            Debug.LogError($"Failed to get CharacterActionInputs on {this}");
            return;
        }
        _confuseCoroutine = StartCoroutine(InvertMovement(_playerCharacterInputActionsInputs));
        
    }

    private IEnumerator InvertMovement(InputActionsInputs _playerCharacterInputActions)
    {
        //Ensures player can't undo the confusion by walking back into the trap again.
        if (_stunningPlayer)
        {
            yield break;
        }

        if (_allowMovementInversion)
        {
            // Initially setting move to the opposite of what it was as otherwise player can hold forward and continue moving forward
            _playerCharacterInputActions.move = new Vector2(-_playerCharacterInputActions.move.x, -_playerCharacterInputActions.move.y);
            _playerCharacterInputActions.SetMovementInverseControls(true);
        }

        if (_allowCameraInversion)
        {
            _playerCharacterInputActions.SetLookInverseControls(true);
        }
        
        _stunningPlayer = true;
        
        //Wait for the duration of the confusion
        while (_timer < _confuseDuration)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        if (_allowMovementInversion)
        {
            _playerCharacterInputActions.SetMovementInverseControls(false);
        }
        if (_allowCameraInversion)
        {
            _playerCharacterInputActions.SetLookInverseControls(false);
        }
        _stunningPlayer = false;
        _timer = 0f;
        _confuseCoroutine = null;
    }
    
}
