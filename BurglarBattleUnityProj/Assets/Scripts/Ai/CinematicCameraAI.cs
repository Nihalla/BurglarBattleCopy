// Author : Norbert Kupeczki - 19040948

using PlayerControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The purpose of the class is to provide 4 dynamic cameras that can zoom in on
/// puzzles or any items, and back once the interaction is completed.
/// On the object you want to zoom on needs to invoke either of these events:
/// <see cref="GlobalEvents.OnPlayerPuzzleInteract"/> <= To zoom in
/// <see cref="GlobalEvents.OnPlayerPuzzleExit"/> <= To zoom out
/// 
/// Even though it says puzzle, it can be used to zoom in and out from any game
/// objects, for example a sheet of paper on a table.
/// The object also has to have an empty that will serve as the zooming camera's
/// destination, this needs to be passed in to the interact event, its local Z will
/// determine where the camera will face once the movement is completed.
/// </summary>

public class CinematicCameraAI : MonoBehaviour
{
    [SerializeField] [Layer] private int[] _playerLayers = Array.Empty<int>();
    
    [SerializeField] private List<Camera> _zoomCameras;
    [SerializeField] private List<FirstPersonController> _players;

    private Vector3[] _origins = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    private Quaternion[] _rotations = new Quaternion[] { Quaternion.identity, Quaternion.identity, Quaternion.identity, Quaternion.identity };

    private delegate IEnumerator LerpIn_Del(int id, Transform finish);
    private delegate IEnumerator LerpOut_Del(int id, Action<int> callBackAction);

    private LerpIn_Del _lerpInFunc;
    private LerpOut_Del _lerpOutFunc;

    private Coroutine[] _lerpIn = new Coroutine[] { null, null, null, null };
    private Coroutine[] _lerpOut = new Coroutine[] { null, null, null, null };

    private const float LERP_SPEED = 8.0f;
    private const float LERP_TOLERANCE = 0.005f; // How close the lerping object has to be to its destination for the movement to be considered completed.

    private void Awake()
    {
        Debug.Assert(_playerLayers != null,    "Player layers isn't set up in the inspector!");
        Debug.Assert(_playerLayers.Length > 0, "Player layers isn't set up in the inspector!");
        
        _lerpInFunc = LerpIn;
        _lerpOutFunc = LerpOut;

        GlobalEvents.PlayerPuzzleInteract += OnPlayerCameraLerpIn;
        GlobalEvents.PlayerPuzzleExit += OnPlayerCameraLerpOut;
        GlobalEvents.PlayerJoinedTheGame += PlayerDeviceEnabled;
    }

    void Start()
    {
        _players = FindPlayers();
        InitCameras();
    }

    private void OnDestroy()
    {
        GlobalEvents.PlayerPuzzleInteract -= OnPlayerCameraLerpIn;
        GlobalEvents.PlayerPuzzleExit -= OnPlayerCameraLerpOut;
        GlobalEvents.PlayerJoinedTheGame -= PlayerDeviceEnabled;
    }

    private void OnPlayerCameraLerpIn(int id, Transform finish)
    {
        _lerpIn[id] = StartCoroutine(_lerpInFunc(id, finish));
    }

    private void OnPlayerCameraLerpOut(int id, Action<int> callBackAction)
    {

        _lerpOut[id] = StartCoroutine(_lerpOutFunc(id, callBackAction));
    }

    private IEnumerator LerpIn(int id, Transform finish)
    {
        // If the camera is zooming out, stopping that first to avoid locking up the camera
        if (_lerpOut[id] != null)
        {
            StopCoroutine(_lerpOut[id]);
        }

        // Store the original position and rotation of the camera before the zoom in
        _origins[id] = _players[id].GetPlayerCamera().transform.position;
        _rotations[id] = _players[id].GetPlayerCamera().transform.rotation;

        // Setting zoom-camera source rectangle to match the players view and swapping cameras
        _zoomCameras[id].rect = _players[id].GetPlayerCamera().rect;
        _players[id].GetPlayerCamera().enabled = false;
        _zoomCameras[id].enabled = true;

        // Positioning the replacement camera to match the position and rotation of the original
        _zoomCameras[id].transform.position = _players[id].GetPlayerCamera().transform.position;
        _zoomCameras[id].transform.rotation = _players[id].GetPlayerCamera().transform.rotation;

        // Start the lerping in
        while (Vector3.Distance(_zoomCameras[id].transform.position, finish.position) > LERP_TOLERANCE)
        {
            _zoomCameras[id].transform.position = Vector3.Lerp(_zoomCameras[id].transform.position, finish.position, LERP_SPEED * Time.deltaTime);
            _zoomCameras[id].transform.rotation = Quaternion.Lerp(_zoomCameras[id].transform.rotation, finish.rotation, LERP_SPEED * Time.deltaTime);
            yield return null;
        }

        // Once the position is close enough, snap on to the destination
        _zoomCameras[id].transform.position = finish.position;
        _zoomCameras[id].transform.rotation = finish.rotation;

    }

