using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Temp_BGM_Manager : MonoBehaviour
{
    private string _sceneid;

    /// <summary>
    /// Int value used to dictate which music should be playing in the level based on which doors are open.
    /// </summary>
    private int _audioState = 0;
    [SerializeField] private AudioSource _menu, _game;
    void Start()
    {
        //subscribes to the scene changing event and then starts the correct music for whichever scene the game is being started from.
        SceneManager.activeSceneChanged += ChangedActiveScene;
        UpdateMusic();
    }

    //these are required components for the ChangedActiveScene in unity, do not remove them.
    private void ChangedActiveScene(Scene current, Scene next)
    {
        UpdateMusic();
    }

    public void UpdateMusic()
    {
        _sceneid = SceneManager.GetActiveScene().name;
        if (_sceneid == "Main Menu Scene")
        {
            if (!_menu.isPlaying)
            { _menu.Play(); }
        }
        else
        {
            if (!_game.isPlaying)
            {
                _game.Play();
            }
        }
    }

    public int GetAudioState()
    {
        return _audioState;
    }

    public void SetAudioState(int newState)
    {
        _audioState = newState;
    }
}
