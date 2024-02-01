using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPressCallAnim : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void ButtonPressed()
    {
        if(_audioSource != null)
        {
            _audioSource.Play();
        }

           // //Debug.Log("Playing audio " + _audioSource.clip.name);
        //GetComponent<Animator>().SetTrigger("Pressed");
    }
}
