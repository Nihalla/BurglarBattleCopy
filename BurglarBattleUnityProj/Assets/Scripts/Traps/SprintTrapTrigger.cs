using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerControllers;

public class SprintTrapTrigger : MonoBehaviour
{
    [SerializeField] private bool _enableTrap = true;
    [SerializeField] private LayerMask _playerLayerMask;
    [SerializeField] private float _sprintStopDuration = 2f;
    public bool EnableTrap => _enableTrap;

    private FirstPersonController _playerCharacterController;
    private float _sprintStopTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (((_playerLayerMask.value & (1 << other.gameObject.layer)) == 0) || !_enableTrap)
        {
            return;
        }
        _playerCharacterController = other.GetComponentInParent<FirstPersonController>();
        if (_playerCharacterController == null)
        {
            return;
        }
        _playerCharacterController.SetCanSprint(false);
        _sprintStopTimer = _sprintStopDuration;
    }

    private void FixedUpdate()
    {
        SprintUpdate();
    }
    private void SprintUpdate()
    {
        if(_playerCharacterController != null)
        {
            if (_sprintStopTimer < 0)
            {
                _playerCharacterController.SetCanSprint(true);
            }
            else
            {
                _sprintStopTimer -= Time.deltaTime;
            }
        }
    }
    private IEnumerator SprintCancelledForDuration(FirstPersonController playerCharacterController)
    {
        playerCharacterController.SetCanSprint(false);
        yield return new WaitForSeconds(_sprintStopDuration);
        playerCharacterController.SetCanSprint(true);
    }

}
