// Author: William Whitehouse

using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.InputSystem.Users;

public class DeviceData
{
    public DeviceData(InputDevice device, InputControlScheme controlScheme)
    {
        // https://forum.unity.com/threads/input-system-generate-c-code-what-its-good-for.995674/
        // https://forum.unity.com/threads/solved-can-the-new-input-system-be-used-without-the-player-input-component.856108/#post-5669128

        // Set variables
        Device           = device;
        ControlScheme    = controlScheme;
        Actions          = new InputActions();
        IsDeviceGamepad  = Device is Gamepad;
        IsDeviceKeyboard = Device is Keyboard;
        CanDeviceRumble  = Device is IDualMotorRumble;

        // Set up Input Actions
        if (IsDeviceKeyboard)
        {
            // NOTE(WSWhitehouse): When device is a keyboard we also want to bind to the mouse...
            Actions.devices     = new[] { Device, Mouse.current };
            Actions.bindingMask = InputBinding.MaskByGroups(Device.name, Mouse.current.name);
        }
        else
        {
            Actions.devices     = new[] { Device };
            Actions.bindingMask = InputBinding.MaskByGroup(Device.name);
        }
        
        Actions.Enable();

        // Set up Input User
        User = InputUser.PerformPairingWithDevice(Device);
        User.AssociateActionsWithUser(Actions);
        User.ActivateControlScheme(ControlScheme);
        
        ////Debug.Log($"New device paired ({Device.name}) with control scheme '{ControlScheme.name}'!");
    }

    /// <summary>
    /// Destroy this Device Data and unpair the input users. Should only
    /// be called by <see cref="InputDevices"/>!
    /// </summary>
    public void Destroy()
    {
        ////Debug.Log($"'{Device.name}' removed!");
                
        User.UnpairDevicesAndRemoveUser();
        Actions.Disable();
        Device        = null;
        ControlScheme = new InputControlScheme();
    }

    // Input Data
    public InputActions Actions             { get; private set; }
    public InputUser User                   { get; private set; }
    public InputDevice Device               { get; private set; }
    public InputControlScheme ControlScheme { get; private set; }

    public bool IsDeviceGamepad  { get; }
    public bool IsDeviceKeyboard { get; }
    public bool CanDeviceRumble  { get; }
    
    public bool RumbleActive { get; private set; } = false;
    public float2 RumbleFreq { get; private set; } = float2.zero;
    
    /// <summary>
    /// Rumble the device based on the frequency parameters. Use <see cref="RumbleReset"/> to stop
    /// device rumble, or <see cref="RumblePulse"/> for quick pulses.
    /// </summary>
    /// <param name="leftFreq">Left frequency controller rumble (clamped between 0..1)</param>
    /// <param name="rightFreq">Right frequency controller rumble (clamped between 0..1)</param>
    public void Rumble(float leftFreq, float rightFreq)
    {
        if (!CanDeviceRumble) return;
        
        RumbleActive = true;
        RumbleFreq   = new float2(leftFreq, rightFreq);
        
        if (leftFreq <= float.Epsilon && rightFreq <= float.Epsilon)
        {
            Debug.LogWarning("DeviceData::Rumble: Left and Right frequencies are 0! Prefer to use `RumbleReset()` instead!");
            RumbleActive = false;
        }
        
        IDualMotorRumble motorRumble = Device as IDualMotorRumble;
        Debug.Assert(motorRumble != null, "Device can rumble but is not a IDualMotorRumble! This is not possible.");
        
        motorRumble.SetMotorSpeeds(leftFreq, rightFreq);
    }
    
    /// <summary>
    /// Pulse the controller rumble for a duration, resets the rumble automatically after that
    /// duration. Requires a valid MonoBehaviour to perform Coroutine logic. See <see cref="Rumble"/>
    /// for full manual control over device rumble. Warning: this will overwrite any manual behaviour
    /// once the duration has surpassed.
    /// </summary>
    /// <param name="leftFreq">Left frequency controller rumble (clamped between 0..1)</param>
    /// <param name="rightFreq">Right frequency controller rumble (clamped between 0..1)</param>
    /// <param name="duration">Duration for pulse. Device rumble is reset after this duration.</param>
    /// <param name="mono">A valid MonoBehaviour to perform Coroutine logic. MUST NOT BE NULL!</param>
    /// <param name="forceStop">Force stop the rumble even if the frequencies have been updated by another call to Rumble</param>
    public void RumblePulse(float leftFreq, float rightFreq, float duration, MonoBehaviour mono, bool forceStop = false)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Float2Compare(float2 lhs, float2 rhs)
        {
            if (math.abs(lhs.x - rhs.x) > float.Epsilon) return false;
            if (math.abs(lhs.y - rhs.y) > float.Epsilon) return false;
            return true;
        }
        
        IEnumerator RumblePulseCoroutine(float leftFreq, float rightFreq, float duration, bool forceStop)
        {
            Rumble(leftFreq, rightFreq);
            
            float timer = duration;
            while(timer > float.Epsilon)
            {
                timer -= Time.deltaTime;
                yield return null; // Wait for Update
            }
            
            float2 freq = new float2(leftFreq, rightFreq);
            if (forceStop || Float2Compare(freq, RumbleFreq))
            {
                RumbleReset();
            }
            
            yield break;
        }
        
        Debug.Assert(mono != null, "DeviceData::RumblePulse: MonoBehaviour parameter is null! Please provide a valid mono.");
        mono.StartCoroutine(RumblePulseCoroutine(leftFreq, rightFreq, duration, forceStop));
    }
    
    /// <summary>
    /// Reset device rumble.
    /// </summary>
    public void RumbleReset()
    {
        if (!CanDeviceRumble) return;
        
        IDualMotorRumble motorRumble = Device as IDualMotorRumble;
        Debug.Assert(motorRumble != null, "Device can rumble but is not a IDualMotorRumble! This is not possible.");
        
        RumbleActive = false;
        RumbleFreq   = float2.zero;
        motorRumble.ResetHaptics();
    }
    
    /// <summary>
    /// Pause the rumble in it's current state, use <see cref="RumbleResume"/> to resume rumble.
    /// </summary>
    public void RumblePause()
    {
        if (!CanDeviceRumble) return;
        
        IDualMotorRumble motorRumble = Device as IDualMotorRumble;
        Debug.Assert(motorRumble != null, "Device can rumble but is not a IDualMotorRumble! This is not possible.");
        
        motorRumble.PauseHaptics();
    }
    
    /// <summary>
    /// Resume rumble from a previous pause (use <see cref="RumblePause"/> to pause rumble).
    /// </summary>
    public void RumbleResume()
    {
        if (!CanDeviceRumble) return;
        
        IDualMotorRumble motorRumble = Device as IDualMotorRumble;
        Debug.Assert(motorRumble != null, "Device can rumble but is not a IDualMotorRumble! This is not possible.");
        
        motorRumble.ResumeHaptics();
    }
    
}