using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VaultController : MonoBehaviour, IInteractable
{
    [Header("Door Pivots")]
    public Transform doorPivot1;
    public Transform doorPivot2;
    [Header("Door Open Rotations")]
    public float openRotation1;
    public float openRotation2;
    [Header("Door Settings")]
    public float rotationSpeed;
    public bool isDoorOpen;

    [SerializeField] private EscapeGame _escapeGame;
    private MeshRenderer[] _meshRenderers = new MeshRenderer[1];
    
    private void Awake()
    {
        GlobalEvents.VaultUnlock += OpenDoor;
        _meshRenderers[0] = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDoorOpen && doorPivot1.transform.localRotation.y <= openRotation1)
        {
            doorPivot1.transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime));
        }
        if (isDoorOpen && doorPivot2.transform.localRotation.y >= openRotation2)
        {
            doorPivot2.transform.Rotate(Vector3.up * (-rotationSpeed * Time.deltaTime));
        }
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        if (!_escapeGame.GetVaultPuzzleActivated())
        {

            _escapeGame.VaultPuzzlePlayer = playerInteraction.PlayerProfile.GetPlayerID();
            _escapeGame.StartVaultPuzzle();
        }
    }
    

    private void OpenDoor()
    {
        isDoorOpen = true;

        FindObjectOfType<VaultTimer>()._vaultTimer = true;
    }
}