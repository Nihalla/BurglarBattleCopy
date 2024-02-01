using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBlock : MonoBehaviour
{
    [SerializeField] private EscapeGame _escapeGame;
    
   

    private void OnTriggerEnter(Collider other)
    {
        _escapeGame.OnPuzzleComplete();
    }
}
