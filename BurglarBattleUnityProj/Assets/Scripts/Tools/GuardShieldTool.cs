// Joshua Weston

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GuardShieldTool : MonoBehaviour, ITool
{ 
    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        GuardManager.AddShield(player);
    }

    public bool CanBeUsed(List<GameObject> nearby, bool hasHit)
    {
        return true;
    }
}
