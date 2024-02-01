using System;
using UnityEngine;
using System.Collections.Generic;

public class ThrowableRock : MonoBehaviour, ITool, IThrowableObject
{
    [SerializeField] private float _stunDuration;

    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        // Called when the tool is used
        FindObjectOfType<EyeSentry>().Stun();
    }

    public bool CanBeUsed(List<GameObject> nearby, bool hasHit)
    {
        return true;
    }

    public void OnObjectHit(Collider collider)
    {
        DestroyThrowable();
    }

    public void DestroyThrowable()
    {
        // Spawn some particles
        Destroy(this);
    }
}
