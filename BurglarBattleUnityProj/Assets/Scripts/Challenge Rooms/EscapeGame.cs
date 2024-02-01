using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscapeGame : MonoBehaviour
{


    [SerializeField] private Collision _mainBlock;
    [SerializeField] private int _force;
    [SerializeField] private Material _activeMaterial;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private List<GameObject> _moveBlocks;
    [SerializeField] private List<MeshRenderer> _quadMeshRenderers;
    [SerializeField] private List<int> _blockLength;
    [SerializeField] private Transform _puzzlePos;
    private List<List<MeshRenderer>> _quadMeshes2D = new List<List<MeshRenderer>>();

    [SerializeField] private List<MeshRenderer> _wireMeshRenderers;
    [SerializeField] private Material _liveWireMaterial;
    [SerializeField] private GameObject _mainBlockFound;

    [SerializeField] private GameObject _mainBlockNotFound;
    [SerializeField] private GameObject _mainBlockRemaining;
    [SerializeField] private int _puzzleIndex;
    [SerializeField] private AudioSource3D _grindingStone;
    private bool _ready = false;
    private bool _completed = false;

    public event Action SwitchToReadyPuzzle;

    public event Action PuzzleStarted;

    private int _blockSelected = 0;
    private Rigidbody _selectedRigidbody;

    private bool _activated = false;

    private DeviceData _deviceData;

    private Vector2 _move;

    private int _playerIndex;

    // Start is called before the first frame update
    private void Start()
    {
        int quad = 0;
        for (int i = 0; i < _moveBlocks.Count; i++)
        {
            List<MeshRenderer> _blockGroup = new List<MeshRenderer>();
            for (int j = 0; j < _blockLength[i]; j++)
            {
                _blockGroup.Add(_quadMeshRenderers[quad]);
                quad++;
            }

            _quadMeshes2D.Add(_blockGroup);
        }

        for (int i = 0; i < _quadMeshes2D[_blockSelected].Count; i++)
        {
            _quadMeshes2D[_blockSelected][i].material = _activeMaterial;
        }

        SwitchToReadyPuzzle += SwitchToReady;
        GlobalEvents.VaultPuzzles += ExitPuzzle;
        PuzzleStarted += ButtonPressed;
       // //Debug.Log("added listener");

    }

    private void OnDestroy()
    {
        GlobalEvents.VaultPuzzles -= ExitPuzzle;
        PuzzleStarted -= ButtonPressed;
        SwitchToReadyPuzzle -= SwitchToReady;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_activated)
        {


            _move = _deviceData.Actions.VaultPuzzle.Move.ReadValue<Vector2>();


            if (_move.x > 0.01 || _move.x < -0.01)
            {
                _selectedRigidbody = _moveBlocks[_blockSelected].GetComponent<Rigidbody>();
                _selectedRigidbody.AddRelativeForce(new Vector2(_move.x * _force, 0f));
                _grindingStone.Play();
            }
            

            if (_move.y > 0.01 || _move.y < -0.01)
            {
                _selectedRigidbody = _moveBlocks[_blockSelected].GetComponent<Rigidbody>();
                _selectedRigidbody.AddRelativeForce(new Vector2(0f, _move.y * _force));
                _grindingStone.Play();
            }

            if (!(_move.y > 0.01 || _move.y < -0.01) && !(_move.x > 0.01 || _move.x < -0.01)) 
            {
                _grindingStone.Stop();
            }

           
        }
    }

    private void ButtonPressed()
    {
        if (!_activated)
        {
            GlobalEvents.OnPlayerPuzzleInteract(_playerIndex, _puzzlePos);

            _deviceData = InputDevices.Devices[_playerIndex];
            _deviceData.Actions.VaultPuzzle.SelectUp.performed += ctx => SelectedUp();
            _deviceData.Actions.VaultPuzzle.SelectDown.performed += ctx => SelectedDown();
            _deviceData.Actions.VaultPuzzle.Exit.performed += ctx => ExitPuzzle(_puzzleIndex);
            _deviceData.Actions.PlayerController.Disable();
            _deviceData.Actions.VaultPuzzle.Enable();
            _activated = true;

            print("swapped");
        }
    }

    private void SelectedUp()
    {

        for (int i = 0; i < _quadMeshes2D[_blockSelected].Count; i++)
        {
            _quadMeshes2D[_blockSelected][i].material = _defaultMaterial;
        }
        
        if (_blockSelected != _moveBlocks.Count - 1)
        {
            _blockSelected++;
        }
        else
        {
            _blockSelected = 0;
        }


        for (int i = 0; i < _quadMeshes2D[_blockSelected].Count; i++)
        {
            _quadMeshes2D[_blockSelected][i].material = _activeMaterial;
        }
    }

    private void SelectedDown()
    {

        for (int i = 0; i < _quadMeshes2D[_blockSelected].Count; i++)
        {
            _quadMeshes2D[_blockSelected][i].material = _defaultMaterial;
        }
        if (_blockSelected != 0)
        {
            _blockSelected--;
        }
        else
        {
            _blockSelected = _moveBlocks.Count - 1;
        }

        for (int i = 0; i < _quadMeshes2D[_blockSelected].Count; i++)
        {
            _quadMeshes2D[_blockSelected][i].material = _activeMaterial;
        }
    }

    private void ExitPuzzle(int puzzleindex)
    {
        if (puzzleindex == _puzzleIndex)
        {
            ////Debug.Log("successful exit" );
            GlobalEvents.OnPlayerPuzzleExit(_playerIndex, RegainPlayerControls);
        }
    }

    private void RegainPlayerControls(int _playerID)
    {

        _deviceData.Actions.VaultPuzzle.Disable();
        _deviceData.Actions.PlayerController.Enable();
        _deviceData.Actions.VaultPuzzle.SelectUp.performed -= ctx => SelectedUp();
        _deviceData.Actions.VaultPuzzle.SelectDown.performed -= ctx => SelectedDown();
        _deviceData.Actions.VaultPuzzle.Exit.performed -= ctx => ExitPuzzle(_puzzleIndex);
        _activated = false;

    }

    private void SetWireToLive()
    {
        for (int i = 0; i < _wireMeshRenderers.Count; i++)
        {
            _wireMeshRenderers[i].material = _liveWireMaterial;

        }
    }

    public void OnPuzzleComplete()
    {
       // //Debug.Log("exitvault puzzle");
        SetWireToLive();
        _completed = true;

        GlobalEvents.OnVaultPuzzles(_puzzleIndex);
    }

    public int VaultPuzzlePlayer
    {
        get { return _playerIndex; }

        set { _playerIndex = value; }
    }

    public bool GetVaultPuzzleActivated()
    {

        return _activated;

    }

    public void StartVaultPuzzle()
    {
        if (_ready)
        {
            PuzzleStarted?.Invoke();
        }
    }

    private void SwitchToReady()
    {
        _mainBlockNotFound.SetActive(false);
        _mainBlockRemaining.SetActive(false);
        _mainBlockFound.SetActive(true);
        _ready = true;
    }

    public void OnSwitchToReadyPuzzle()
    {
        SwitchToReadyPuzzle?.Invoke();
    }



    public bool GetVaultPuzzleCompleted()
    {
        return _completed;
    }
}