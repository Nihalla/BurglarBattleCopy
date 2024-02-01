// Author: Connor Easterbrook
// Team: Polymatrix

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// This is a manager class to control the Settings menu.
/// </summary>
public class UISettingsManager : MonoBehaviour
{
    public bool isSettingsOpen = false;
    private bool _onPrimaryButtons = true;
    private int _headingOption = 0;
    [SerializeField] private Button[] _primaryButtons;
    [SerializeField] private GameObject[] _subPanels;
    [SerializeField] private GameObject _subPanelCover;
    [SerializeField] private Button _mainOptionsButton;
    private Color _defaultColour;
    [SerializeField] private Color _selectedColour;
    private float _verticalInput;
    private float _timer = 0.0f;
    private float _delay = 0.2f;
    private bool _pressCheck = false;
    [SerializeField] private UIStateController _menuControllerScript;
    [SerializeField] private UIState _uiState;

    private Vector2 move;

    private async void OnEnable()
    {
        _defaultColour = _primaryButtons[0].GetComponentInChildren<TextMeshProUGUI>().color;
        _headingOption = 0;

        UIStateController.menuInputControllers.PlayerSelectEvent.AddListener(SelectInput);
        UIStateController.menuInputControllers.PlayerCancelEvent.AddListener(BackInput);

        await Task.Delay(250);
        isSettingsOpen = true;
        _primaryButtons[_headingOption].GetComponent<ButtonInfo>().Highlight();
        _primaryButtons[_headingOption].GetComponentInChildren<TextMeshProUGUI>().color = _selectedColour; // Make the text colour of the button selected if it is selected through headingOption
    }

    private void OnDisable()
    {
        UIStateController.menuInputControllers.PlayerSelectEvent.RemoveListener(SelectInput);
        UIStateController.menuInputControllers.PlayerCancelEvent.RemoveListener(BackInput);
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.activeSelf)
        {
            _verticalInput = _menuControllerScript.move.y;

            if(isSettingsOpen)
            {
                if(_onPrimaryButtons)
                {
                    GetPrimaryInput();
                }
            }
        }
    }

    /// <summary>
    /// Gets the input for the primary buttons
    /// </summary>
    private void GetPrimaryInput()
    {
        // If the user inputs a vertical direction then change the selected button
        if(_verticalInput != 0 && _uiState == _menuControllerScript.currentState)
        {
            IteratePrimaryButton();
        }
    }

    /// <summary>
    /// Iterates through the primary buttons and selects the next one
    /// </summary>
    private void IteratePrimaryButton()
    {
        if(_timer < 0)
        {
            _primaryButtons[_headingOption].GetComponentInChildren<TextMeshProUGUI>().color = _defaultColour;
            _primaryButtons[_headingOption].GetComponent<ButtonInfo>().Unhighlight();
            if(_verticalInput > 0)
            {
                _subPanels[_headingOption].SetActive(false);

                if(_headingOption > 0)
                {
                    _headingOption--;
                }
                else
                {
                    _headingOption = _primaryButtons.Length - 1;
                }

                _subPanels[_headingOption].SetActive(true);

                _timer = _delay;
                _pressCheck = true;
                AudioManager.PlayScreenSpace(_menuControllerScript.optionChangeAudio);
            }
            else if(_verticalInput < 0)
            {
                _subPanels[_headingOption].SetActive(false);

                if(_headingOption < _primaryButtons.Length - 1)
                {
                    _headingOption++;
                }
                else
                {
                    _headingOption = 0;
                }
                _subPanels[_headingOption].SetActive(true);

                _timer = _delay;
                _pressCheck = true;
            }

            _primaryButtons[_headingOption].GetComponentInChildren<TextMeshProUGUI>().color = _selectedColour;
            _primaryButtons[_headingOption].GetComponent<ButtonInfo>().Highlight();
            _menuControllerScript.menuOption = _headingOption;

            AudioManager.PlayScreenSpace(_menuControllerScript.optionChangeAudio);
        }
        else
        {
            _timer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Selects the primary button and disables the other buttons
    /// </summary>
    private void SelectInput()
    {
        if(_primaryButtons[_headingOption] == _primaryButtons[4])
        {
            DisableSettingsMenu();
            _menuControllerScript.SettingsChange();
        }
        else
        {
            //Debug.Log("Button Select");

            for(int i = 0; i < _primaryButtons.Length; i++)
            {
                if(i != _headingOption)
                {
                    _primaryButtons[i].interactable = false;
                }
            }
            _subPanelCover.SetActive(false);

            _onPrimaryButtons = false;
        }

        AudioManager.PlayScreenSpace(_menuControllerScript.optionSelectAudio);
    }

    /// <summary>
    /// Gets the input for the settings that the primary buttons allow you to access
    /// </summary>
    private void BackInput()
    {
        //Debug.Log("Secondary Input");
        if(!_onPrimaryButtons)
        {
            for(int i = 0; i < _primaryButtons.Length; i++)
            {
                _primaryButtons[i].interactable = true;
            }
            _subPanelCover.SetActive(true);

            _onPrimaryButtons = true;
        }
        else
        {
            DisableSettingsMenu();
            _menuControllerScript.SettingsChange();
        }
    }

    public void DisableSettingsMenu()
    {
        _primaryButtons[_headingOption].GetComponent<ButtonInfo>().Unhighlight();
        _subPanels[_headingOption].SetActive(false); // Make the panel active if it is selected through headingOption
        transform.parent.transform.parent.gameObject.SetActive(false);
        isSettingsOpen = false;
    }

}
