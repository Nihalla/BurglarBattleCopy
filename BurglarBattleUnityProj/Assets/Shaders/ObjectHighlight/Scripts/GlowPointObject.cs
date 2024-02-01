using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowPointObject : MonoBehaviour
{
    private int objectMask;
    private int hightlightMask;

    // Start is called before the first frame update
    void Start()
    {
        objectMask = LayerMask.NameToLayer("Actor");
        hightlightMask = LayerMask.NameToLayer("Glow");
    }

    private void OnMouseOver()
    {
        gameObject.layer = hightlightMask;
        ////Debug.Log("Hi");
    }

    private void OnMouseExit()
    {
        gameObject.layer = objectMask;
       // //Debug.Log("bye");
    }
}
