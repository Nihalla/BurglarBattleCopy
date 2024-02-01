using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using PlayerControllers;
using UnityEngine.InputSystem;

public class PlayerProfile : MonoBehaviour
{
    [SerializeField] private int _playerID = -1;
    [SerializeField] private FirstPersonController.PlayerTeam _team = FirstPersonController.PlayerTeam.UNKNOWN;
    private FourPlayerManager _manager;
    private GameObject _parent;
    private FirstPersonController _controller;
    private Camera _cam;

    public FirstPersonController playerController => _controller;
    public FirstPersonController.PlayerTeam Team  => _team;
    
    private void Awake()
    {
        _manager = FindObjectOfType<FourPlayerManager>();
        _controller = GetComponent<FirstPersonController>();
        _playerID = _manager.GetPlayerID(this);
        _team = _manager.AssignTeam(_playerID);
        _cam = GetComponentInChildren<Camera>();
        UpdateFPC();    
    }

    private void Start()
    {
        //Camera Rects are now assigned with base on Global Lobby Data indexes rather than player IDs to reflect the correct player's positions
        
        int camIndex = _playerID;
        if (!GlobalLobbyData.isListEmpty())
        {
            camIndex = GlobalLobbyData.s_deviceIDPair.IndexOf(_playerID);
        }
        SetCamRect(_manager.GetCameraPosition(camIndex));
    }

    /// <summary>
    /// Updates the values for PlayerID and Team on the FirstPersonController
    /// </summary>
    private void UpdateFPC()
    {
        _controller.SetTeam(_team);
        _controller.SetPlayerID(_playerID);

    }

    public void SetPlayerID(int newID)
    {
        _playerID = newID;
        _controller.SetPlayerID(_playerID);
    }
    
    public void SetTeam(FirstPersonController.PlayerTeam newTeam)
    {
        _team = newTeam;
        _controller.SetTeam(newTeam);
    }
    
    public void SetCamRect(Vector2 position) 
    {
        _cam.rect = new Rect(position.x, position.y, 0.5f, 0.5f);
    }
    
    public int GetPlayerID()
    {
        return _playerID;
    }
    
    public FirstPersonController.PlayerTeam GetTeam()
    {
        return _team;
    }
    
    public void Disconnect()
    {
        Debug.LogError("User #" + gameObject.GetComponent<PlayerInput>().user.id + " disconnected with their device - " + gameObject.GetComponent<PlayerInput>().GetDevice<InputDevice>());
    }
    
    public void Reconnect()
    {
        Debug.LogError("User #" + gameObject.GetComponent<PlayerInput>().user.id + " has reconnected");
    }
    
    public FirstPersonController GetPlayer()
    {
        return _controller;
    }
}
