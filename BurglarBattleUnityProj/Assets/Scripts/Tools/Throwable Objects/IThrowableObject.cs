using System;
using UnityEngine;

public interface IThrowableObject
{
    public void OnObjectHit(Collider collider);

    public void DestroyThrowable();
}
