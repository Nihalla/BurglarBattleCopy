using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CascadeGame;
using UnityEngine.Events;

namespace CascadeGame
{
    public class CascadeManager : MonoBehaviour, IInteractable
    {
        //Gameobjects used in the minigame
        [Header("Cascade GameObjects")]
        [SerializeField] private Cascade[] _cascades;

        [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
        private MeshRenderer _meshRenderer => _meshRenderers[0];

        [SerializeField] private Material _defaultMat;
        [SerializeField] private Material _hoverMat;
        [SerializeField] private Material _holdMat;

        [SerializeField] private Transform _lootLocation;
        [SerializeField] private GameObject chestLoot;
        [SerializeField] private LootSelector lootType;

        [HideInInspector] public bool requiresPlayerRef = true;

        // Unity Event
        [Header("Unity Event")]
        public UnityEvent OnChallengeCompleteEvent;

        //Parameters for changing Cascade game rules
        #region Minigame Parameters
        [Header("Cascade Parameters")]
        [SerializeField] private float _defaultBeadSpeed;
        [SerializeField] private float _defaultHitWindowBuffer = 0.15f;
        [Range(0, 1)]
        [SerializeField] private float _defaultHitWindowPos = 0.5f;
        [Range(0, 0.5f)]
        [SerializeField] private float _defaultHitWindowSize = 0.15f;
        #endregion

        //Paremters for tracking information in the minigame
        #region Tracking Parameters
        private int _noOfCascades;
        private int _currentCascade = 0;
        private MiniGameState _miniGameState;
        #endregion

        private void Awake()
        {
            InitCascadeGame();
        }

        public Span<MeshRenderer> GetInteractionMeshRenderers()
        {
            return _meshRenderers.AsSpan();
        }

        public void OnInteractHoverStarted()
        {
            _meshRenderer.sharedMaterial = _hoverMat;
        }

        public void OnInteractHoverEnded()
        {
            _meshRenderer.sharedMaterial = _defaultMat;
        }

        public void OnInteract(PlayerInteraction invokingPlayerInteraction)
        {
            if (_miniGameState == MiniGameState.INACTIVE)
            {
                _miniGameState = MiniGameState.RUNNING;
            }
            else if (_miniGameState == MiniGameState.RUNNING)
            {
                _cascades[_currentCascade].CheckWinCondition();
            }
            else if (_miniGameState == MiniGameState.END)
            {
                
            }
        }

        /*    private void SetLockpickPlayerID(int ID)
            {
                _lockpickPlayerID = ID;
                //Debug.Log(ID);
            }

            private void SetUpDevice()
            {
                for (int i = 0; i < InputDevices.CurrentDeviceCount; i++)
                {
                    InputDevices.Devices[i].Actions.PlayerInteraction.Interact.performed += ctx => SetLockpickPlayerID(i - 1);
                }
            }*/

        private void Update()
        {
            MiniGameStateHandling();
        }

        private void InitCascadeGame()
        {
            _noOfCascades = _cascades.Length;
            foreach (Cascade cascade in _cascades)
            {
                cascade.InitCascade(_defaultBeadSpeed, _defaultHitWindowBuffer, _defaultHitWindowPos, _defaultHitWindowSize);
            }
            DefaultCascadeGame();
            DefaultCascades();
        }

        //Resets the mini game to default.
        private void DefaultCascadeGame()
        {
            _miniGameState = MiniGameState.INACTIVE;
            _currentCascade = 0;
        }

        //Resets each individual cascade to default.
        private void DefaultCascades()
        {
            foreach (Cascade cascade in _cascades)
            {
                cascade.SetState(CascadeState.INACTIVE);
                cascade.ResetCascade();
            }
        }

        //Handles the overarching state of this minigame.
        private void MiniGameStateHandling()
        {
            switch (_miniGameState)
            {
                case MiniGameState.INACTIVE:
                    break;
                case MiniGameState.RUNNING:
                    _miniGameState = RunGame(_currentCascade);
                    break;
                case MiniGameState.SUCCESS:
                    dropLoot();
                    break;
                case MiniGameState.FAILURE:
                    ResetGame();
                    break;
            }
        }

        //Updates minigame state based off on return status of currently active cascade.
        private MiniGameState RunGame(int currentIndex)
        {
            CascadeState cascadeState = _cascades[currentIndex].GetState();

            switch (cascadeState)
            {
                case CascadeState.INACTIVE:
                    _cascades[currentIndex].SetState(CascadeState.RUNNING);
                    break;
                case CascadeState.SUCCESS:
                    if (CheckWinCondition())
                    {
                        return MiniGameState.SUCCESS;
                    }
                    _currentCascade++;
                    break;
                case CascadeState.FAILURE:
                    return MiniGameState.FAILURE;
            }
            return MiniGameState.RUNNING;
        }

        //Checks the win condition of the minigame.
        private bool CheckWinCondition()
        {
            foreach (Cascade cascade in _cascades)
            {
                if (cascade.GetState() != CascadeState.SUCCESS)
                {
                    return false;
                }
            }
            OnChallengeCompleteEvent?.Invoke();
            return true;

        }

        //Resets the minigame.
        private void ResetGame()
        {
            //Reset the game to original inactive state.
            DefaultCascadeGame();
            DefaultCascades();
        }

        public MiniGameState GetMiniGameState()
        {
            return _miniGameState;
        }

        public bool GetPlayerRefRequirement()
        {
            return requiresPlayerRef;
        }

        private void dropLoot()
        {
            Instantiate(chestLoot, _lootLocation.position, _lootLocation.rotation);

            _miniGameState = MiniGameState.END;
        }
    }

    public enum MiniGameState
    {
        INACTIVE,
        RUNNING,
        SUCCESS,
        FAILURE,
        END
    }
}

