using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissingMainBlock : MonoBehaviour
{
    [SerializeField] private EscapeGame _escapeGame;
    [SerializeField] private LayerMask _layerMask;
    
   

    private void OnTriggerEnter(Collider other)
    {
        if ((_layerMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
                _escapeGame.OnSwitchToReadyPuzzle();
                    gameObject.SetActive(false);
        }
    
    }
}
