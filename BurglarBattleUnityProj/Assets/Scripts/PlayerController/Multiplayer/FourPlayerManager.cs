using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cinemachine;
using UnityEngine;
using PlayerControllers;
using UnityEngine.InputSystem;

public class FourPlayerManager : MonoBehaviour
{
    public List<Transform> spawnpoints = new List<Transform>();
    [SerializeField] private List<Vector2> _cameraPos = new List<Vector2>();
    public int maxPlayers = 4;
    public GameObject playerPrefab;

    [SerializeField] private List<PlayerProfile> _players = new List<PlayerProfile>();
    private Dictionary<FirstPersonController.PlayerTeam, int> _scores = new Dictionary<FirstPersonController.PlayerTeam, int>();
    private Dictionary<int , FirstPersonController.PlayerTeam> _teams = new Dictionary<int, FirstPersonController.PlayerTeam>();
    private int _teamOneMembers = 0;
    private int _teamTwoMembers = 0;
    private bool _inSceneTesting = false;
    
    /// <summary> Get all player profiles in the scene. </summary>
    public List<PlayerProfile> Players => _players;

    // NOTE(Zack): we're caching the components for the players, so that we don't have to do GetComponents on components
    // that are accessed frequently;
    // they are all hardcoded to [4] as this is the max player count;
    private static Transform[] _playerTransforms                 = new Transform[4]; 
    private static FirstPersonController[] _playerFPSControllers = new FirstPersonController[4];
    public static Transform[] PlayerTransforms           => _playerTransforms;
    public static FirstPersonController[] FPSControllers => _playerFPSControllers;

    public static int InstantiatedPlayerCount { get; private set; } = 0;

