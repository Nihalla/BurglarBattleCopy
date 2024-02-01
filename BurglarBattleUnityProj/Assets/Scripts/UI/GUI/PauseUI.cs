using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class PauseUI : MonoBehaviour
{
    private InputActions _inputs;
    private bool _menuActive = false;
    private bool _controlsMenuAcive = false;
    private int _currentButton = 0;

    public GameObject pauseMenuPanel;
    public GameObject[] pauseMenuButtons;
    public GameObject controlsPanel;

    [Scene] public int mainMenuScene;

    private void Awake()
    {
        _inputs = new InputActions();
        _inputs.Enable();
        _inputs.MenuUI.Pause.performed += PauseMenu;
        _inputs.MenuUI.Move.performed += ChangeOptions;
        _inputs.MenuUI.Select.performed += SelectButton;
        _inputs.MenuUI.Back.performed += CloseControlMenu;
    }

    private void OnDestroy()
    {
        _inputs.MenuUI.Pause.performed -= PauseMenu;
        _inputs.MenuUI.Move.performed -= ChangeOptions;
        _inputs.MenuUI.Select.performed -= SelectButton;
        _inputs.MenuUI.Back.performed -= CloseControlMenu;
    }

    //enabling and disabling the pause menu
    private void PauseMenu(InputAction.CallbackContext context)
    {
        if(_menuActive && !_controlsMenuAcive)
        {
            pauseMenuPanel.SetActive(false);
            _menuActive = false;

            Time.timeScale = 1;

            for (int i = 0; i < InputDevices.MAX_DEVICE_COUNT; ++i)
            {
                if (InputDevices.Devices[i] == null) continue;
                InputDevices.Devices[i].Actions.PlayerController.Enable();
                InputDevices.Devices[i].Actions.PlayerLockpick.Enable();
            }

            for (int i = 0; i < pauseMenuButtons.Length; ++i)
            {
                pauseMenuButtons[i].GetComponent<Image>().color = Color.blue;
            }

        }
        else if (!_menuActive && !_controlsMenuAcive)
        {
            pauseMenuPanel.SetActive(true);
            _menuActive = true;

            Time.timeScale = 0;

            for(int i = 0; i < InputDevices.MAX_DEVICE_COUNT; ++i)
            {
                if (InputDevices.Devices[i] == null) continue;
                InputDevices.Devices[i].Actions.PlayerController.Disable();
                InputDevices.Devices[i].Actions.PlayerLockpick.Disable();
            }

            _currentButton = 0;
            pauseMenuButtons[_currentButton].GetComponent<Image>().color = Color.green;
        }

    }   
    
    //Changing selected menu option
    private void ChangeOptions(InputAction.CallbackContext context)
    {
        if(!_controlsMenuAcive && _menuActive)
        {
            Vector2 value = context.ReadValue<Vector2>();

            // change previous buttons colour back to unselected
            int previousButton = _currentButton;
            pauseMenuButtons[previousButton].GetComponent<Image>().color = Color.blue;

            // update new button to show selected
            _currentButton -= (int)value.y;
            _currentButton = WrapIndex(_currentButton, pauseMenuButtons.Length);
            pauseMenuButtons[_currentButton].GetComponent<Image>().color = Color.green;
        }
    }


    //Selecting the current hovered option
    private void SelectButton(InputAction.CallbackContext context)
    {
        if(!_menuActive)
        {
            return;
        }
        
        if(!_controlsMenuAcive)
        {
            if (_currentButton == 0)
            {
                PauseMenu(new InputAction.CallbackContext());
            }
            else if (_currentButton == 1)
            {
                controlsPanel.SetActive(true);
                _controlsMenuAcive = true;
            }
            else if (_currentButton == 2)
            {
                Time.timeScale = 1;
                SceneManager.LoadSceneAsync(mainMenuScene);
            }
        }
    }


    //Closing the player controls menu
    private void CloseControlMenu(InputAction.CallbackContext context)
    {
        if(!_controlsMenuAcive)
        {
            return;
        }

        if(_controlsMenuAcive)
        {
            controlsPanel.SetActive(false);
            _controlsMenuAcive = false;
        }
    }


    private int WrapIndex(int index, int arrayLength)
    {
        return ((index % arrayLength) + arrayLength) % arrayLength;
    }


}
