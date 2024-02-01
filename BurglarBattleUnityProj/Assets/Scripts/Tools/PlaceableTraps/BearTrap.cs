// Joshua Weston

using UnityEngine;
using System.Collections.Generic;
using PlayerControllers;

public class BearTrap : MonoBehaviour, ITool
{
    public FirstPersonController.PlayerTeam _team;
    public GameObject trapPrefab;

    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        if (hasHit)
        {
            GameObject trap = Instantiate(trapPrefab, lookPoint.point, Quaternion.identity);
            trap.transform.SetParent(null);

            trap.GetComponent<BoxCollider>().enabled = true;
            trap.GetComponent<BearTrap>().SetTeam(player.GetComponent<PlayerProfile>().GetTeam());
        }
    }
    public bool CanBeUsed(List<GameObject> nearby, bool hasHit)
    {
        return hasHit;
    }

    void SetTeam(FirstPersonController.PlayerTeam newTeam)
    {
        _team = newTeam;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerProfile>(out PlayerProfile profile))
        {
            // if the player entering this trap is on the same team as the player that placed the trap we don't do anything
            if (profile.GetTeam() == _team) return;

            // Player of other team has stepped onto the trap
            profile.GetPlayer().StunPlayerForTimer(2f);
            Destroy(gameObject);
        }
    }
}
