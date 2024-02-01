using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionVaultPuzzleButton : MonoBehaviour, IInteractable
{
    [SerializeField] private EscapeGame _escapeGame;
    
    private MeshRenderer[] _meshRenderers = new MeshRenderer[1];

    private void Awake()
    {
        _meshRenderers[0] = gameObject.GetComponent<MeshRenderer>();
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (!_escapeGame.GetVaultPuzzleActivated() || !_escapeGame.GetVaultPuzzleCompleted())
        {

            _escapeGame.VaultPuzzlePlayer = playerInteraction.PlayerProfile.GetPlayerID();
            _escapeGame.StartVaultPuzzle();
        }
    }

}
