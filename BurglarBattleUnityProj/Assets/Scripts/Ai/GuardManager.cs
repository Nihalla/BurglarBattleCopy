using PlayerControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// General AI Managment with Utility Functions.
/// 
/// </summary>
//
// Note for development, if you are creating new functions for the GuardManager please make them static, call track() where needed, and always use _gm.X when calling internal functions or variables. 
public class GuardManager : MonoBehaviour
{
    [Header("Enable guard spawning, insterting of patrol points, getting of previous patrols. Data Heavy only enable if these features are required in this scene.\n Do not set during runtime.")]
    [SerializeField]
    private bool _extendedFeatures = false;
    private List<Guard> _guardsList = new List<Guard>();
    private List<Watcher> _watchers = new List<Watcher>();
    private List<PatrolPoint> _patrolPoints = new List<PatrolPoint>();
    [SerializeField]
    private GameObject GuardPrefab;

    [Space(10)]
    [Header("Teleport Positions")]
    [SerializeField] private List<Transform> _prisonPositionsTeamOneL1 = new List<Transform>();
    [SerializeField] private List<Transform> _prisonPositionsTeamTwoL1 = new List<Transform>();
    [SerializeField] private List<Transform> _prisonPositionsTeamOneL2 = new List<Transform>();
    [SerializeField] private List<Transform> _prisonPositionsTeamTwoL2 = new List<Transform>();

    [SerializeField] private List<Transform> _teamOneStartPos = new List<Transform>();
    [SerializeField] private List<Transform> _teamTwoStartPos = new List<Transform>();
    
    [SerializeField] private float _shieldPlayerForTime = 10f;

    //ToDo(Charles) Refactor the whole shield system to use player data instead of this list.
    private List<GameObject> _shieldedPlayers = new List<GameObject>();
    private List<GameObject> _ignoredPlayers = new List<GameObject>();

    private static GuardManager _gm;
    private static Scene _scene;

    public static List<PatrolPoint> s_patrolPoints => _gm._patrolPoints;

    private void Awake()
    {
        // REVIEW(Zack): we're never resetting the static instance of the GuardManager, so we're relying on the track(),
        // function to figure sort out the GuardManager instance everytime a function is called, which as more functions are added
        // this function could be forgotten to be added at the start of the new function, causing undefined behaviour.
        // Instead we could have a OnDestroy() function that simply checks if (_gm == this) _gm = null;,
        // and this would most likely do the same job without needing to check/fix the state of the static GuardManager,
        // for every function call.
        name = "[GuardManager]";
        GuardManager[] x = FindObjectsOfType<GuardManager>();
        if (x.Length > 1)
        {
            if (x[0] != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _gm = this;
            }
            return;
        }
        _gm = this; // This was missing - Norbert
    }

    /// <summary>
    /// Spawn any number of guards on a patrol path.
    /// Chooses the first patrol path in the list if Colour does not exist.
    /// 
    /// This function is static and does but requires a GuardManager to be in the scene.
    /// </summary>
    /// <param name="quantity">Number of guards to spawn</param>
    /// <param name="PatrolColour">Colour of patrol</param>
    /// <returns>A list of the guards created by the function.</returns>
    public static List<Guard> SpawnGuards(int quantity, Color PatrolColour)
    {
        if (!_gm._extendedFeatures)
        {
           // //Debug.Log("Please enable extended features on the Guard Manager to use this function.");
            return null;
        }
        List<Guard> createdGuards = new List<Guard>();
        for (int i = 0; i < _gm._patrolPoints.Count; ++i )
        {
            if (_gm._patrolPoints[i].patrolRouteColour == PatrolColour)
            {
                for (int j = 0; j < quantity; j++)
                {
                    GameObject tempGuard = Instantiate(_gm.GuardPrefab, _gm._patrolPoints[i].transform.position, Quaternion.identity);
                    tempGuard.GetComponent<PatrolComponent>().patrolColour = PatrolColour;
                    tempGuard.GetComponent<PatrolComponent>().currentPatrolPoint = _gm._patrolPoints[i];
                    createdGuards.Add(tempGuard.GetComponent<Guard>());
                }
                break;
                
            }
        }

        if (createdGuards.Count == 0)
        {
            for (int i = 0; i < quantity; i++)
            {
                GameObject tempGuard = Instantiate(_gm.GuardPrefab, _gm._patrolPoints[0].transform.position,
                    Quaternion.identity);
                tempGuard.GetComponent<PatrolComponent>().patrolColour = PatrolColour;
                tempGuard.GetComponent<PatrolComponent>().currentPatrolPoint = _gm._patrolPoints[0];
                createdGuards.Add(tempGuard.GetComponent<Guard>());
            }
        }

        return createdGuards;
    }
    