    private void Awake() 
    {
        _inSceneTesting = GlobalLobbyData.isListEmpty();
        _scores.Add(FirstPersonController.PlayerTeam.TEAM_ONE, 0);
        _scores.Add(FirstPersonController.PlayerTeam.TEAM_TWO, 0);
        _scores.Add(FirstPersonController.PlayerTeam.UNKNOWN, -1);

        // Only instantiates all 4 players if there is at least one paired device before instantiation.
        if (InputDevices.CurrentDeviceCount > 0)
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                GameObject p = Instantiate(playerPrefab, spawnpoints[i].position, spawnpoints[i].rotation);
                TrackPlayerComponents(p.transform);
            }
        }     
    }

    private void OnDestroy()
    {
        // NOTE(Zack): we set the transforms to null so that the garbage collector can reclaim the memory when we no longer need it
        for (int i = 0; i < PlayerTransforms.Length; ++i)
        {
            PlayerTransforms[i] = null;
            FPSControllers[i] = null;
        }

        InstantiatedPlayerCount = 0;
    }

    /// <summary>
    /// Assigns a new player with a player ID and adds it to the teams dictionary without assigning it a team.
    /// If an already existent player is called, then it will return its ID according to its index in the player list
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <returns></returns>
    public int GetPlayerID(PlayerProfile newPlayer)
    {
        if(!_players.Contains(newPlayer))
        {
            _players.Add(newPlayer);
            if (!_teams.ContainsKey(_players.Count - 1))
            {
                _teams.Add(_players.Count - 1, FirstPersonController.PlayerTeam.UNKNOWN);
            }
            else
            {
                _teams[_players.Count - 1] = FirstPersonController.PlayerTeam.UNKNOWN;
            }
            if(_inSceneTesting)
            {
                return _players.Count - 1;
            }
            return GlobalLobbyData.GetID(_players.Count - 1);
        }
        else
        {
            return newPlayer.GetPlayerID();
        }
    }
    /// <summary>
    /// Temporary function that assigns a Vector representing a spawn point for the player to spawn in.
    /// This should be replaced at a later date according to what is needed.
    /// Currently players cannot overlap on spawn, so unique spawnpoints are needed.
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public Vector3 GetSpawnPoint(int playerID)
    {
        if(playerID < 0 || playerID > spawnpoints.Count)
        {
            return spawnpoints[0].position;
        }
        else
        {
            return spawnpoints[playerID].position;
        }
    }
    /// <summary>
    /// While no lobby system is in place to better allocate the teams, this function will handle team assignment.
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns></returns>
    public FirstPersonController.PlayerTeam AssignTeam(int playerID)
    {
        int index = playerID;
        if (!_inSceneTesting)
        {
            index = GlobalLobbyData.s_deviceIDPair.IndexOf(playerID);
        }
            switch (index)
            {
                case 0:
                    _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_ONE;
                    return FirstPersonController.PlayerTeam.TEAM_ONE;
                case 1:
                    _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_ONE;
                    return FirstPersonController.PlayerTeam.TEAM_ONE;
                case 2:
                    _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_TWO;
                    return FirstPersonController.PlayerTeam.TEAM_TWO;
                case 3:
                    _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_TWO;
                    return FirstPersonController.PlayerTeam.TEAM_TWO;    
            }
        /*
        if(_teams.ContainsKey(playerID))
        {
            if (_teamOneMembers < maxPlayers/2)
            {
                _teamOneMembers++;
                _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_ONE;
                //Debug.Log(_teams[_players.Count - 1]);
                return FirstPersonController.PlayerTeam.TEAM_ONE;
            }
            else if (_teamTwoMembers < maxPlayers/2)
            {
                _teamTwoMembers++;
                _teams[playerID] = FirstPersonController.PlayerTeam.TEAM_TWO;
                //Debug.Log(_teams[_players.Count - 1]);
                return FirstPersonController.PlayerTeam.TEAM_TWO;
            }
        }
        */
        return FirstPersonController.PlayerTeam.UNKNOWN;
    }
    /// <summary>
    /// This function will return the current score of a team.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int CheckScores(FirstPersonController.PlayerTeam team)
    {
        if(_scores.ContainsKey(team))
        {
            return (_scores[team]);
        }
        else
        {
            return -1;
        }
    }
    /// <summary>
    /// This function will return the current score of a team by first checking what team a player is part of.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public int CheckScores(int playerID)
    {
        if(playerID > -1 && playerID < _players.Count)
        {
            return (_scores[_players[playerID].GetTeam()]);
        }
        else
        {
            return -1;
        }
    }
    /// <summary>
    /// Adds a value to a teams score. Use a negative value to decrease points instead
    /// </summary>
    /// <param name="team"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    public int AddScore(FirstPersonController.PlayerTeam team, int points)
    {
        if(_scores.ContainsKey(team))
        {
            return -1;
        }
        _scores[team] += points;
        if(_scores[team] < 0)
        {
            _scores[team] = 0;
        }
        return _scores[team];
    }
    /// <summary>
    /// Adds a value to a teams score. Use a negative value to decrease points instead
    /// </summary>
    /// <param name="team"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    public int AddScore(int playerID, int points)
    {
        if (playerID < 0 || playerID > _players.Count)
        {
            return -1;
        }
        FirstPersonController.PlayerTeam team = _teams[playerID];
        return AddScore(team, points);
    }
    /// <summary>
    /// Get a specific camera position which is needed for setting up player camera positions on instantiation
    /// </summary>
    /// <param name="playerID"></param>
    /// <returns name="cameraPosition"></returns>
    public Vector2 GetCameraPosition(int ID)
    {
        return _cameraPos[ID];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TrackPlayerComponents(Transform player)
    {
        Debug.Assert(InstantiatedPlayerCount < PlayerTransforms.Length, "We're trying to track more than 4 players");

        PlayerTransforms[InstantiatedPlayerCount] = player;
        FPSControllers[InstantiatedPlayerCount]   = player.GetComponent<FirstPersonController>();

        InstantiatedPlayerCount += 1;
    }
}
