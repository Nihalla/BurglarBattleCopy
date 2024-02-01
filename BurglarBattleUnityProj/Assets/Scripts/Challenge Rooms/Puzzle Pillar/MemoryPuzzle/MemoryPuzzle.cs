using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

/// <summary>
/// Once the player steps on the pressure pad, the blocks will flash up
/// and then the player has to remember which blocks were active and then press the buttons in the correct order.
/// </summary>
public class MemoryPuzzle : MonoBehaviour
{
    [Header("Block References")]
    [SerializeField] private MemoryPuzzleBlock[] _blocks;
    [Header("Puzzle Settings")]
    [SerializeField] private int _sequenceLength = 5;

    [Header("Feedback Settings")]
    [SerializeField] private int _numberOfFlashes = 3;
    [SerializeField] private float _flashTime = .75f;
    [SerializeField] private float _timeBetweenFlashes = .5f;
    [SerializeField] private float _incorrectFlashTime = 1f;

    public UnityEvent OnPuzzleComplete;
    //public PuzzleDel OnPuzzleCompleteEvent { get; set; }

    private bool _playerIsInPuzzle = false;
    private bool _puzzleCompleted = false;
    
    private int _correctBlocksPressed = 0;
    
    private Coroutine _flashBlocksCoroutine;
    private Coroutine _feedbackIncorrectSequenceCoroutine;
    private Coroutine _restartingPuzzleCoroutine;
    
    

    private void Awake()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].OnWrongBlockPressed += RestartPuzzle;
            _blocks[i].OnCorrectBlockPressed += UpdateCorrectBlocks;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].OnWrongBlockPressed -= RestartPuzzle;
            _blocks[i].OnCorrectBlockPressed -= UpdateCorrectBlocks;
        }
    }

    public void StartPuzzle()
    {
        if (_puzzleCompleted || !_playerIsInPuzzle)
        {
            return;
        }
        
        SetRandomBlocksToInSequence();

        if (_flashBlocksCoroutine != null)
        {
            StopCoroutine(_flashBlocksCoroutine);
        }
        
        _flashBlocksCoroutine = StartCoroutine(FlashSequenceBlocks());
    }
    
    private IEnumerator FlashSequenceBlocks()
    {
        float timer = 0f;
        
        for (int i = 0; i < _numberOfFlashes; i++)
        {
            SetColorOfInSequenceBlocks(MemoryPuzzleBlock.BlockState.ON);
            while (timer < _flashTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            SetColorOfInSequenceBlocks(MemoryPuzzleBlock.BlockState.OFF);
            
            timer = 0f;
            
            // NOTE(Sebadam2010: Allowing player to instantly press the button as soon as the flashing sequence has finished to make it feel more responsive.
            // Instead of having to wait for the _timeBetweenFlashes to finish.
            if (i == _numberOfFlashes - 1)
            {
                break;
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

    public void EndPuzzle()
    {
        if (_puzzleCompleted)
        {
            return;
        }
        
        if (_feedbackIncorrectSequenceCoroutine != null)
        {
            StopCoroutine(_feedbackIncorrectSequenceCoroutine);
        }
        if (_flashBlocksCoroutine != null)
        {
            StopCoroutine(_flashBlocksCoroutine);
        }
        

        SetAllBlocksToInSequence(false);
        SetAllBlocksPressable(false);
        _correctBlocksPressed = 0;
        SetColorOfAllBlocks(MemoryPuzzleBlock.BlockState.OFF);
    }

    public void RestartPuzzle()
    {
        _correctBlocksPressed = 0;
        SetAllBlocksPressable(false);
        SetAllBlocksToInSequence(false);
        
        if (_restartingPuzzleCoroutine != null)
        {
            StopCoroutine(_restartingPuzzleCoroutine);
        }
        _restartingPuzzleCoroutine = StartCoroutine(RestartingPuzzle());
    }

    private IEnumerator RestartingPuzzle()
    {
        yield return StartCoroutine(FeedbackIncorrectSequence());
        StartPuzzle();
    }

    public void FinishPuzzle()
    {
        SetColorOfAllBlocks(MemoryPuzzleBlock.BlockState.CORRECT);
        SetAllBlocksPressable(false);
        _puzzleCompleted = true;
        OnPuzzleComplete?.Invoke();
    }
    
    private void UpdateCorrectBlocks()
    {
        _correctBlocksPressed++;
        if (_correctBlocksPressed >= _sequenceLength)
        {
            FinishPuzzle();
        }
    }
    
    //Feedbacks to the player that they pressed the wrong button
    private IEnumerator FeedbackIncorrectSequence()
    {
        float timer = 0f;
        
        SetColorOfAllBlocks(MemoryPuzzleBlock.BlockState.INCORRECT);
        while (timer < _incorrectFlashTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        SetColorOfAllBlocks(MemoryPuzzleBlock.BlockState.OFF);
    }

    public void SetRandomBlocksToInSequence()
    {
        int counter = 0;
        SetAllBlocksToInSequence(false);
        
        while (counter < _sequenceLength)
        {
            int randomIndex = Random.Range(0, _blocks.Length);
            if (!_blocks[randomIndex].IsInSequence)
            {
                _blocks[randomIndex].IsInSequence = true;
                counter++;
            }
        }
    }

    public void SetAllBlocksToInSequence(bool inSequence)
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].IsInSequence = inSequence;
        }
    }
    
    public void SetAllBlocksPressable(bool pressable)
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].IsPressable = pressable;
        }
    }
    
    public void SetColorOfInSequenceBlocks(MemoryPuzzleBlock.BlockState blockState)
    {
        for(int i = 0; i < _blocks.Length; i++)
        {
            if (_blocks[i].IsInSequence)
            {
                _blocks[i].ChangeColour(blockState);
            }
        }
    }
    
    public void SetColorOfAllBlocks(MemoryPuzzleBlock.BlockState blockState)
    {
        for(int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].ChangeColour(blockState);
        }
    }

    public void SetPlayerInPuzzle(bool playerInPuzzle)
    {
        _playerIsInPuzzle = playerInPuzzle;
    }
    
}
