using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(LayerMaskTrigger))]
public class KillBox : MonoBehaviour 
{
    private FourPlayerManager _playerManager;
    private LayerMaskTrigger _layerTrigger;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _playerManager = FindObjectOfType<FourPlayerManager>(); // TODO(Zack): remove the need for using [FindObjectOfType]

        _layerTrigger = GetComponent<LayerMaskTrigger>();
        _layerTrigger.onAnyColliderEnter += OnAnyEnter;

        // we enforce the collider on this object to be a trigger
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }

    private void OnDestroy()
    {
        _layerTrigger.onAnyColliderEnter -= OnAnyEnter;
    }

    private void OnAnyEnter(Collider other)
    {
        // REVIEW(Zack): do we want to change it so that they don't respawn at the spawn location?
        // maybe they should spawn in jail?
        PlayerProfile profile = other.GetComponentInParent<PlayerProfile>();
        int playerID = _playerManager.GetPlayerID(profile);
        float3 spawn = _playerManager.GetSpawnPoint(playerID);
        other.transform.position = spawn;
    }
}
