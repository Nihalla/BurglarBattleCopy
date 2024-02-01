using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonFrenzyManager : MonoBehaviour
{
    [SerializeField] private ButtonFrenzyTimer _timer;
    [Space]
    [SerializeField] private ButtonFrenzyBlock[] _blocks;

    [Tooltip("This should refer to the button that is used to start the timer")]
    [SerializeField] private ChangeButtonColour _timerStartButton;
    
    public UnityEvent OnChallengeCompletedEvent;

    private int _blocksPressed = 0;
    private int _blocksToPress;
    
    private bool _challengeStarted = false;
    private bool _challengeCompleted = false;
    
    private void Awake()
    {
        _timer.OnTimerFinishedEvent += FailChallenge;
        _blocksToPress = _blocks.Length;
        
        for (int i = 0 ; i < _blocks.Length ; i++)
        {
            _blocks[i].OnBlockPressedEvent += UpdateBlocksPressed;
        }
    }

    private void OnDestroy()
    {
        _timer.OnTimerFinishedEvent -= FailChallenge;
        
        //TODO CHANGE TO FOR LOOP
        foreach (var block in _blocks)
        {
            block.OnBlockPressedEvent -= UpdateBlocksPressed;
        }
    }
    
    public void StartChallenge()
    {
        if (_challengeCompleted || _challengeStarted)
        {
            return;
            
        }

        _challengeStarted = true;
        _timer.StartTimer();
        ChangeStateOnAllBlocks(ButtonFrenzyBlock.BlockState.ON);
        _timerStartButton.ChangeColour(ButtonState.PRESSED);
    }
    
    public void CompleteChallenge()
    {
        _timer.CompleteTimer();
        OnChallengeCompletedEvent?.Invoke();
        _challengeCompleted = true;
        _timerStartButton.ChangeColour(ButtonState.DISABLED);
    }

    public void FailChallenge()
    {
        _timer.StopTimer();
        _timer.RestartTimer();
        ChangeStateOnAllBlocks(ButtonFrenzyBlock.BlockState.OFF);
        _blocksPressed = 0;
        _challengeStarted = false;
        _timerStartButton.ChangeColour(ButtonState.ENABLED);
    }

    private void ChangeStateOnAllBlocks(ButtonFrenzyBlock.BlockState blockState)
    {
        for(int i = 0; i < _blocks.Length; i++)
        {
            _blocks[i].ChangeState(blockState);
        }
    }
    
    private void UpdateBlocksPressed()
    {
        _blocksPressed++;
        if (_blocksPressed >= _blocksToPress)
        {
            CompleteChallenge();
        }
    }
}
