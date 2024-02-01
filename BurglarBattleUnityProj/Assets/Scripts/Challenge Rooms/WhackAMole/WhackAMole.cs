using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class WhackAMole : MonoBehaviour
{
    [Header("Block Reference")]
    [SerializeField] private List<WhackAMoleBlock> _blocks;
    [Header("Progress Bar Reference")]
    [SerializeField] private ProgressBar _progressBar;

    [Header("Challenge Settings")] 
    //TODO(Sebadam2010): Make this random between min and max range.
    [SerializeField] private int _blocksToHit = 10;

    [SerializeField] private float _minTimeBeforeNextBlockAppears = 1f;
    [SerializeField] private float _maxTimeBeforeNextBlockAppears = 2.25f;

    [Header("Feedback Settings")] 
    [SerializeField] private float _minBlockFlashLength = 2f;
    [SerializeField] private float _maxBlockFlashLength = 3f;

    public UnityEvent OnChallengeCompleteEvent;

    private int _blocksHit = 0;

    private Coroutine _challengeCoroutine;

    private bool _challengeComplete = false;
    
    private void Awake()
    {
        for (int i = 0 ; i < _blocks.Count; i++)
        {
            _blocks[i].OnBlockPressedEvent += UpdateBlocksPressed;
        }
    }
    
    private void OnDestroy()
    {
        for (int i = 0 ; i < _blocks.Count; i++)
        {
            _blocks[i].OnBlockPressedEvent -= UpdateBlocksPressed;
        }
    }

    private void Start()
    {
        //NOTE(Sebadam2010): Just having the challenge continuously running as LayerMaskTrigger is a bit buggy in regards to
        //team 1 and 2 having some issues with the trigger depending on who enters it.
        //However, this could maybe cause FPS issues as it will be running all the time no matter where the players are.
        StartChallenge();
    }

    public void StartChallenge()
    {
        if (_challengeComplete)
        {
            return;
        }
        
        if (_challengeCoroutine != null)
        {
            StopCoroutine(_challengeCoroutine);
        }
        
        _progressBar.ChangeState(ProgressBar.ProgressBarState.ENABLED);
        _challengeCoroutine = StartCoroutine(ChallengeCoroutine());
    }

    public void StopChallenge()
    {
        if (_challengeComplete)
        {
            return;
        }
        
        if (_challengeCoroutine != null)
        {
            StopCoroutine(_challengeCoroutine);
        }
        _progressBar.ChangeState(ProgressBar.ProgressBarState.DISABLED);
        
        for (int i = 0 ; i < _blocks.Count; i++)
        {
            _blocks[i].StopFlash();
            _blocks[i].ChangeState(ButtonState.DISABLED);
        }
    }

    private void CompleteChallenge()
    {
        _progressBar.ChangeState(ProgressBar.ProgressBarState.COMPLETE);
        _challengeComplete = true;
        OnChallengeCompleteEvent?.Invoke();
        
        for (int i = 0 ; i < _blocks.Count; i++)
        {
            if (_blocks[i].GetButtonState() == ButtonState.ENABLED)
            {
                _blocks[i].StopFlash();

                //REVIEW(Sebadam2010): Doing it like this as otherwise coroutine will finish after changing state.
                while (!_blocks[i].ReturnWhenFlashBlockFinished())
                {
                }
                _blocks[i].ChangeState(ButtonState.COMPLETED);
            }
            else
            {
                _blocks[i].ChangeState(ButtonState.COMPLETED);
            }
        }

    }

    private IEnumerator ChallengeCoroutine()
    {
        float timer = 0f;
        
        while (!_challengeComplete)
        {
            int randomIndex = Random.Range(0, _blocks.Count);
            float randomBlockFlashLength = Random.Range(_minBlockFlashLength, _maxBlockFlashLength);
            float randomTimeBeforeNextBlockAppears = Random.Range(_minTimeBeforeNextBlockAppears, _maxTimeBeforeNextBlockAppears);
         
            //NOTE(Sebadam2010): Make sure we don't add the same block twice.
            if (_blocks.Count > 0)
            {
                while (_blocks[randomIndex].GetButtonState() == ButtonState.ENABLED)
                {
                    randomIndex = Random.Range(0, _blocks.Count);
                }
            }
            _blocks[randomIndex].Flash(randomBlockFlashLength);
            
            while (timer < randomTimeBeforeNextBlockAppears)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
        }
    }
    
    private void UpdateBlocksPressed(WhackAMoleBlock block)
    {
        _blocksHit++;
        _progressBar.UpdateProgressBar(_blocksHit, _blocksToHit);
        
        if (_blocksHit >= _blocksToHit)
        {
            CompleteChallenge();
        }
    }
}
