using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO(Zack): make a timer based respawn system as well
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(LayerMaskTrigger))]
public class ItemKillBox : MonoBehaviour
{
    private LayerMaskTrigger _layerTrigger;
    private BoxCollider _boxCollider;

    private void Awake()
    {   
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
        CachePosition cached = other.GetComponent<CachePosition>();
        other.transform.position = cached.pos;
    }
}
