using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointObject : MonoBehaviour
{

    private int objectMask;
    private int hightlightMask;



    // Start is called before the first frame update
    void Start()
    {
        objectMask = LayerMask.NameToLayer("Objects");
        hightlightMask = LayerMask.NameToLayer("Highlight");
    }

    // Update is called once per frame
    void Update()
    {


    }

    private void OnMouseOver()
    {
        gameObject.layer = hightlightMask;
       // //Debug.Log("Hello");
    }

    private void OnMouseExit()
    {
        gameObject.layer = objectMask;
       // //Debug.Log("bye");
    }
}