    /// <summary>
    /// Inserts a patrol point before the nearest patrol point in the path. Colour is case-sensitive. 
    /// This method is expensive and should be avoided if possible.
    /// 
    /// This function is static and does but requires a GuardManager to be in the scene.
    /// </summary>
    /// <param name="position">The position to create the patrol point.</param>
    /// <param name="PatrolColour">The colour of patrol to add the point to.</param>
    /// <returns></returns>
    public static GameObject InsertPatrolPoint(Vector3 position, Color PatrolColour)
    {
        if (!_gm._extendedFeatures)
        {
           // //Debug.Log("Please enable extended features on the Guard Manager to use this function.");
            return null;
        }
        GameObject pp = new GameObject("CodeSpawned PatrolPoint");
        pp.transform.position = position;
        pp.AddComponent<PatrolPoint>().patrolRouteColour = PatrolColour;

        PatrolPoint[] potentials = _gm._patrolPoints.ToArray();
        Array.Sort(potentials,
            delegate(PatrolPoint x, PatrolPoint y)
            {
                return Vector3.Distance(x.gameObject.transform.position, pp.transform.position)
                    .CompareTo(Vector3.Distance(y.gameObject.transform.position, pp.transform.position));
            });
        for (int i = 0; i < potentials.Length; ++i)
        {
            if ((potentials[i].patrolRouteColour == PatrolColour) && (potentials[i] != pp.GetComponent<PatrolPoint>()))
            {
                GetPreviousPatrolPoint(potentials[i]).Relink(pp.GetComponent<PatrolPoint>());
                pp.GetComponent<PatrolPoint>().Relink(potentials[i]);
                break;
            }
        }

        return pp;
    }

    /// <summary>
    /// Gets the previous patrol point of any given patrol point, provided it has one, as should always be the case.
    /// This method is expensive and should be avoided if possible.
    /// 
    /// This function is static and does but requires a GuardManager to be in the scene.
    /// </summary>
    /// <param name="pp">The Patrol Point you want to find the previous point of.</param>
    /// <returns>The Previous Point.</returns>
    public static PatrolPoint GetPreviousPatrolPoint(PatrolPoint pp)
    {
        if (!_gm._extendedFeatures)
        {
            return null;
        }

        PatrolPoint[] potentials = _gm._patrolPoints.ToArray();
        Array.Sort(potentials,
            delegate(PatrolPoint x, PatrolPoint y)
            {
                return Vector3.Distance(x.gameObject.transform.position, pp.transform.position)
                    .CompareTo(Vector3.Distance(y.gameObject.transform.position, pp.transform.position));
            });
        for (int i = 0; i < potentials.Length; ++i)
        {
            if (potentials[i].nextPatrolPoint == pp)
            {
                return potentials[i];
            }
        }
        Debug.LogError("Unlinked Patrol, please fix.");
        return pp;
    }
    
    /// <summary>
    /// Registers an AI type with the manager
    ///
    /// Currently registrable types:
    /// <ol>
    /// <li>PatrolPoint</li>
    /// <li>PatrolArea</li>
    /// <li>Guards</li>
    /// <li>Watcher</li>
    /// </ol>
    /// 
    /// This function is static and does but requires a GuardManager to be in the scene.
    /// </summary>
    /// <param name="registrableObject"></param>
    public static void Register(MonoBehaviour registrableObject)
    {
        if (Application.isPlaying)
        {
            
            if (registrableObject.GetType() == typeof(PatrolPoint) || registrableObject.GetType() == typeof(PatrolArea))
            {
                _gm._patrolPoints.Add((PatrolPoint)registrableObject);
            }
            else if (registrableObject.GetType() == typeof(Guard))
            {
                _gm._guardsList.Add((Guard)registrableObject);
            }
            else if (registrableObject.GetType() == typeof(Watcher))
            {
                _gm._watchers.Add((Watcher)registrableObject);
            }
        }
    }
    
    /// <summary>
    /// Removes an ai type from the manager.
    /// 
    /// This function is static and does but requires a GuardManager to be in the scene.
    /// </summary>
    /// <param name="registrableObject"></param>
    public static void Unregister(MonoBehaviour registrableObject)
    {
        if (Application.isPlaying)
        {
            
            if (registrableObject.GetType() == typeof(PatrolPoint) || registrableObject.GetType() == typeof(PatrolArea))
            {
                _gm._patrolPoints.Remove((PatrolPoint)registrableObject);
            }
            else if (registrableObject.GetType() == typeof(Guard))
            {
                _gm._guardsList.Remove((Guard)registrableObject);
            }
            else if (registrableObject.GetType() == typeof(Watcher))
            {
                _gm._watchers.Remove((Watcher)registrableObject);
            }
        }        
    }

