// Author: Joe Gollin

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Static class used to implement PS5 Controller Functions inc: Lightbar Colour, Rumble
/// </summary>
public static class PS5Control
{

    /// <summary>
    /// Sets the lightbar colour based on the team, slotIndex is used to set controllers independently 
    /// </summary>
    /// <param name="setColour"></param>
    /// <param name="slotIndex"></param>
    public static void SetGamepadColour(Color setColour, int slotIndex)
    {
        //var pad = UnityEngine.InputSystem.PS5.DualSenseGamepad.GetBySlotIndex(slotIndex);
        //var gamepad = UnityEngine.InputSystem.DualShock.DualSenseGamepadHID.all;
        //gamepad[slotIndex].SetLightBarColor(setColour);
        
    }

    /// <summary>
    /// Sets the controller rumble using the high frequency and low frequency motor speeds, slotIndex is used to set controllers independently.
    /// Example: PS5Control.Rumble(1f, 1f, _playerID, 2f);
    /// </summary>
    /// <param name="high"></param>
    /// <param name="low"></param>
    /// <param name="slotindex"></param>
    /// <param name="duration"></param>
    public static void Rumble(float high, float low, int slotindex, float duration)
    {
        ////Debug.Log("Rumble GO");
        var gamepad = Gamepad.all;
        
        if (gamepad[slotindex] != null)
        {
            // Starts rumble
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                gamepad[slotindex].SetMotorSpeeds(high, low);
                elapsedTime += Time.deltaTime;
            }
            // Stops Rumble
            gamepad[slotindex].SetMotorSpeeds(0f, 0f);;
        }
    }
}
