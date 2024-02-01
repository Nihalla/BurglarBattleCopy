using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackTrigger : MonoBehaviour
{
    public GameObject colliderBox;
    public int playerRef;
    public EndScreen endScreen;
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
        if (other.name == colliderBox.name)
        {
            endScreen.stopMovement[playerRef] = true;
        }
    }
}
