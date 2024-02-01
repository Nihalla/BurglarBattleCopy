using PlayerControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Script should not be used outside of debugging.
/// </summary>
public class InputDeviceTracker : MonoBehaviour
{
    [SerializeField] private FourPlayerManager _manager;
    
    private bool _isActive = false;

    private void Awake()
    {
        if (_manager == null)
        {
            _manager = GetComponent<FourPlayerManager>();
        }
    }
    
    private void Start()
    {
        // If starting from a scene without pairing devices through the lobby, this will enable searching for devices
        if(InputDevices.CurrentDeviceCount < 1)
        {
            InputDevices.StartSearchForDevices();
            InputDevices.OnDevicePairedEvent += EnableDevice;
            _isActive = true;
        }
    }

    private void OnDestroy()
    {
        if (!_isActive) return;
        
        InputDevices.StopSearchForDevices();
        InputDevices.OnDevicePairedEvent -= EnableDevice;
        _isActive = false;
    }

    private void FixedUpdate()
    {
        // REVIEW (Norbert)
        // Instead of a late update, this could run from a coroutine that can stop
        // once all the players joined in, currently it runs even if all the players
        // are in, negatively impacting the performance.
        if(_isActive && InputDevices.CurrentDeviceCount == 4)
        {       
            InputDevices.StopSearchForDevices();
            InputDevices.OnDevicePairedEvent -= EnableDevice;
            _isActive = false;
        }
    }

    /// <summary>
    /// If there were no paired devices when the scene is opened when a user pairs a new device it will instantiate a new player.
    /// </summary>
    private void EnableDevice(int deviceIndex)
    {
        if (InputDevices.Devices[deviceIndex].IsDeviceKeyboard)
        {
            return;
        }

       // //Debug.Log("Device paired, instantiating player");

        // REVIEW (Norbert)
        // This spawns all the players to the same position, which may lead to undesired results.

        /*Instantiate(_manager.playerPrefab, _manager.spawnpoints[0].position, _manager.spawnpoints[0].rotation);
        GlobalEvents.OnPlayerJoinedTheGame(deviceIndex);*/

        GameObject newPlayer = Instantiate(_manager.playerPrefab, _manager.spawnpoints[deviceIndex].position, _manager.spawnpoints[deviceIndex].rotation);
        GlobalEvents.OnPlayerJoinedTheGame(deviceIndex);

        // we track the newly instantiated players position
        FourPlayerManager.TrackPlayerComponents(newPlayer.transform);
    }
}
