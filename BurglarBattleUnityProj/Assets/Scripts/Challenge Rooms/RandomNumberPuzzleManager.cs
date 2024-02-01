// Author: Jake Martin (Jake Martin)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

//REVIEW(Norbert) This manager doesn't seem to manage anything, the whole puzzle is managed by the torches,
//                which makes it very difficult to follow, change or debug. I would suggest to rewrite it
//                in a way that a manager manages the states and checks for completion, and the torches
//                only handle player interactions, and feed data back to the manager about their individual
//                states.


public class RandomNumberPuzzleManager : MonoBehaviour 
{
    //REVIEW(Norbert) Most of your variables don't comply with the coding standards, please check
    //                the relevant documentation and fix them. Also, you can delete the unused comments.

    // Start is called before the first frame update

    public GameObject[] puzzleOjects;
    // public GameObject[] sortedObjects;

    
    public GameObject _interactableObject;

    //REVIEW(Norbert) The _assignedNumbers variable doesn't do anything, you only use as an index for
    //                for the randimiser. You can delete it, unless you really want to cache the index,
    //                but I can't see any reason to do so.
    private int _assignedNumbers;
    private int randomisePuzzle;
    public int _sortedIndex;



    void Awake()
    {
        AsignRandomNumbers();
    }

    private void AsignRandomNumbers()
    {
        //REVIEW(Norbert) The below assignment doesn't do anything, at the first run of the for loop
        //                this cached reference will be discarded, and a new reference will be cahced.
        _interactableObject = puzzleOjects[randomisePuzzle];

        for (int _assignedNumbers = 0; _assignedNumbers < puzzleOjects.Length; _assignedNumbers++)
        {

            randomisePuzzle = Random.Range(0, puzzleOjects.Length);
            _interactableObject = puzzleOjects[randomisePuzzle];
            puzzleOjects[randomisePuzzle] = puzzleOjects[_assignedNumbers];
            puzzleOjects[_assignedNumbers] = _interactableObject;
        }

        puzzleOjects[_assignedNumbers] = puzzleOjects[_sortedIndex];
    }
}
