using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


public class UIStateController : MonoBehaviour
{
    public static UIStateController Instance;

    [Header("Animations")]
    [SerializeField] private Animator _anim;

    [Header("Menus")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _lobbyMenu, _creditsMenu, _settingsMenu;

    [Header("Scripts")]
    [SerializeField] private UISettingsManager _settingsManagerScript;
    [SerializeField] private LevelChange _levelChangeScript;
    [SerializeField] private PlayerPromptDetector _playerPromptDetectorScript;
    private float _timer;
    private WaitForSeconds _waitForSeconds;
    public static MenuInputController menuInputControllers;
    public Vector2 move;
    public int menuOption;
    public UIState currentState;

    public static bool introduced = false;

    [Header("AnimatedObjects")]
    [SerializeField] private GameObject _splashDoor;

    [Header("Audio")]
    [SerializeField] private Audio _splashDoorAudio;
    [SerializeField] private Audio _splashDoorSqueakAudio;
    [Space(5)]
    public Audio optionChangeAudio;
    public Audio optionSelectAudio;

    public static bool acceptInput { get; set; }


    /*//Cooldown between select presses so menu has time to transition
    private IEnumerator WaitForTransition()
    {
        acceptInput = false;
        _timer = float.Epsilon;
        while (_timer < 3f)
        {
            _timer += Time.deltaTime;
            yield return _waitForSeconds;
        }
        acceptInput = true;
        yield return null;
    }*/

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }

        menuOption = 0;
        currentState = UIState.Splash;
        if(menuInputControllers == null)
        {
            menuInputControllers = new MenuInputController();
        }
        else
        {
            menuInputControllers.EnableMenuControls();
        }

        menuInputControllers.PlayerSelectEvent.AddListener(UIChange);
        acceptInput = true;
    }

    public void ChangeMenuOption(int option)
    {
        menuOption = option;
    }

    private void OnEnable()
    {
        ////Debug.Log("UI");
        acceptInput = true;

        if(introduced)
        {
            _anim.SetTrigger("SkipToMain");
            currentState = UIState.Main;
            _mainMenu.SetActive(true);
        }
        else
        {
            currentState = UIState.Splash;
        }

        if(menuInputControllers == null)
        {
            menuInputControllers = new MenuInputController();
        }
        else
        {
            menuInputControllers.EnableMenuControls();
        }
    }

    public void UIChange()
    {
        int option = menuOption;

        ////Debug.Log("Current scene:"+ currentState+ "Option:" + option);
        if(acceptInput)
        {
            /*StartCoroutine(WaitForTransition());*/

            if(currentState != UIState.Splash && currentState != UIState.Lobby)
            {
                AudioManager.PlayScreenSpace(optionSelectAudio);
            }

            switch(currentState)
            {
                case UIState.Splash:
                    SplashChange();
                    break;
                case UIState.Main:
                    MainChange(option);
                    break;
                case UIState.Lobby:
                    LobbyChange(option);
                    break;
                case UIState.Credits:
                    CreditsChange();
                    break;
                    /*case UIState.Settings: 
                        SettingsChange(); 
                        break;*/
            }
        }
    }

    public void Update()
    {
        move = menuInputControllers.moveInput;
    }

    public void SplashChange()
    {
        _splashDoor.GetComponent<Animator>().SetTrigger("Pressed");
        AudioManager.PlayOneShotWorldSpace(_splashDoorAudio, _splashDoor.transform.position);
        PlaySqueak();

        _anim.SetTrigger("SplashToMain");
        _mainMenu.SetActive(true);
        currentState = UIState.Main;

        introduced = true;
    }

    private void PlaySqueak()
    {
        /*await Task.Delay(300);*/
        AudioManager.PlayOneShotWorldSpace(_splashDoorSqueakAudio, _splashDoor.transform.position);
    }
 
    public void MainChange(int option)
    {
        switch(option)
        {
            case 0:
                //Main To Lobby
                if(_anim != null)
                {
                    _anim.SetTrigger("MainToLobby");
                }

                if(_lobbyMenu != null)
                {
                    _lobbyMenu.SetActive(true);
                }

                currentState = UIState.Lobby;
                break;
            case 1:
                //Main To Settings
                _anim.SetTrigger("MainToSettings");
                // _settingsManagerScript.EnableSettingsMenu();
                _settingsMenu.SetActive(true);
                currentState = UIState.Settings;
                break;
            case 2:
                //Main To Credits
                _anim.SetTrigger("MainToCredits");
                _creditsMenu.SetActive(true);
                currentState = UIState.Credits;
                break;
        }
        _mainMenu.SetActive(false);
    }

    public void LobbyChange(int option)
    {
        switch(option)
        {
            case 0:
                //Lobby To Scene
                if(_playerPromptDetectorScript.CheckTeamValidity())
                {
                    _playerPromptDetectorScript.OnLobbyConfirm();
                    _anim.SetTrigger("LobbyToScreen");
                    menuInputControllers.DisableMenuControls();
                    currentState = UIState.Main;
                    menuOption = 0;
                    _levelChangeScript.ChangeScene();
                    FadeTransition.instance.FadeIn(true);
                    if (_lobbyMenu != null)
                    {
                        _lobbyMenu.SetActive(false);
                    }
                }
                break;
            case 1:
                AudioManager.PlayScreenSpace(optionSelectAudio);
                //Lobby To Main
                _anim.SetTrigger("LobbyToMain");
                _mainMenu.SetActive(true);
                currentState = UIState.Main;
                _lobbyMenu.SetActive(false);
                break;
        }

    }

    public void CreditsChange()
    {
        //Credits To Main
        _anim.SetTrigger("CreditsToMain");
        _mainMenu.SetActive(true);
        currentState = UIState.Main;
        _creditsMenu.SetActive(false);
    }

    public void SettingsChange()
    {
        //Settings To Main
        _anim.SetTrigger("SettingsToMain");
        _mainMenu.SetActive(true);
        _settingsMenu.SetActive(false);
        currentState = UIState.Main;
    }
}

public enum UIState
{
    Main,
    Lobby,
    Credits,
    Settings,
    Splash
}
