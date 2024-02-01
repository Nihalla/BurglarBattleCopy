using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace PlayerControllers
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance;
        private bool _isDestroyed = false;

        //bool for if the game is paused
        public static bool _MainGameIsPaused { get; set; }
        //insert options menu scene here
        [SerializeField] GameObject _Resumebutton;

        [SerializeField] private FirstPersonController _inputs;

        //pause menu getter so it can be activated and deactivated
        public GameObject _PauseMenuUI;
        [SerializeField] private List<GUIUpdater> _GUIMenuUI = new List<GUIUpdater>();
        private int _numPlayers = 0;
        private DeviceData _device;

        //[SerializeField] GameObject playerInteraction;

        //For the cycling of the menus
        private int _optionIndex = 0;
        private int _numOptions = 4;

        private Vector2 _uiMoveInputs;
        public float inputDeadzone = 0.1f;
        // Update is called once per frame

        [Header("Audio")]
        [SerializeField] private Audio _optionChangeAudio;
        [SerializeField] private Audio _optionSelectAudio;

        [SerializeField] private Temp_BGM_Manager _bgm;


        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _GUIMenuUI.Clear();
            GUIUpdater[] PlayerGUIUpdaters = FindObjectsOfType<GUIUpdater>();
            foreach(var player in PlayerGUIUpdaters)
            {
                if(_numPlayers <= 4)
                {
                    _GUIMenuUI.Add(player);
                    _numPlayers++;
                }
                else
                {
                    break;
                }
            }

            SetUpDevices();
        }

        public void SetDevice(int index)
        {
            _device = InputDevices.Devices[index];
        }

        public void AddGUIUpdater(GUIUpdater player)
        {
            if(_numPlayers > 4)
            {
                return;
            }

            _GUIMenuUI.Add(player);
            _numPlayers++;

            SetUpDevices();
        }

        public void SetUpDevices()
        {
            for(int i = 0; i < InputDevices.CurrentDeviceCount; i++)
            {
                InputDevices.Devices[i].Actions.MenuUI.Select.performed += ctx => { if(!_isDestroyed) ActivateOption(); };
                InputDevices.Devices[i].Actions.MenuUI.Start.performed += ctx => { if(!_isDestroyed) OnPause(); };
                InputDevices.Devices[i].Actions.MenuUI.Move.performed += ctx => { if(!_isDestroyed) OnUIMove(ctx.ReadValue<Vector2>()); };
                InputDevices.Devices[i].Actions.MenuUI.Move.canceled += ctx => { if(!_isDestroyed) OnUIMove(Vector2.zero); };
                InputDevices.Devices[i].Actions.MenuUI.Enable();
            }
        }

        public InputActions GetActions()
        {
            return _device.Actions;
        }

        private void OnPause()
        {
            if(_MainGameIsPaused)
            {
                for(int i = 0; i <= 2; i++)
                {
                    _PauseMenuUI.transform.GetChild(i).GetComponent<ButtonInfo>().Unhighlight();
                }
                ResumeGame();
            }
            else
            {
                _optionIndex = 0;

                PauseGame();
            }
        }

        private void OnUIMove(Vector2 inputs)
        {
            _uiMoveInputs = inputs;
            if(MathF.Abs(_uiMoveInputs.x) > inputDeadzone || MathF.Abs(_uiMoveInputs.y) > inputDeadzone)
            {
                if(MathF.Abs(_uiMoveInputs.x) > MathF.Abs(_uiMoveInputs.y))
                {
                    _optionIndex += (int)Mathf.Sign(_uiMoveInputs.x);
                }
                else
                {
                    _optionIndex -= (int)Mathf.Sign(_uiMoveInputs.y);
                }

                _optionIndex = (_optionIndex + _numOptions) % _numOptions;

                if(transform.GetChild(0).transform.gameObject.activeInHierarchy)
                {
                    AudioManager.PlayScreenSpace(_optionChangeAudio);
                }
            }

            switch(_optionIndex)
            {
                case 0:
                    // Handle option 0 Resmune Game 0
                    _PauseMenuUI.transform.GetChild(2).GetComponent<ButtonInfo>().Unhighlight();
                    _PauseMenuUI.transform.GetChild(1).GetComponent<ButtonInfo>().Unhighlight();
                    _PauseMenuUI.transform.GetChild(0).GetComponent<ButtonInfo>().Highlight();
                    break;
                case 1:
                    // Handle option 1 Options menu 2
                    _PauseMenuUI.transform.GetChild(0).GetComponent<ButtonInfo>().Unhighlight();
                    _PauseMenuUI.transform.GetChild(1).GetComponent<ButtonInfo>().Unhighlight();
                    _PauseMenuUI.transform.GetChild(2).GetComponent<ButtonInfo>().Highlight();
                    break;
                case 2:
                    // Handle option 2 Mainmenu 1
                    _PauseMenuUI.transform.GetChild(2).GetComponent<ButtonInfo>().Unhighlight();
                    _PauseMenuUI.transform.GetChild(1).GetComponent<ButtonInfo>().Highlight();
                    _PauseMenuUI.transform.GetChild(0).GetComponent<ButtonInfo>().Unhighlight();
                    break;
            }
        }

        private void ActivateOption()
        {
            if(_MainGameIsPaused)
            {
                if(transform.GetChild(0).transform.gameObject.activeInHierarchy)
                {
                    AudioManager.PlayScreenSpace(_optionSelectAudio);
                }

                switch(_optionIndex)
                {
                    case 0:
                        ResumeGame();
                        break;
                    case 1:
                        OpenOptionsMenu();
                        break;
                    case 2:
                        GoToMainMenu();
                        break;
                }
            }
        }

        //pauses time and sets the pause menu to be deactive
        public void ResumeGame()
        {
            _PauseMenuUI.SetActive(false);
            //_GUIMenuUI.SetActive(true);
            foreach (GUIUpdater player in _GUIMenuUI)
            { 
                player.gameObject.SetActive(true);
            }
            Time.timeScale = 1f;
            _MainGameIsPaused = false;

            if(transform.GetChild(0).transform.gameObject.activeInHierarchy)
            {
                AudioManager.PlayScreenSpace(_optionSelectAudio);
            }

            //_inputs.enabled = true;
            //playerInteraction.SetActive(true);
        }

        //pauses time and sets the pause menu to be active
        private void PauseGame()
        {
            //EventSystem.current.SetSelectedGameObject(null);
            //EventSystem.current.SetSelectedGameObject(_Resumebutton);
            _PauseMenuUI.SetActive(true);

            foreach (GUIUpdater player in _GUIMenuUI)
            { 
                player.gameObject.SetActive(false);
            }

            Time.timeScale = 0f;
            _MainGameIsPaused = true;
            _PauseMenuUI.transform.GetChild(0).GetComponent<ButtonInfo>().Highlight();
            //_inputs.enabled = false;
            //playerInteraction.SetActive(false);
        }

        //sets the options menu to active
        public void OpenOptionsMenu()
        {
            return;
            //options.SetActive(true);
            //_PauseMenuUI.SetActive(false);
        }

        //loads the main menu scene
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            _MainGameIsPaused = false;
            gameObject.SetActive(false);
            UIStateController.introduced = true;

            SceneManager.LoadScene("Main Menu Scene");
        }

        private void CleanUpDevices()
        {
            for(int i = 0; i < InputDevices.CurrentDeviceCount; i++)
            {
                InputDevices.Devices[i].Actions.MenuUI.Select.performed -= ctx => ActivateOption();
                InputDevices.Devices[i].Actions.MenuUI.Start.performed -= ctx => OnPause();
                InputDevices.Devices[i].Actions.MenuUI.Move.performed -= ctx => OnUIMove(ctx.ReadValue<Vector2>());
                InputDevices.Devices[i].Actions.MenuUI.Move.canceled -= ctx => OnUIMove(Vector2.zero);
                InputDevices.Devices[i].Actions.MenuUI.Disable();
            }
        }

        private void OnDestroy()
        {
            CleanUpDevices();
            _isDestroyed = true;
        }
    }
}




