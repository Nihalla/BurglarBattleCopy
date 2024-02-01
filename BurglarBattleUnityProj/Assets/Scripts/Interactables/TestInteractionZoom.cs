// Author: Norbert Kupeczki - 19040948

using System;
using UnityEngine;

public class TestInteractionZoom : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _interactingPlayer;
    [SerializeField] private Transform _puzzleInteractionPoint;
    [Space]
    [Header ("Materials")]
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _holdMat;

    private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
    private MeshRenderer _meshRenderer => _meshRenderers[0];

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteract(PlayerInteraction interaction)
    {
        if (_interactingPlayer == null &&
            Vector3.Dot(interaction.gameObject.transform.forward, _puzzleInteractionPoint.transform.forward) > 0)
        {
            ////Debug.Log("Puzzle interaction on");
            _interactingPlayer = interaction.gameObject;
            GlobalEvents.OnPlayerPuzzleInteract(_interactingPlayer.GetComponent<PlayerProfile>().GetPlayerID(), _puzzleInteractionPoint);
        }
        else if (_interactingPlayer == interaction.gameObject)
        {
           ////Debug.Log("Puzzle interaction off");
            GlobalEvents.OnPlayerPuzzleExit(_interactingPlayer.GetComponent<PlayerProfile>().GetPlayerID(), TestCallback);
            _interactingPlayer = null;
        }
        else
        {
            ////Debug.Log("Can't interact with the puzzle");
        }
    }
    
    public void OnInteractHoldStarted(PlayerInteraction playerInteraction) { }

    public void OnInteractHoldEnded(PlayerInteraction playerInteraction) { }

    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _hoverMat;
    }

    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }

    private void TestCallback(int id)
    {
       ////Debug.Log("Controls on: Player" + id);
    }
}
