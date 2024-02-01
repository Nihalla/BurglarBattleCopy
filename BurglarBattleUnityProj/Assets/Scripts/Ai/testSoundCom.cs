using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSoundCom : MonoBehaviour
{
    public bool test = false;
    public float distSound = 8f;

  
    void Update()
    {
        if (test) 
        {
            test = false;
            gameObject.AddComponent<SoundMaker>().Init(distSound);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, distSound);
    }
}
