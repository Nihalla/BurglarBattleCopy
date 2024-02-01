using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LandingMenu : MonoBehaviour
{
    private float _verticalInput;
    [SerializeField] private ButtonInfo[] _menuOptions;
    private float _timer = 0.0f;
    private float _delay = 0.2f;
    [SerializeField] private UIStateController _menuControllerScript;
    [SerializeField] private UIState _uiState;
    private Vector2 move;
    private bool _isActive;
    //private bool _registeredMenu;
    public int lastHighlighted;

    private void Start()
    {
        _menuOptions[_menuControllerScript.menuOption].GetComponent<ButtonInfo>().Highlight();
    }


    // Update is called once per frame
    void Update()
    {
        _verticalInput = _menuControllerScript.move.y;
        if (gameObject.activeInHierarchy && UIStateController.acceptInput)
        {
            GetInput();
        }
    }

    private void GetInput()
    {
        if (_verticalInput != 0)
        {
            IterateMenuOption();
        }
    }

    private void IterateMenuOption()
    {
        if (_timer < 0 && _menuControllerScript.currentState == _uiState)
        {
            if (_uiState == UIState.Credits)
            {
                _menuControllerScript.menuOption = 0;
            }
            
            int menuOption = _menuControllerScript.menuOption;
            _menuOptions[menuOption].Unhighlight();

            if (_verticalInput > 0.1f)
            {
                if (menuOption == 0)
                {
                    menuOption = _menuOptions.Length - 1;
                }
                else
                {
                    menuOption--;
                }

                _timer = _delay;
                AudioManager.PlayScreenSpace(_menuControllerScript.optionChangeAudio);
            }
            else if (_verticalInput < -0.1f)
            {
                if (menuOption == _menuOptions.Length - 1)
                {
                    menuOption = 0;
                }
                else
                {
                    menuOption++;
                }

                _timer = _delay;
                AudioManager.PlayScreenSpace(_menuControllerScript.optionChangeAudio);
            }

            if (_uiState == UIState.Credits)
            {
               menuOption = 0;
            }
         
            _menuControllerScript.ChangeMenuOption(menuOption);
            lastHighlighted = menuOption;
            _menuOptions[menuOption].Highlight();
        }
        else
        {
            _timer -= Time.deltaTime;
        }
    }
    
    private void OnDisable()
    {
        _menuOptions[lastHighlighted].Unhighlight();
    }

    private void OnEnable()
    { 
        _menuControllerScript.ChangeMenuOption(0);
        _menuOptions[_menuControllerScript.menuOption].Highlight();

    }
}
