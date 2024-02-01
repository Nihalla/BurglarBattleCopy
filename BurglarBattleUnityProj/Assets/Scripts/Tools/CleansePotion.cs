using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerControllers;

public class CleansePotion : MonoBehaviour, ITool
{
    [SerializeField] private GameObject _cleansePS;
    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        //check if slowed or stunned **NEEDS TO BE ADDED BY PLAYER TEAM**
        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        fpc.SetBaseMoveSpeed(8f);

        Instantiate(_cleansePS, fpc.transform);

        fpc.SetStunnedState(false);
    }

    public bool CanBeUsed(List<GameObject> nearby, bool hasHit)
    {
        return true;
    }
}
