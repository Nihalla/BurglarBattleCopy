// Author: William Whitehouse

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

public static class InputDevices
{
    /// <summary>
    /// The maximum amount of devices allowed.
    /// </summary>
    public const int MAX_DEVICE_COUNT = 4;
    
    /// <summary>
    /// The current amount of devices paired.
    /// </summary>
    public static int CurrentDeviceCount { get; private set; } = 0;

    /// <summary>
    /// The paired device array, its length is <see cref="MAX_DEVICE_COUNT"/>.
    /// New devices are added when a player presses a button on the device and
    /// when we're searching <see cref="StartSearchForDevices"/> and
    /// <see cref="StopSearchForDevices"/>. Invalid or unpaired devices are
    /// null, they can be removed by calling <see cref="RemoveDevice"/>. 
    /// </summary>
    public static DeviceData[] Devices { get; private set; } = new DeviceData[MAX_DEVICE_COUNT];
    
    public delegate void OnDevicePairedDel(int deviceIndex);
    public delegate void OnDeviceRemovedDel(int deviceIndex);

    /// <summary>
    /// An event that is invoked when a new input device is paired.
    /// </summary>
    public static OnDevicePairedDel OnDevicePairedEvent;
    
    /// <summary>
    /// An event that is invoked when a input device is removed.
    /// </summary>
    public static OnDeviceRemovedDel OnDeviceRemovedEvent;
    
    /// <summary>
    /// Holds if we are currently searching for devices as we shouldn't subscribe
    /// to the InputUser events multiple times.
    /// </summary>
    public static bool s_SearchingForDevices { get; private set; } = false;

    /// <summary>
    /// Start searching for devices to pair too. Won't pair to the same
    /// device multiple times. Call <see cref="StopSearchForDevices"/>
    /// when done searching. The <see cref="OnDevicePairedEvent"/> is
    /// invoked when a new device is found and paired.
    /// </summary>
    public static void StartSearchForDevices()
    {
        if (s_SearchingForDevices) return;
        s_SearchingForDevices = true;
        
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        ++InputUser.listenForUnpairedDeviceActivity;
    }

    /// <summary>
    /// Stop searching for devices. See <see cref="StartSearchForDevices"/>
    /// for start equivalent function.
    /// </summary>
    public static void StopSearchForDevices()
    {
        if (!s_SearchingForDevices) return;
        s_SearchingForDevices = false;
        
        InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;
        --InputUser.listenForUnpairedDeviceActivity;
    }
    
    /// <summary>
    /// Remove and unpair a device at the requested index from the list.
    /// Invokes <see cref="OnDeviceRemovedEvent"/> with the appropriate
    /// index. 
    /// </summary>
    /// <param name="deviceIndex">Index of the device to remove</param>
    public static void RemoveDevice(int deviceIndex)
    {
        if (deviceIndex < 0)                 return;
        if (deviceIndex >= MAX_DEVICE_COUNT) return;
        if (Devices[deviceIndex] == null)    return;
        
        // Destroy device
        Devices[deviceIndex].Destroy();
        Devices[deviceIndex] = null;
        CurrentDeviceCount--;
        
        OnDeviceRemovedEvent?.Invoke(deviceIndex);
    }

#region PRIVATE IMPLEMENTATION

    /// <summary>
    /// The private input actions used to detect devices. This should NOT be used
    /// for reading any player input.
    /// </summary>
    private static readonly InputActions s_inputActions = new InputActions();
    
    /// <summary>
    /// An array of all the control schemes used to detect valid input devices. See
    /// <see cref="InputDevices"/> static constructor for initialisation of array.
    /// </summary>
    private static InputControlScheme[] s_controlSchemes;

    static InputDevices()
    {
        // NOTE(WSWhitehouse): Getting the index of all the control schemes here so they can be split up into their own array
        ReadOnlyArray<InputControlScheme> controlSchemes = s_inputActions.controlSchemes;
        int keyboardIndex = controlSchemes.IndexOf(x => x.name == "Keyboard and Mouse");
        int gamepadIndex  = controlSchemes.IndexOf(x => x.name == "Gamepad");
        
        s_controlSchemes    = new InputControlScheme[2];
        s_controlSchemes[0] = controlSchemes[keyboardIndex];
        s_controlSchemes[1] = controlSchemes[gamepadIndex];

        // NOTE(WSWhitehouse): Initialise all devices to null...
        for (int i = 0; i < MAX_DEVICE_COUNT; i++)
        {
            Devices[i] = null;
        }
    }

    
    private static void PairDevice(InputDevice device, InputControlScheme controlScheme)
    {
        // Get a unused device
        int deviceIndex = -1;
        for (int i = 0; i < MAX_DEVICE_COUNT; i++)
        {
            if (Devices[i] != null) continue;
            
            deviceIndex = i;
            break;
        }
        
        if (deviceIndex == -1) return;
        
        // Create and pair new device
        Devices[deviceIndex] = new DeviceData(device, controlScheme);
        CurrentDeviceCount++;
        
        OnDevicePairedEvent?.Invoke(deviceIndex);
    }

    private static void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        if (CurrentDeviceCount >= MAX_DEVICE_COUNT) return;
        if (control is not ButtonControl)           return;
        
        InputDevice device = control.device;
        if (device is Mouse)    return;
        if (device is Keyboard) return;
        
        InputControlScheme? controlScheme = FindControlScheme(device);
        
        if (!controlScheme.HasValue) return;
        if (!IsDeviceValid(device))  return;

        PairDevice(device, controlScheme.Value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InputControlScheme? FindControlScheme(InputDevice device)
    {
        if (s_inputActions.controlSchemes.Count <= 0) return null;

        using InputControlList<InputDevice> unpairedDevices = InputUser.GetUnpairedInputDevices();
        return InputControlScheme.FindControlSchemeForDevices(unpairedDevices, s_controlSchemes, device);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDeviceValid(InputDevice device)
    {
        foreach (InputActionMap actionMap in s_inputActions.asset.actionMaps)
        {
            if (actionMap.IsUsableWithDevice(device)) return true;
        }

        return false;
    }

#endregion // PRIVATE IMPLEMENTATION

}
