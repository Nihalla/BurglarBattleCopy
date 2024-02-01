using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Background_Music_Manager : MonoBehaviour
{
    private int _sceneid;
    /// <summary>
    /// Int value used to dictate which music should be playing in the level based on which doors are open.
    /// </summary>
    private int _audioState = 0;
    [SerializeField] private AudioSource _menu, _gameStart, _gameSecondFloor, _gameThirdFloor, _gameEnd, _endScreen;
    [SerializeField] private AudioSource _transitionChime;
    [SerializeField] private AudioSource _vaultAlarm;
    void Start()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;
        UpdateMusic();
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        _audioState = 0;
        UpdateMusic();
    }

    public void UpdateMusic()
    {
        _sceneid = SceneManager.GetActiveScene().buildIndex;
        if (_sceneid == 0)
        {
            if (_gameStart.isPlaying)
            {
                _gameStart.Stop();
            }
            if (_gameSecondFloor.isPlaying)
            {
                _gameSecondFloor.Stop();
            }
            if (_gameThirdFloor.isPlaying)
            {
                _gameThirdFloor.Stop();
            }
            if (_gameEnd.isPlaying)
            {
                _gameEnd.Stop();
            }
            if (_endScreen.isPlaying)
            {
                _endScreen.Stop();
            }

            if (!_menu.isPlaying)
            {
                _menu.Play();
            }
        }
        else if (_sceneid == 1)
        {
            if (_menu.isPlaying)
            {
                _menu.Stop();
            }

            if (_endScreen.isPlaying)
            {
                _endScreen.Stop();
            }

            switch (_audioState)
            {
                case 0:
                    if (!_gameStart.isPlaying)
                    {
                        _gameStart.Play();
                        _gameSecondFloor.Stop();
                        _gameThirdFloor.Stop();
                        _gameEnd.Stop();
                    }
                    break;
                case 1:
                    if (!_gameSecondFloor.isPlaying)
                    {
                        _transitionChime.Play();
                        _gameStart.Stop();
                        _gameSecondFloor.Play();
                        _gameThirdFloor.Stop();
                        _gameEnd.Stop();
                    }
                    break;
                case 2:
                    if (!_gameThirdFloor.isPlaying)
                    {
                        _transitionChime.Play();
                        _gameStart.Stop();
                        _gameSecondFloor.Stop();
                        _gameThirdFloor.Play();
                        _gameEnd.Stop();
                    }
                    break;

                case 3:
                    if (!_gameEnd.isPlaying || !_vaultAlarm.isPlaying)
                    {
                        _vaultAlarm.Play();
                        _gameStart.Stop();
                        _gameSecondFloor.Stop();
                        _gameThirdFloor.Stop();
                        StartCoroutine(EndGameMusic());
                    }
                    break;

                default:
                    {
                        ////Debug.Log("Error Playing BGM");
                    }
                    break;
            }
        }
        else if (_sceneid == 2)
        {
            if (_gameStart.isPlaying)
            {
                _gameStart.Stop();
            }
            if (_gameSecondFloor.isPlaying)
            {
                _gameSecondFloor.Stop();
            }
            if (_gameThirdFloor.isPlaying)
            {
                _gameThirdFloor.Stop();
            }
            if (_gameEnd.isPlaying)
            {
                _gameEnd.Stop();
            }
            if (_menu.isPlaying)
            {
                _menu.Stop();
            }
            if (!_endScreen.isPlaying)
            {
                _endScreen.Play();
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

    IEnumerator EndGameMusic()
    {
        yield return new WaitForSeconds(10f);
        _gameEnd.Play();
    }
}
