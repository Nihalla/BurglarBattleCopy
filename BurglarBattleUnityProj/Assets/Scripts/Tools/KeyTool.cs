// Joshua Weston

using UnityEngine;
using System.Collections.Generic;

public class KeyTool : MonoBehaviour, ITool
{
    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        foreach (GameObject obj in nearby)
        {
            if (obj.GetComponent<InteractableLock>())
            {
                obj.GetComponent<InteractableLock>().ForceUnlock();
            }
        }
    }
    public bool CanBeUsed(List<GameObject> nearby, bool hasHit)
    {
        foreach (GameObject obj in nearby)
        {
            if (obj.GetComponent<InteractableLock>())
            {
                return obj.GetComponent<InteractableLock>().CheckUnlockStatus();
            }
        }
        return false;
    }
}
