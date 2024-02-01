// Author: Christy Dwyer (ChristyDwyer)

using System.Collections.Generic;
using UnityEngine;
using PlayerControllers;

public class ElectricFloor : MonoBehaviour
{
    [SerializeField] private float _stunDuration;

    [SerializeField] private LayerMask _characterLayer;
    private List<GameObject> _charactersTouching;

    private ElectricWire _wireScript;

    private void Awake()
    {
        _wireScript = GetComponent<ElectricWire>();

        _charactersTouching = new();
    }

    private void Update()
    {
        if (_wireScript.GetPowered())
        {
            if (_charactersTouching.Count > 0)
            {
                for (int i = 0; i < _charactersTouching.Count; ++i)
                {
                    // Stun player
                    //FirstPersonController playerController = _charactersTouching[i].GetComponent<FirstPersonController>();

                    if (TryGetComponent<FirstPersonController>(out FirstPersonController playerController))
                    {
                        playerController.StunPlayerForTimer(_stunDuration);
                        continue;
                    }
                    
                    // Stun guard
                }

                _charactersTouching.Clear();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object is in the layermask
        if ((_characterLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            _charactersTouching.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((_characterLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (_charactersTouching.Contains(collision.gameObject))
            {
                _charactersTouching.Remove(collision.gameObject);
            }
        }
    }
}
