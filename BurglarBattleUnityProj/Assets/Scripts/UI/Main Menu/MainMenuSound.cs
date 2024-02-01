using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSound : MonoBehaviour
{
    [Header("Splash Door")]
    [SerializeField] private Audio _splashDoorAudio;
    [SerializeField] private Audio _splashDoorSqueakAudio;
    [SerializeField] private GameObject _splashDoor;
    [Space(5)]
    [SerializeField] private Audio optionChangeAudio;
    [SerializeField] private Audio optionSelectAudio;

    public void PlaySplashScreen(float squeakDelay = 0.3f)
    {
        StartCoroutine(PlaySplashDoor(squeakDelay));
    }

    private IEnumerator PlaySplashDoor(float squeakDelay)
    {
        /*AudioManager.PlayOneShotWorldSpace(_splashDoorAudio, _splashDoor.transform.position);*/
        AudioManager.PlayScreenSpace(_splashDoorAudio);
        yield return new WaitForSeconds(squeakDelay);
        /*AudioManager.PlayOneShotWorldSpace(_splashDoorSqueakAudio, _splashDoor.transform.position);*/
        AudioManager.PlayScreenSpace(_splashDoorSqueakAudio);
    }

    public void PlayButtonSelect()
    {
        AudioManager.PlayScreenSpace(optionSelectAudio);
    }

    public void PlayButtonChange()
    {
        AudioManager.PlayScreenSpace(optionChangeAudio);
    }
}
