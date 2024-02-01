using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public GameObject PlayButton;
    public GameObject CreditsButton;
    public GameObject OptionsButton;
    public GameObject CreditsReturn;
    public GameObject OptionsReturn;
    public GameObject LobbyReturn;
    public Animator anim;
    public GameObject firstButton;

    private void Start()
    {
        // EventSystem.current.SetSelectedGameObject(null);
        // EventSystem.current.SetSelectedGameObject(firstButton);

    }

    

    public void GoToUI(int option)
    {
        switch (option)
        {
            case 0:
                //Main To Lobby
                anim.SetTrigger("MainToLobby");
                break;
            case 1:
                //Main To Credits
                anim.SetTrigger("MainToCredits");
                break;
            case 2:
                //Main To Settings
                anim.SetTrigger("MainToSettings");
                break;
            case 3:
                //Lobby To Main
                anim.SetTrigger("LobbyToMain");
                break;
            case 4:
                //Credits To Main
                anim.SetTrigger("CreditsToMain");
                break;
            case 5:
                //Settings To Main
                anim.SetTrigger("SettingsToMain");
                break;
            case 6:
                //Lobby To Screen
                ////Debug.Log("Lobby to Screen");
                anim.SetTrigger("LobbyToScreen");
                break;
        }
    }
    // public void SetSelectedButton(int option)
    // {
    //     switch (option)
    //     {
    //         case 0:
    //             //Main To Lobby
    //             EventSystem.current.SetSelectedGameObject(LobbyReturn);
    //             break;
    //         case 1:
    //             //Main To Credits
    //             EventSystem.current.SetSelectedGameObject(CreditsReturn);
    //             break;
    //         case 2:
    //             //Main To Settings
    //             EventSystem.current.SetSelectedGameObject(OptionsReturn);
    //             break;
    //         case 3:
    //             //Lobby To Main
    //             EventSystem.current.SetSelectedGameObject(PlayButton);
    //             break;
    //         case 4:
    //             //Credits To Main
    //             EventSystem.current.SetSelectedGameObject(CreditsButton);
    //             break;
    //         case 5:
    //             //Settings To Main
    //             EventSystem.current.SetSelectedGameObject(OptionsButton);
    //             break;
    //     }
    // }
    // public void Exit()
    // {
    //     Application.Quit();
    // }

    public void LevelLoad()
    {
        SceneManager.LoadScene("PrototypeScene");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
