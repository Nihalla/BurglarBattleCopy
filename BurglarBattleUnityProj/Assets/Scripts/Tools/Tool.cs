// Joshua Weston

using UnityEngine;
using System.Collections.Generic;

public interface ITool
{
    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit);
    public bool CanBeUsed(List<GameObject> nearby, bool hasHit);
}
