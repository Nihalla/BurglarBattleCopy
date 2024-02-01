// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts

using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControllers
{
	public class InputActionsInputs : MonoBehaviour
	{
		private DeviceData _device;
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool crouch;

        //UI button inputs
        public bool start;
		public bool useTool;
		public bool shareLoot;
		public bool throwObject;
        public bool useAbility;
        public bool useTool2;
        public bool useEquipment;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		[Header("Inversion Settings")]
		[SerializeField] private bool _movementInverse = false;
		[SerializeField] private bool _lookInverse = false;

		public void OnMove(InputAction.CallbackContext value)
		{
			MoveInput(value.ReadValue<Vector2>());
		}

		public void OnLook(InputAction.CallbackContext value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.ReadValue<Vector2>());
			}
		}

		public void OnJump(InputAction.CallbackContext value)
		{
			JumpInput(value.performed);
		}

		public void OnSprint(InputAction.CallbackContext value)
		{
			SprintInput(value.performed);
		}

		public void OnCrouch(InputAction.CallbackContext value)
        {
			CrouchInput(value.performed);
        }

        //UI and GUI Inputs 
        public void OnStart(InputAction.CallbackContext value)
        {
            StartInput(value.performed);
        }
		public void OnUseTool(InputAction.CallbackContext value)
        {
			UseToolInput(value.performed);
        }
		public void OnThrowObject(InputAction.CallbackContext value)
		{
			UseThrowObjectInput(value.performed);
		}
        public void OnUseTool2(InputAction.CallbackContext value)
        {
            UseTool2Input(value.performed);
        }

		public void OnShareLoot(InputAction.CallbackContext value)
        {
			ShareLootInput(value.performed);
        }
		public void MoveInput(Vector2 newMoveDirection)
		{
			if (_movementInverse)
			{
				move = newMoveDirection * -1;	
			}
			else
			{
				move = newMoveDirection;
			}
			
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			if (_lookInverse)
			{
				look = newLookDirection * -1;
			}
			else
			{
				look = newLookDirection;
			}
			
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

        //UI and GUI Inputs
        public void StartInput(bool newStartState)
        {
            start = newStartState;
        }

		public void UseToolInput(bool newToolState)
		{
			useTool = newToolState;
		}

        public void UseAbilityInput(bool newAbilityState)
        {
            useAbility = newAbilityState;
        }

        public void UseTool2Input(bool newTool2State)
        {
            useTool2 = newTool2State;
        }

        public void UseEquipmentInput(bool newEquipmentState)
        {
            useEquipment = newEquipmentState;
        }

		public void UseThrowObjectInput(bool newThrowState)
		{
			throwObject = newThrowState;
		}


		public void SetMovementInverseControls(bool inverse)
		{
			_movementInverse = inverse;
		}
		
		public void SetLookInverseControls(bool inverse)
		{
			_lookInverse = inverse;
		}
		
		public void CrouchInput(bool newCrouchState)
        {
			crouch = newCrouchState;
        }

		public void ShareLootInput(bool newLootState)
        {
			shareLoot = newLootState;
        }

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			if(newState)
            {
				Cursor.lockState = CursorLockMode.Locked;

			}
            else
            {
				Cursor.lockState = CursorLockMode.None;

			}
		}

		public void SetDevice(int index)
        {
			_device = InputDevices.Devices[index];
        }
		public InputActions GetActions()
        {
			return _device.Actions;
        }
		public void SetUpDevice()
        {
			if(_device != null)
            {
				_device.Actions.PlayerController.Move.performed += ctx => OnMove(ctx);
				_device.Actions.PlayerController.Move.canceled += ctx => MoveInput(Vector2.zero);
				_device.Actions.PlayerController.Jump.performed += ctx => OnJump(ctx);
				_device.Actions.PlayerController.Jump.canceled += ctx => OnJump(ctx);
				_device.Actions.PlayerController.Sprint.performed += ctx => OnSprint(ctx);
				_device.Actions.PlayerController.Sprint.canceled += ctx => OnSprint(ctx);
				_device.Actions.PlayerController.Crouch.performed += ctx => OnCrouch(ctx);
				_device.Actions.PlayerController.Crouch.canceled += ctx => OnCrouch(ctx);
                _device.Actions.PlayerController.Start.performed += ctx => OnStart(ctx);
                _device.Actions.PlayerController.Start.canceled += ctx => OnStart(ctx);;
				_device.Actions.PlayerController.CameraRotation.performed += ctx => OnLook(ctx);
				_device.Actions.PlayerController.CameraRotation.canceled += ctx => LookInput(Vector2.zero);
				_device.Actions.PlayerController.UseTool.performed += ctx => OnUseTool(ctx);
				_device.Actions.PlayerController.UseTool.canceled += ctx => OnUseTool(ctx);
				_device.Actions.PlayerController.Throw.performed += ctx => OnThrowObject(ctx);
				_device.Actions.PlayerController.Throw.canceled += ctx => OnThrowObject(ctx);

			}
        }
    }
}
