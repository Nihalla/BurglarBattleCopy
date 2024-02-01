using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DelayedButtonPress : MonoBehaviour
{
    private float time =0;
    public void ChangeButton(GameObject button)
    {
        StartCoroutine(ChangeButtonRoutine(button));
    }
    public void Delay(float delay)
    {
        time = delay;
    }

    IEnumerator ChangeButtonRoutine(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSeconds(time);
        EventSystem.current.SetSelectedGameObject(button);
    }
}   
