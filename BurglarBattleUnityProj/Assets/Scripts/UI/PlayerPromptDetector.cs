using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using TMPro;

public enum DeviceTeamStatus
{
    TEAM_ONE = 1,
    NONE = 0,
    TEAM_TWO = 2
}
public class PlayerPromptDetector : MonoBehaviour
{
    [SerializeField] private GameObject[] _playerLabels = new GameObject[4];
    [SerializeField] private GameObject[] _playerTeamObj = new GameObject[4];
    [SerializeField] private GameObject[] _playerJoinButtons = new GameObject[4];
    [SerializeField] private TMP_Text[] _playerIdentifierText = new TMP_Text[4];
    [SerializeField] private float[] _buttonPositions = new float[3];
    private List<int> _connectedDeviceIDs = new List<int>();
    private Dictionary<int, DeviceTeamStatus> _teamsStatus = new Dictionary<int, DeviceTeamStatus>();
    private Vector2 _uiMovement;
    public GameObject playButton;
    public DelayedButtonPress delayedButtonScript;

    void Start()
    {
        InputDevices.StartSearchForDevices();
        InputDevices.OnDevicePairedEvent += OnDevicePaired;
        for (int i = 0; i < InputDevices.MAX_DEVICE_COUNT; i++)
        {
            InputDevices.RemoveDevice(i);
        }
        for (int i = 0; i < _playerJoinButtons.Length; i++)
        {
            _playerJoinButtons[i].SetActive(true);
        }
    }
    private void Update()
    {
        //CheckforDevices();
        if(CheckTeamValidity())
        {
            playButton.GetComponent<Button>().interactable = true;
            //delayedButtonScript.ChangeButton(playButton);
            playButton.GetComponent<Button>().Select();
        }
        else
        {
            playButton.GetComponent<Button>().interactable = false;
        }
    }
    private void FixedUpdate()
    {
        
    }
    public void ResetDevices()
    {
        for (int i = 0; i <= InputDevices.CurrentDeviceCount; i++)
        {
            InputDevices.RemoveDevice(i);
            _playerJoinButtons[i].SetActive(true);
            _playerLabels[i].SetActive(false);
        }
    }
    public void CheckforDevices()
    {
        for (int i = 0; i < InputDevices.MAX_DEVICE_COUNT; i++)
        {
            if (InputDevices.Devices[i] == null)
            {
                _playerJoinButtons[i].SetActive(true);
                _playerLabels[i].SetActive(false);
                continue;
            }
            if (InputDevices.Devices[i] != null)
            {
                _playerJoinButtons[i].SetActive(false);
                _playerLabels[i].SetActive(true);
                continue;
            }
        }
    }
    public void OnDevicePaired(int deviceID)
    {
        if (InputDevices.CurrentDeviceCount >= InputDevices.MAX_DEVICE_COUNT)
        {
            Debug.LogError("Too many players");
            InputDevices.StopSearchForDevices();
        }

        InputDevices.Devices[deviceID].Actions.MenuUI.Move.performed += OnMove;
        InputDevices.Devices[deviceID].Actions.MenuUI.Move.canceled += OnMove;
        _connectedDeviceIDs.Add(deviceID);
        _playerJoinButtons[deviceID].SetActive(false);
        _playerLabels[deviceID].SetActive(true);

        if(!_teamsStatus.ContainsKey(InputDevices.Devices[deviceID].Device.deviceId))
        {
            _teamsStatus.Add(InputDevices.Devices[deviceID].Device.deviceId, DeviceTeamStatus.NONE);
        }
    }
    public void OnLobbyConfirm()
    {
        int[] keyArray = new int[4];
        _teamsStatus.Keys.CopyTo(keyArray, 0);
        for (int i = 0; i < _teamsStatus.Count; i++)
        {
            switch(_teamsStatus[keyArray[i]])
            {
                case DeviceTeamStatus.TEAM_ONE:
                    GlobalLobbyData.AddStartID(i);
                    break;
                case DeviceTeamStatus.TEAM_TWO:
                    GlobalLobbyData.AddID(i);
                    break;
                case DeviceTeamStatus.NONE:
                    break;
            }
        }
    }
    public bool CheckTeamValidity()
    {
        if(_teamsStatus.Count == 4)
        {
            int teamOne = 0;
            int teamTwo = 0;
            int[] keyArray = new int[4];
            _teamsStatus.Keys.CopyTo(keyArray, 0);
            for (int i = 0; i < _teamsStatus.Keys.Count; i++)
            {
                switch (_teamsStatus[keyArray[i]])
                {
                    case DeviceTeamStatus.TEAM_ONE:
                        teamOne++;
                        break;
                    case DeviceTeamStatus.TEAM_TWO:
                        teamTwo++;
                        break;
                    case DeviceTeamStatus.NONE:
                        return false;
                }
            }

            if(teamOne == 2 && teamTwo == 2)
            {
                return true;
            }
        }
        return false;
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            int _deviceID = GetConnectedID(ctx.control.device.deviceId);
            _uiMovement = ctx.ReadValue<Vector2>();
            int _currentDeviceID = ctx.control.device.deviceId;
            if(_uiMovement.x < -0.9f)
            {
                //change state to left
                switch (_teamsStatus[_currentDeviceID])
                {
                    case DeviceTeamStatus.TEAM_ONE:
                        break;
                    case DeviceTeamStatus.NONE:
                        _teamsStatus[_currentDeviceID] = DeviceTeamStatus.TEAM_ONE;
                        Color team1 = new Color(0,90,255,255);
                        _playerTeamObj[_deviceID].GetComponent<Image>().color = Color.blue;
                        break;
                    case DeviceTeamStatus.TEAM_TWO:
                        _teamsStatus[_currentDeviceID] = DeviceTeamStatus.TEAM_ONE;
                        Color team2 = new Color(0, 90, 255, 255);
                        _playerTeamObj[_deviceID].GetComponent<Image>().color = Color.blue;
                        break;
                }
            }
            else if(_uiMovement.x > 0.9f)
            {
                //change state to right
                switch (_teamsStatus[_currentDeviceID])
                {
                    case DeviceTeamStatus.TEAM_ONE:
                        _teamsStatus[_currentDeviceID] = DeviceTeamStatus.TEAM_TWO;
                        _playerTeamObj[_deviceID].GetComponent<Image>().color = Color.red;
                        break;
                    case DeviceTeamStatus.NONE:
                        _teamsStatus[_currentDeviceID] = DeviceTeamStatus.TEAM_TWO;
                        _playerTeamObj[_deviceID].GetComponent<Image>().color = Color.red;
                        break;
                    case DeviceTeamStatus.TEAM_TWO:      
                        break;
                }
            }
            TeamAllignment(_deviceID, ((int)_teamsStatus[_currentDeviceID]));
        }
        else if (ctx.canceled)
        {
            _uiMovement = Vector2.zero;
        }

    }
    public void EndSearch()
    {
        InputDevices.StopSearchForDevices();
    }
    public int GetConnectedID(int uniqueID)
    {
        for (int i = 0; i < InputDevices.CurrentDeviceCount; i++)
        {
            if (InputDevices.Devices[i].Device.deviceId == uniqueID)
            {
                return i;
            }
        }
        return -1;
    }
    private void OnDestroy()
    {
        InputDevices.OnDevicePairedEvent -= OnDevicePaired;
        int deviceID = 0;
        for (int i = 0; i < _connectedDeviceIDs.Count; i++)
        {
            deviceID = _connectedDeviceIDs[i];
            InputDevices.Devices[deviceID].Actions.MenuUI.Move.performed -= OnMove;
            InputDevices.Devices[deviceID].Actions.MenuUI.Move.canceled -= OnMove;
        }
    }
    private void TeamAllignment(int deviceID, int team)
    {
        RectTransform rectTransform = _playerTeamObj[deviceID].GetComponent<RectTransform>();
        RectTransform textRectTransform = _playerIdentifierText[deviceID].GetComponent<RectTransform>();
        if(rectTransform)
        {
            rectTransform.localPosition = new Vector3(_buttonPositions[team], rectTransform.localPosition.y, rectTransform.localPosition.z);
            if (team == 1) 
            { 
                rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
                textRectTransform.localRotation = Quaternion.Euler(0, 0, 0);
                _playerLabels[deviceID].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
                _playerTeamObj[deviceID].transform.Find("ArrowLeft").gameObject.SetActive(false);
                _playerTeamObj[deviceID].transform.Find("ArrowRight").gameObject.SetActive(true);
            }
            else if (team == 2) 
            { 
                rectTransform.localRotation = Quaternion.Euler(0, 180, 0);
                textRectTransform.localRotation = Quaternion.Euler(0, 180, 0);
                _playerLabels[deviceID].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 180, 0);
                _playerTeamObj[deviceID].transform.Find("ArrowLeft").gameObject.SetActive(false);
                _playerTeamObj[deviceID].transform.Find("ArrowRight").gameObject.SetActive(true);
            }
            Debug.LogError(rectTransform.localPosition);
        }
        
    }
}