    private IEnumerator LerpOut(int id, Action<int> callBackAction)
    {
        // If the camera is zooming in, stopping that first to avoid locking up the camera
        if (_lerpIn[id] != null)
        {
            StopCoroutine(_lerpIn[id]);
        }

        // Start the lerping out
        while (Vector3.Distance(_zoomCameras[id].transform.position, _origins[id]) > LERP_TOLERANCE)
        {
            _zoomCameras[id].transform.position = Vector3.Lerp(_zoomCameras[id].transform.position, _origins[id], LERP_SPEED * Time.deltaTime);
            _zoomCameras[id].transform.rotation = Quaternion.Lerp(_zoomCameras[id].transform.rotation, _rotations[id], LERP_SPEED * Time.deltaTime);
            yield return null;
        }

        // Once the position is close enough, snap on to the destination
        _zoomCameras[id].transform.position = _origins[id];
        _zoomCameras[id].transform.rotation = _rotations[id];
        
        // Reset the temporary data storage of original position and rotation
        _origins[id] = Vector3.zero;
        _rotations[id] = Quaternion.identity;

        // Swapping cameras
        _players[id].GetPlayerCamera().enabled = true;
        _zoomCameras[id].enabled = false;

        callBackAction(id);
    }

    // Initialises four cameras to temporarily replace the player cameras when solving a puzzle
    // May need to extend the functionality of this method to copy more data from the player cameras!!!
    private void InitCameras()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject newCamera = new GameObject { name = "Camera " + i };
            newCamera.transform.SetParent(gameObject.transform);
            
            _zoomCameras.Add(newCamera.AddComponent<Camera>());
            _zoomCameras[i].enabled       = false;
            _zoomCameras[i].nearClipPlane = 0.01f;
            _zoomCameras[i].depth         = -1;
            _zoomCameras[i].allowHDR      = true;
            
            _zoomCameras[i].GetUniversalAdditionalCameraData().renderPostProcessing = true;
            _zoomCameras[i].GetUniversalAdditionalCameraData().volumeLayerMask      = int.MaxValue;

            for (int layerIndex = 0; layerIndex < _playerLayers.Length; layerIndex++)
            {
                int layer = _playerLayers[layerIndex];
                _zoomCameras[i].cullingMask &= ~(1 << layer);
            }
            
            // If the player with ID matching the value of i exists, the camera copies it's source rectangle.
            if (_players[i] != null)
            {
                _zoomCameras[i].rect = _players[i].GetPlayerCamera().rect;
            }
        }
    }

    // Finds all the players in the scene and returns a list of them in the 
    // order of their player IDs.

    private List<FirstPersonController> FindPlayers()
    {
        FirstPersonController[] results = new FirstPersonController[4];
        FirstPersonController[] players = FindObjectsOfType<FirstPersonController>();

        //Sorting the players based on their player ID
        foreach (FirstPersonController player in players)
        {
            results[player.gameObject.GetComponent<PlayerProfile>().GetPlayerID()] = player;
        }

        return results.ToList();
    }

    private void PlayerDeviceEnabled(int deviceID)
    {
        FirstPersonController[] players = FindObjectsOfType<FirstPersonController>();
        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].TryGetComponent(out PlayerProfile profile) &&
                profile.GetPlayerID() == deviceID)
            {
                _players[i] = players[i];
                _zoomCameras[i].rect = _players[i].GetPlayerCamera().rect;
            }
        }
    }
}
