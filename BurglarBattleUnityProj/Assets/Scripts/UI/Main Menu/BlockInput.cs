using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockInput : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UIStateController.acceptInput = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UIStateController.acceptInput = true;
    }
}
