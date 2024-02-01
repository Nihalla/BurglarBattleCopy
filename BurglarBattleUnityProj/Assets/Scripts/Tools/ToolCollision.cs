using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolCollision : MonoBehaviour
{
    [SerializeField] private Animator _toolAnim;

    [SerializeField] LayerMaskTrigger _layerMaskTrigger;

    public void LowerTool()
    {
        _toolAnim.SetBool("ShouldLower", true);
    }

    public void RaiseTool()
    {
        _toolAnim.SetBool("ShouldLower", false);
    }
}
