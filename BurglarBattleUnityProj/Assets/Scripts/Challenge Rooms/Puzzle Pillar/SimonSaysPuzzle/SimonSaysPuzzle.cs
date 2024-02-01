using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SimonSaysPuzzle : MonoBehaviour
{
    [Header("References")]
    //[SerializeField] private LayerMaskTrigger _layerTrigger;

    [Header("Team Setting")]
    [Tooltip("Ensure this is set to the same team as the cell this puzzle is in")]
    [SerializeField] private FirstPersonController.PlayerTeam _teamCell = FirstPersonController.PlayerTeam.UNKNOWN;
    [Header("Block References")]
    [SerializeField] private SimonSaysPuzzleBlock[] _blocks;
    [Header("Reset Settings")]
    [Tooltip("If true, will reset the puzzle after the player has completed it after a delay")]
    [SerializeField] private bool _resettable = true;
    [Tooltip("The delay before the puzzle resets after the player has completed it")]
    [SerializeField] private float _resetDelay = 5f;
    
    [Header("Puzzle Settings")]
    [SerializeField] private int _sequenceLength = 1;
    [Tooltip("The maximum sequence length the puzzle can reach")]
    [SerializeField] private int _maxSequenceLength = 7;
    [SerializeField] private float _preFlashDelay = .75f;
    [SerializeField] private float _flashTime = .75f;
    [SerializeField] private float _timeBetweenFlashes = .5f;
    [SerializeField] private float _incorrectFlashTime = 1f;

    //This event is invoked once the puzzle has been completed
    public UnityEvent OnPuzzleCompleteEvent;
    //This event is invoked once the puzzle has been reset (Normally after a delay after the player has completed the puzzle)
    public UnityEvent OnPuzzleResetEvent;

    private bool _playerIsInPuzzle = false;
    
    private bool _puzzleCompleted = false;

    private int _correctBlocksPressed = 0;
    private int _currentRound = 1;
    
    private Coroutine _flashSequenceCoroutine;
    private Coroutine _restartPuzzleCoroutine;
    private Coroutine _resetPuzzleCoroutine;
    
    private List<int> _sequence = new List<int>();
    
    // NOTE(Zack): we're now handling the player entering and exiting the puzzle stuff ourselves rather than using the LayerMaskTrigger
    // handle the first in/last out events, as this was causing bugs when the players get caught by the AI and are force teleported
    // out of the trigger. Currently we lock to the first player that enters the trigger for starting/stopping the puzzle (either player
    // can still interact with it).
    private bool _PlayerIsUsingPuzzle => _currentPlayerID > -1;
    private int _currentPlayerID = -1;

    private void Awake()
    {
        //_layerTrigger.onAnyColliderEnter += PlayerEnteredPuzzle;
        //_layerTrigger.onAnyColliderExit  += PlayerLeftPuzzle;

        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].OnWrongBlockPressed += RestartPuzzle;
            _blocks[i].OnCorrectBlockPressed += UpdateCorrectBlocks;
        }

        GlobalEvents.OnPlayerCaughtEvent += IncrementSequenceLength;

        SetAllBlocksPressable(false);
       
        _sequence.Clear();
        CreateRandomSequence();
        
        //NOTE(Sebadam2010): This change allows for there to only be one round (at whatever the sequence length is currently).
        _currentRound = _sequenceLength;

    }
    
    private void OnDestroy()
    {
       //_layerTrigger.onAnyColliderEnter -= PlayerEnteredPuzzle;
       // _layerTrigger.onAnyColliderExit  -= PlayerLeftPuzzle;

        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].OnWrongBlockPressed -= RestartPuzzle;
            _blocks[i].OnCorrectBlockPressed -= UpdateCorrectBlocks;
        }
        
        GlobalEvents.OnPlayerCaughtEvent -= IncrementSequenceLength;
    }
    
    //This function is called when the last player leaves the pressure pad
    public void EndPuzzle()
    {
        if (_puzzleCompleted)
        {
            return;
        }
        
        if (_restartPuzzleCoroutine != null)
        {
            StopCoroutine(_restartPuzzleCoroutine);
        }
        if (_flashSequenceCoroutine != null)
        {
            StopCoroutine(_flashSequenceCoroutine);
        }

        _correctBlocksPressed = 0;
        SetAllBlocksToInSequence(false);
        SetAllBlocksPressable(false);
        SetColorOfAllBlocks(SimonSaysPuzzleBlock.BlockState.OFF);
    }

    private void PlayerEnteredPuzzle(Collider player)
    {
        if (_PlayerIsUsingPuzzle) return;

        PlayerProfile profile = player.GetComponent<PlayerProfile>();
        _currentPlayerID = profile.GetPlayerID();
        StartPuzzle();
    }

    private void PlayerLeftPuzzle(Collider player)
    {
        if (!_PlayerIsUsingPuzzle) return;

        PlayerProfile profile = player.GetComponent<PlayerProfile>();
        if (profile.GetPlayerID() != _currentPlayerID) return;
        
        _currentPlayerID = -1;
        EndPuzzle();
    }
    
    //This function is called when the first player enters the pressure pad
    private void StartPuzzle()
    {
        if (_puzzleCompleted)
        {
            return;
        }

        SetNextInSequenceBlock();
        
        if (_flashSequenceCoroutine != null)
        {
            StopCoroutine(_flashSequenceCoroutine);
        }
        
        _flashSequenceCoroutine = StartCoroutine(FlashCurrentRoundSequenceBlocks());
    }
    
    //Creates a list of random numbers that will be used to determine which blocks (index) will be in the sequence
    private void CreateRandomSequence()
    {
        _sequence.Clear();
        
        for (int i = 0; i < _sequenceLength; i++)
        {
            _sequence.Add(Random.Range(0, _blocks.Length));
        }
        
        SetNextInSequenceBlock();
    }
    private IEnumerator FlashCurrentRoundSequenceBlocks()
    {
        float timer = 0f;
        SetAllBlocksPressable(false);
        
        for (int i = 0; i < _currentRound; i++)
        {
            while (timer < _preFlashDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0f;
            
            if (i == _currentRound - 1)
            {
                _blocks[_sequence[i]].FlashBlock(_flashTime, SimonSaysPuzzleBlock.BlockState.SEQUENCE_SHOW, true);
            }
            else
            {
                _blocks[_sequence[i]].FlashBlock(_flashTime, SimonSaysPuzzleBlock.BlockState.SEQUENCE_SHOW, false);
            }
            
            while (timer < _timeBetweenFlashes)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0f;
        }
        
        SetAllBlocksPressable(true);
    }

    private IEnumerator FlashAllBlocks(SimonSaysPuzzleBlock.BlockState blockState)
    {
        float timer = 0f;
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].FlashBlock(_flashTime, blockState, false);
        }
        
        //Wait for all blocks to finish flashing before exiting coroutine.
        while (timer < _flashTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    private void SetAllBlocksPressable(bool pressable)
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].IsPressable = pressable;
        }
    }

    private void UpdateCorrectBlocks()
    {
        _correctBlocksPressed++;

        if (_currentRound == _sequence.Count && _correctBlocksPressed == _currentRound)
        {
            FinishPuzzle();
            return;
        }
        
        
        if (_correctBlocksPressed >= _currentRound)
        {
            SetAllBlocksPressable(false);

            StartNextRound();
            
            if (_flashSequenceCoroutine != null)
            {
                StopCoroutine(_flashSequenceCoroutine);
            }
            _flashSequenceCoroutine = StartCoroutine(FlashCurrentRoundSequenceBlocks());
        }
        else
        {
            //Set next block's InSequence to be true.
            SetNextInSequenceBlock();
        }
    }

    private void StartNextRound()
    {
        _currentRound++;
        _correctBlocksPressed = 0;
        SetNextInSequenceBlock();
    }
    
    private void SetNextInSequenceBlock()
    {
        SetAllBlocksToInSequence(false);
        _blocks[_sequence[_correctBlocksPressed]].IsInSequence = true;
    }
    
    private void SetAllBlocksToInSequence(bool inSequence)
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].IsInSequence = inSequence;
        }
    }

    //This function is called when the player finishes the entire Simon Says sequence (Completes Puzzle)
    private void FinishPuzzle()
    {
        //NOTE(Sebadam2010): This is here to stop the flashing of all blocks in the sequence as it will transition back to its original colour after. 
        //Even after SetColorOfAllBlocks is called if pressed fast enough without this loop.
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].StopBlockFlash();
        }

        SetColorOfAllBlocks(SimonSaysPuzzleBlock.BlockState.CORRECT);
        SetAllBlocksPressable(false);
        _puzzleCompleted = true;
        OnPuzzleCompleteEvent?.Invoke();

        if (_resettable)
        {
            if (_resetPuzzleCoroutine != null)
            {
                StopCoroutine(_resetPuzzleCoroutine);
            }
            
            _resetPuzzleCoroutine = StartCoroutine(ResetPuzzle(_resetDelay));
        }
    }
    
    private IEnumerator ResetPuzzle(float _resetDelay)
    {
        float timer = 0f;
        while (timer < _resetDelay)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        //NOTE(Sebadam2010): This may be a slight issue if the player is still in the puzzle when the puzzle resets and the door is closing slowly.
        //As the initial simon says flash may be harder to see while the door is closing. But should be okay if door closes fast enough. (Need to test)
        OnPuzzleResetEvent?.Invoke();
        
        _puzzleCompleted = false;
        
        //NOTE(Sebadam2010): This change allows for there to only be one round (at whatever the sequence length is currently).
        _currentRound = _sequenceLength;
        
        _correctBlocksPressed = 0;
        
        CreateRandomSequence();
        SetAllBlocksPressable(false);
        
        //NOTE(Sebadam2010): I am placing this here as opposed to in StartPuzzle is due to the !playerIsInPuzzle check in the StartPuzzle function.
        //So the colours won't change if player is not in the layermask trigger.
        SetColorOfAllBlocks(SimonSaysPuzzleBlock.BlockState.OFF);
        StartPuzzle();
        
        yield break;
    }

    //This function is called when the player presses the wrong block
    private void RestartPuzzle()
    {
        if (_puzzleCompleted)
        {
            return;
        }

        _correctBlocksPressed = 0;
        
        SetAllBlocksPressable(false);
        SetNextInSequenceBlock();
        
        if (_restartPuzzleCoroutine != null)
        {
            StopCoroutine(_restartPuzzleCoroutine);
        }
        
        _restartPuzzleCoroutine = StartCoroutine(RestartingPuzzle());
    }

    
    private IEnumerator RestartingPuzzle()
    {

        yield return StartCoroutine(FlashAllBlocks(SimonSaysPuzzleBlock.BlockState.INCORRECT));
        
        if (_flashSequenceCoroutine != null)
        {
            StopCoroutine(_flashSequenceCoroutine);
        }
        
        _flashSequenceCoroutine = StartCoroutine(FlashCurrentRoundSequenceBlocks());
        
        yield break;
    }
    

    private void SetColorOfAllBlocks(SimonSaysPuzzleBlock.BlockState blockState)
    {
        for(int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].ChangeColour(blockState);
        }
    }

    public void FlashCurrentSequence()
    {
        RestartPuzzle();
    }

    //This function is mainly called from the player caught script (CatchComponent.cs) so that the simon says scales with the amount of times the player gets caught.
    public void IncrementSequenceLength(FirstPersonController playerTeam)
    {
        if (playerTeam.GetTeam() != _teamCell || _sequenceLength >= _maxSequenceLength)
        {
            return;
        }
        
        
        _sequenceLength++;
        
       // ////Debug.Log($"Increment team: {_teamCell} sequence length now: {_sequenceLength}");
    }

    public void DecrementSequenceLength(FirstPersonController playerTeam)
    {
        if (playerTeam.GetTeam() != _teamCell || _sequenceLength >= _maxSequenceLength)
        {
            return;
        }

        _sequenceLength--;
    }

    public void SetSequenceLength(int sequenceLength)
    {
        _sequenceLength = sequenceLength;
    }
}
