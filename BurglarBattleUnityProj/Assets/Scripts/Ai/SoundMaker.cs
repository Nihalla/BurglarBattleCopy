using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaker : MonoBehaviour
{
    public void Init(float noise) 
    {
        MakeSound(noise);
    }

    private void MakeSound(float soundDist) 
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, soundDist);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform.GetComponent<SoundDetectionComponent>() != null)
            {
                 hitCollider.transform.GetComponent<SoundDetectionComponent>().DistanceSoundDetection(this.transform.position, soundDist);
            }
        }
        Destroy(this);
    }
}
