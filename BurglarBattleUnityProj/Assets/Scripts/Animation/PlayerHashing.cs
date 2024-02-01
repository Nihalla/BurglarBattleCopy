using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHashing : MonoBehaviour
{
    public int walkBool;
    public int interactBool;
    public int crouchIdleBool;
    public int crouchWalkBool;
    public int sprintBool;
    private void Awake()
    {
        walkBool = Animator.StringToHash("Walk");
        interactBool = Animator.StringToHash("Interaction");
        crouchIdleBool = Animator.StringToHash("Crouch");
        crouchWalkBool = Animator.StringToHash("CrouchWalk");
        sprintBool = Animator.StringToHash("Sprint");
    }
}