    public static Transform GetPrisonPoint(FirstPersonController.PlayerTeam teamNumber, GuardLevel level)
    {
        switch (teamNumber)
        {
            case FirstPersonController.PlayerTeam.TEAM_ONE:
                if (level == GuardLevel.LEVEL_1)
                {
                    return _gm._prisonPositionsTeamOneL1[Random.Range(0, _gm._prisonPositionsTeamOneL1.Count)];
                }
                else
                {
                    return _gm._prisonPositionsTeamOneL2[Random.Range(0, _gm._prisonPositionsTeamOneL1.Count)];
                }

            case FirstPersonController.PlayerTeam.TEAM_TWO:
                if (level == GuardLevel.LEVEL_1)
                {
                    return _gm._prisonPositionsTeamTwoL1[Random.Range(0, _gm._prisonPositionsTeamOneL1.Count)];
                }
                else
                {
                    return _gm._prisonPositionsTeamTwoL2[Random.Range(0, _gm._prisonPositionsTeamOneL1.Count)];
                }

            default:
                break;
        }

        return null;
    }

    public static Transform GetPlayerRespawnPoint(FirstPersonController.PlayerTeam teamNumber) 
    {
        switch (teamNumber)
        {
            case FirstPersonController.PlayerTeam.TEAM_ONE:
                return _gm._teamOneStartPos[Random.Range(0, _gm._teamOneStartPos.Count)];

            case FirstPersonController.PlayerTeam.TEAM_TWO:
                return _gm._teamTwoStartPos[Random.Range(0, _gm._teamTwoStartPos.Count)];

            case FirstPersonController.PlayerTeam.UNKNOWN:
                break;
            default:
                break;
        }

        return null;
    }

    #region SHIELD_LOGIC
    /// <summary>
    /// Returns true if the player should be ignored.
    /// </summary>
    /// <param name="Player">A Player gameobject</param>
    /// <returns>True if the player should be ignored.</returns>
    public static bool PlayerIgnored(GameObject Player)
    {
        if (_gm._ignoredPlayers.Contains(Player))
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Returns whether the player is shielded.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool IsPLayerShielded(GameObject player)
    {
        return _gm._shieldedPlayers.Contains(player);
    }
    
    /// <summary>
    /// Adds a Shield to a player that will break after one hit, there is a four second grace period after being hit to run.
    /// </summary>
    /// <param name="Player">The player gameobject</param>
    /// <returns>0 if player has a Shield already, 1 if a shield has been added.</returns>
    /*public int AddShield(GameObject Player)
    {
        if (_gm._shieldedPlayers.Contains(Player))
        {
            return 0;
        }
        _gm._shieldedPlayers.Add(Player);
        return 1;
    }*/

    public static void AddShield(GameObject player)
    {
        if (_gm._shieldedPlayers.Contains(player)) return;
        _gm._shieldedPlayers.Add(player);
        _gm.StartCoroutine(_gm.ShieldRemove(_gm._shieldPlayerForTime, player));
    }

   

    /// <summary>
    /// To be called when a guard hits a players shield.
    /// </summary>
    /// <param name="Player"></param>
    /// <returns>
    /// <ul>
    /// <li> 1 if the player's shield has been broken and they are being ignored.
    /// <li> 0 if the player has no shield.
    /// <li> -1 if the player has a shield but is already being ignored.
    /// </ul>
    /// </returns>
    public static int OnShieldHit(GameObject Player)
    {
        if (_gm._shieldedPlayers.Contains(Player))
        {
            if (!_gm._ignoredPlayers.Contains(Player))
            {
             _gm._ignoredPlayers.Add(Player);
             _gm._shieldedPlayers.Remove(Player);
             _gm.StartCoroutine(_gm.IgnoreRemove(_gm._shieldPlayerForTime,Player));
             return 1;
            }
            return -1;
        }
        return 0;
    }
    
    private IEnumerator IgnoreRemove(float seconds, GameObject Player)
    {
        yield return new WaitForSeconds(seconds);
        _gm._ignoredPlayers.Remove(Player);
    }
    
    private IEnumerator ShieldRemove(float seconds, GameObject Player)
    {
        yield return new WaitForSeconds(seconds);
        _gm._shieldedPlayers.Remove(Player);
    }
    

    /// <summary>
    /// Attempts to add the specified Player to the list of ignored players.
    /// </summary>
    /// <param name="Player">The player to ignore</param>
    /// <returns>true if player has been added to ignored players, false if they were already ignored</returns>
    public static bool IgnorePlayer(GameObject Player)
    {
        if (!PlayerIgnored(Player))
        {
            _gm._ignoredPlayers.Add(Player);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to remove the specified Player from the list of ignored players.
    /// </summary>
    /// <param name="Player">The player to unignore</param>
    /// <returns>true if player has been removed from ignored players, false if they were not being ignored</returns>
    public static bool UnignorePlayer(GameObject Player)
    {
        if (PlayerIgnored(Player))
        {
            _gm._ignoredPlayers.Remove(Player);
            return true;
        }

        return false;
    }
    #endregion
}
