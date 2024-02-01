using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Update_BGM : MonoBehaviour
{
    private Background_Music_Manager _bgmManager;
    void Start()
    {
        _bgmManager = GameObject.FindObjectOfType<Background_Music_Manager>();
    }

    public void OpenFirstDoor()
    {
        if (_bgmManager == null)
        {
            //if we failed to find the manager on start find it now
            _bgmManager = GameObject.FindObjectOfType<Background_Music_Manager>();
        }
        if (_bgmManager.GetAudioState() == 0)
        {
            _bgmManager.SetAudioState(1);
            _bgmManager.UpdateMusic();
        }
    }

    public void OpenSecondDoor()
    {
        if (_bgmManager == null)
        {
            //if we failed to find the manager on start find it now
            _bgmManager = GameObject.FindObjectOfType<Background_Music_Manager>();
        }
        if (_bgmManager.GetAudioState() == 1)
        {
            _bgmManager.SetAudioState(2);
            _bgmManager.UpdateMusic();
        }
    }

    public void OpenVaultDoor()
    {
        if (_bgmManager == null)
        {
            //if we failed to find the manager on start find it now
            _bgmManager = GameObject.FindObjectOfType<Background_Music_Manager>();
        }
        if (_bgmManager.GetAudioState() == 2)
        {
            _bgmManager.SetAudioState(3);
            _bgmManager.UpdateMusic();
        }
    }
}
