using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class MenuInputController
{
    /// <summary>
    /// MenuInputController, A static input controller that handles data for menus.
    /// Subscribe to PlayerSelectEvent to access the Select input.
    /// access moveInput to get the vertical and horizontal keyboard or gamepad controls.
    /// </summary>
    public Vector2 moveInput;
    
    public UnityEvent PlayerSelectEvent;
    public UnityEvent PlayerCancelEvent;
    private List<int> _connectDeviceIDs = new List<int>();

    public MenuInputController()
    {
        if (PlayerSelectEvent == null)
        {
            PlayerSelectEvent = new UnityEvent();
        }

        if (PlayerCancelEvent == null)
        {
            PlayerCancelEvent = new UnityEvent();
        }

        moveInput = Vector2.zero; 
        if (InputDevices.CurrentDeviceCount < 1)
        {
            InputDevices.StartSearchForDevices();
            InputDevices.OnDevicePairedEvent += SetupDevice;
            InputDevices.OnDeviceRemovedEvent += UnregisterDevice;
            
        }
    }
    
    /// <summary>
    /// SetupDevice adds listeners to the relevant events.
    /// Also stops device search if at max devices.
    /// </summary>
    /// <param name="deviceID"></param>
    public void SetupDevice(int deviceID)
    {
        if (InputDevices.CurrentDeviceCount >= InputDevices.MAX_DEVICE_COUNT)
        {
            InputDevices.StopSearchForDevices();
        }

        InputDevices.Devices[deviceID].Actions.MenuUI.Move.performed += OnMove;
        InputDevices.Devices[deviceID].Actions.MenuUI.Move.canceled += OnMove;
        InputDevices.Devices[deviceID].Actions.MenuUI.Select.performed += OnSelect;
        InputDevices.Devices[deviceID].Actions.MenuUI.Cancel.performed += OnCancel;
        _connectDeviceIDs.Add(deviceID);

    }
    private void OnSelect(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
              PlayerSelectEvent?.Invoke();
        }
    }

    private void OnCancel(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            PlayerCancelEvent?.Invoke();
        }
    }

    private void OnMove(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            MoveInput(value.ReadValue<Vector2>());
        }
        else if(value.canceled)
        {
            MoveInput(Vector2.zero);
        }
        
    }

    private void MoveInput(Vector2 newMoveDirection)
    {
     
        moveInput = newMoveDirection;
        
    }
  

    /// <summary>
    /// If unregistered device then search for devices
    /// </summary>
    /// <param name="deviceID"></param>
    public void UnregisterDevice(int deviceID)
    {
        if (InputDevices.CurrentDeviceCount <4)
        {
            InputDevices.StartSearchForDevices();
        }
    }

    /// <summary>
    /// Toggles off the MenuUI actions for each active device.
    /// </summary>
    public void DisableMenuControls()
    {
        
        foreach(var inputcontroller in InputDevices.Devices)
        {
            if (inputcontroller != null)
            {
                inputcontroller.Actions.MenuUI.Disable();
                PlayerSelectEvent.RemoveAllListeners();
                PlayerCancelEvent.RemoveAllListeners();
            }
        }
    }

    /// <summary>
    /// Toggles on the MenuUI actions for each active device.
    /// </summary>
    public void EnableMenuControls()
    {
        foreach(var inputcontroller in InputDevices.Devices)
        {
            if (inputcontroller != null)
            {
                inputcontroller.Actions.MenuUI.Enable();
            }
        }
    }
    private void OnDestroy()
    {
        UnsubscribeEvents();
    }
    public void UnsubscribeEvents()
    {
        InputDevices.OnDevicePairedEvent -= SetupDevice;
        InputDevices.OnDeviceRemovedEvent -= UnregisterDevice;

        int deviceID = 0;
        for(int i = 0; i < _connectDeviceIDs.Count; i++)
        {
            deviceID = _connectDeviceIDs[i];
            InputDevices.Devices[deviceID].Actions.MenuUI.Move.performed -= OnMove;
            InputDevices.Devices[deviceID].Actions.MenuUI.Move.canceled -= OnMove;
            InputDevices.Devices[deviceID].Actions.MenuUI.Select.performed -= OnSelect;
            InputDevices.Devices[deviceID].Actions.MenuUI.Cancel.performed -= OnCancel;
        }
        /*InputDevices.Devices[deviceID].Actions.MenuUI.Move.performed += OnMove;
        InputDevices.Devices[deviceID].Actions.MenuUI.Move.canceled += OnMove;
        InputDevices.Devices[deviceID].Actions.MenuUI.Select.performed += OnSelect;
        InputDevices.Devices[deviceID].Actions.MenuUI.Cancel.performed += OnCancel;*/
    }
}
