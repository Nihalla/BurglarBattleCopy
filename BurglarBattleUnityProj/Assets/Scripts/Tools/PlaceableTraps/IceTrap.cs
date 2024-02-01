// Joshua Weston

using UnityEngine;
using System.Collections.Generic;
using PlayerControllers;
using System.Collections;

public class IceTrap : MonoBehaviour, ITool
{
    public float slowFactor = 0.5f;
    public float slowLength = 3.0f;
    public float trapLifespan;
    public GameObject fieldObject;

    private FirstPersonController.PlayerTeam _team;
    public GameObject trapPrefab;

    public void Use(List<GameObject> nearby, GameObject player, GameObject toolObject, RaycastHit lookPoint, bool hasHit)
    {
        if (hasHit)
        {
            GameObject trap = Instantiate(trapPrefab, lookPoint.point, Quaternion.identity);
            IceTrap trapController = trap.GetComponent<IceTrap>();
            trap.transform.SetParent(null);

            trap.GetComponent<SphereCollider>().enabled = true;
            trapController.fieldObject.SetActive(true);
            trapController.SetTeam(player.GetComponent<PlayerProfile>().GetTeam());
            trapController.StartCoroutine(trapController.StartTimer(trap));
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

    public IEnumerator StartTimer(GameObject toolObject)
    {
        yield return new WaitForSeconds(trapLifespan);
        Destroy(toolObject);
    }

    IEnumerator SlowEffect(Collider player)
    {
        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        float baseMS = fpc.GetBaseMoveSpeed();

        fpc.SetBaseMoveSpeed(baseMS *= slowFactor);
        fpc.SetFreeze(true);
        yield return new WaitForSeconds(slowLength);
        fpc.SetFreeze(false);
        fpc.SetBaseMoveSpeed(baseMS /= slowFactor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerProfile>().GetTeam() != _team)
        {
            // Player of other team has stepped onto the trap
            other.GetComponent<MonoBehaviour>().StartCoroutine(SlowEffect(other));
        }
    }
}
