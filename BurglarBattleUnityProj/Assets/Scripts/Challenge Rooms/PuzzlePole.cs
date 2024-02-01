using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePole : MonoBehaviour
{
    #region [Variables]

    // Furture plans
    public GameObject[] puzzlePoleAreas;

    //Game objects
    public GameObject DialSelection1;
    public GameObject DialSelection2;
    public GameObject DialSelection3;

    public GameObject Clue1;
    public GameObject Clue2;
    public GameObject Clue3;

    //Int
    public int ChoiceSelection1;
    public int ChoiceSelection2;
    public int ChoiceSelection3;

    public int Answer1;
    public int Answer2;
    public int Answer3;


    #endregion

    #region [Functions]

    //Select choice colour
    void SelectRotation1()
    {
        // Add 1 to this Int
        ChoiceSelection1++;

        //Roate to next choice / colour
        DialSelection1.transform.Rotate(0, 60, 0, Space.Self);

        // Reset
        if (ChoiceSelection1 == 7)
        {
            ChoiceSelection1 = 1;
        }
    }

    void SelectRotation2()
    {
        // Add 1 to this Int
        ChoiceSelection2++;

        //Roate to next choice / colour
        DialSelection2.transform.Rotate(0, 60, 0, Space.Self);

        // Reset
        if (ChoiceSelection2 == 7)
        {
            ChoiceSelection2 = 1;
        }
    }

    void SelectRotation3()
    {
        // Add 1 to this Int
        ChoiceSelection3++;

        //Roate to next choice / colour
        DialSelection3.transform.Rotate(0, 60, 0, Space.Self);

        // Reset
        if (ChoiceSelection3 == 7)
        {
            ChoiceSelection3 = 1;
        }
    }

    //Genrate the required answer
    void RandomAnswer1(int randNum1)
    {
        Answer1 = randNum1;

        ////Debug.Log(randNum1);
    }

    void RandomAnswer2(int randNum2)
    {
        Answer2 = randNum2;

        ////Debug.Log(randNum2);
    }

    void RandomAnswer3(int randNum3)
    {
        Answer3 = randNum3;

        ////Debug.Log(randNum3);
    }

    #endregion

    #region [Start & Update functions]

    private void Start()
    {
        //Set Input selection to default
        ChoiceSelection1 = 1;
        ChoiceSelection2 = 1;
        ChoiceSelection3 = 1;

        //At start genrate required puzzles
        RandomAnswer1(Random.Range(1, 6));
        RandomAnswer2(Random.Range(1, 6));
        RandomAnswer3(Random.Range(1, 6));
    }

    private void Update()
    {
        //Protype input functions for checking if puzzle is interactable
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectRotation1();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectRotation2();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectRotation3();
        }

        //Check to see if player has correct combation
        if (ChoiceSelection1 == Answer1 && ChoiceSelection2 == Answer2 && ChoiceSelection3 == Answer3)
        {
            ////Debug.Log("Puzzle is solved");
        }
    }

    #endregion

    #region [Future updates / plans]

    // I plan to make the door puzzle in the near furture a spawnable clone that can generate it's own functions above,
    // with the need for invidual 
    void RandomPuzzleGenrator(int randNum)
    {
        ////Debug.Log(randNum);

        //GameObject clone = Instantiate(DialSelection1, Vector3.zero, Quaternion.identity);
        ////Debug.Log(clone.name);
        ////Debug.Log(clone.gameObject.name);
    }

    #endregion

}
