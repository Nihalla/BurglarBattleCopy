using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroduceLevel : MonoBehaviour
{
    [Header("Modified GameObjects")]
    [SerializeField] private GameObject _canvasParent;
    [SerializeField] private Animator _doorAnimator;
    /*[SerializeField] private GameObject _floorTwo;*/

    private void OnEnable()
    {
        //_canvasParent.SetActive(false);
        /*_floorTwo.SetActive(true);*/
        FirstPersonController.IsDisabled = true;
    }

    private void OnDisable()
    {
        // FIX(Zack): stops null reference exception
        if (_canvasParent != null) {
            _canvasParent.SetActive(true);
        }
        /*_floorTwo.SetActive(false);*/
        Destroy(_doorAnimator);
        FirstPersonController.IsDisabled = false;
    }
}
