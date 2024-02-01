using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVisualSettings : MonoBehaviour
{
    //public AudioMixer mixer;

    enum DropOptions
    {
        OFF,
        MINIMAL,
        MAX
    }



    /// <summary>
    /// All settings functionality for the AUDIO/VISUAL settings menu
    /// Any functions relating to objects within this group are found here
    /// </summary>

    /// <param name="slider"></param>
    public void MainVolume(Slider slider)
    {
        //mixer.SetFloat("MainVolume", Mathf.Log10(slider.value) * 20);
    }

    public void MusicVolume(Slider slider)
    {
        //mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SFXVolume(Slider slider)
    {
        //mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }

    public void MotionBlur(TMP_Dropdown motionBlurDropDown)
    {
        switch (motionBlurDropDown.value)
        {
            case (int)DropOptions.MAX:
                //set motionblur to ON or MAX
                break;
            case (int)DropOptions.MINIMAL:
                //set motionblur to MINIMAL or REDUCED
                break;
            case (int)DropOptions.OFF:
                //set motionblur to OFF 
                break;
        }
    }

    public void CameraShake(TMP_Dropdown cameraShakeDropDown)
    {
        switch (cameraShakeDropDown.value)
        {
            case (int)DropOptions.MAX:
                //set screenshake to ON or MAX
                break;
            case (int)DropOptions.MINIMAL:
                //set screenshake to MINIMAL or REDUCED
                break;
            case (int)DropOptions.OFF:
                //set screenshake to OFF
                break;
        }
    }
}