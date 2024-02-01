// Author: Zack Collins
// Edit: Norbert Kupeczki - rotation is also cached

using System;
using UnityEngine;
using Unity.Mathematics;

public class CachePosition : MonoBehaviour 
{
    [NonSerialized] public float3 pos;
    [NonSerialized] public Quaternion rot;

    private void Start()
    {
        pos = gameObject.transform.position;
        rot = gameObject.transform.rotation;
    }
}
