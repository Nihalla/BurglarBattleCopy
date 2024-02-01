using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;


public class GameplaySettings : MonoBehaviour
{

    enum GPPref
    {
        Hold,
        Toggle
    }

    public void AimSensX(Slider slider)
    {
        //Slider value used to change x sensitivity
    }

    public void AimSensY(Slider slider)
    {
        //Slider value used to change y sensitivity
    }

    public void InvertAim(Toggle toggle)
    {
        if (toggle.isOn)
        {

        }

        if (!toggle.isOn)
        {

        }
    }

    public void SprintPref(TMP_Dropdown dropdown)
    {
        switch(dropdown.value)
        {
            case (int)GPPref.Hold:
                //Change controller pref to "hold" for sprint
                break;

            case (int)GPPref.Toggle:
                //change controller pref to "toggle" for sprint
                break;
        }
    }

    public void CrouchPref(TMP_Dropdown dropdown)
    {
        switch(dropdown.value)
        {
            case (int)GPPref.Hold:
                //change controller pref to "hold" for crouch
                break;

            case (int)GPPref.Toggle:
                //change controller pref to "toggle" for crouch
                break;
        }
    }
}
