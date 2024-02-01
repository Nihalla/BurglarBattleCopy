//Author: Norbert Kupeczki - 19040948

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RiddleDoor : MonoBehaviour
{
    //Game objects
    [SerializeField] private GameObject[] _dials = new GameObject[3];
    [SerializeField] private PuzzleDoorHintDecal[] _clues = new PuzzleDoorHintDecal[3];
    [SerializeField] private bool _disableClueProjectors = false;

    [SerializeField] private int[] _currentValue = new int[3];
    [SerializeField] private int[] _answers = new int[3];

    [SerializeField] private Transform _leftDoor;
    [SerializeField] private Transform _rightDoor;

    [SerializeField] private Material[] _colours = new Material[6];

    private delegate IEnumerator DialRotation(int dialIndex);
    private DialRotation _dialRotateFunc;
    private Coroutine[] _rotateCoroutines = new Coroutine[3];

    public bool _finalDoor = false;
    
    private bool _puzzleSolved = false;
    private const float LERP_SPEED = 4.0f;

    private Quaternion[] _rotationTargets = new Quaternion[3];

    private Background_Music_Manager _bgmManager;


    private void Awake()
    {
        _dialRotateFunc = RotateDial;

        InitDials();
    }

    private void Start()
    {
        GenerateRandomAnswers();
        _bgmManager = FindObjectOfType<Background_Music_Manager>();
    }

    private void Rotate(int id)
    {
        if (_puzzleSolved)
        {
            return;
        }
        _currentValue[id] += 1;
        _rotateCoroutines[id] = StartCoroutine(_dialRotateFunc(id));

        // Reset
        if (_currentValue[id] == 6)
        {
            _currentValue[id] = 0;
        }

        CheckSolution();
    }

    private void GenerateRandomAnswers()
    {
        do
        {
            for (int i = 0; i < _answers.Length; i++)
            {
                _answers[i] = Random.Range(0, _colours.Length);
                _clues[i].SetDecalColour(_colours[_answers[i]].color);
            }
        } while (CheckAnswers());
    }

    // Returns true if all answers are the same.
    private bool CheckAnswers()
    {
        return _answers[0] == _answers[1] && _answers[0] == _answers[2];
    }

    private void InitDials()
    {
        for (int i = 0; i < _currentValue.Length; i++)
        {
            _currentValue[i] = 0;
            _rotationTargets[i] = _dials[i].transform.rotation;
            _dials[i].GetComponent<Dial>().DialInteraction += Rotate;
        }
    }

    private bool CheckSolution()
    {
        for (int i = 0; i < _currentValue.Length ; i++)
        {
            if (_currentValue[i] != _answers[i])
            {
                return false;
            }
        }

        _puzzleSolved = true;
        OpenDoors();
        return true;
    }

    public void OpenDoors()
    {
        StartCoroutine(SlideDoor(_leftDoor, 3.0f));
        StartCoroutine(SlideDoor(_rightDoor, -3.0f));
        StartCoroutine(DestroyDials());

        if (_finalDoor)
        {
            StartCoroutine(SlideDoor(_leftDoor, 3.0f));
            StartCoroutine(SlideDoor(_rightDoor, -3.0f));
            StartCoroutine(DestroyDials());

            GlobalEvents.InitiateVaultTimer();
            _bgmManager.SetAudioState(3);
            _bgmManager.UpdateMusic();

        }
    }

    private IEnumerator SlideDoor(Transform door, float direction)
    {
        Vector3 target = door.localPosition + new Vector3(direction, 0,0);
        door.gameObject.SetActive(false);
        while (Vector3.Distance(door.transform.position, target) > 0.05f)
        {
            door.localPosition = Vector3.Lerp(door.localPosition, target, LERP_SPEED * Time.deltaTime);
            yield return null;
        }

        door.position = target;
        
        yield break;
    }

    private IEnumerator DestroyDials()
    {
        while (_dials[2].transform.localScale.z > 0.05f)
        {
            for (int i = 0; i < _dials.Length; ++i)
            {
                //REVIEW(Felix): This is not how you use a lerp, it will never reach the target value. You need to use a time variable and divide the total time by the time. 
                _dials[i].transform.localScale = Vector3.Lerp(_dials[i].transform.localScale, Vector3.zero, 3.0f * LERP_SPEED * Time.deltaTime);
                yield return null;
            }
        }

        for (int i = 0; i < _dials.Length; ++i)
        {
            _dials[i].transform.localScale = Vector3.zero;
            
            //NOTE(Sebadam2010): Setting gameObject to inactive as there is still a collision with the player occuring even when the scale is zero.
            _dials[i].gameObject.SetActive(false);
            
            // NOTE(WSWhitehouse): In some cases we shouldn't disable the projectors as they
            // should be placed around the level rather than on the door itself...
            if (_disableClueProjectors)
            {
                _clues[i].DisableProjector();
            }
        }

        yield break;
    }

    private IEnumerator RotateDial(int dialIndex)
    {
        if (_rotateCoroutines[dialIndex] != null)
        {
            StopCoroutine(_rotateCoroutines[dialIndex]);
        }

        Transform dial = _dials[dialIndex].transform;
        _rotationTargets[dialIndex] = _rotationTargets[dialIndex] * Quaternion.Euler(0, 60, 0);

         while (Quaternion.Angle(dial.rotation, _rotationTargets[dialIndex]) > 0.5f)
        {
            dial.rotation = Quaternion.Lerp(dial.rotation, _rotationTargets[dialIndex], 2.0f * LERP_SPEED * Time.deltaTime);
            yield return null;
        }

        dial.rotation = _rotationTargets[dialIndex];
        yield break;
    }
}
