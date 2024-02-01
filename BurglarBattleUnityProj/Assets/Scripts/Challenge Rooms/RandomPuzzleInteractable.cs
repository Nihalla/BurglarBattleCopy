using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RandomPuzzleInteractable : MonoBehaviour, IInteractable
{

    public RandomNumberPuzzleManager puzzleManager;
    public GameObject interactedObject;

    public GameObject chestLoot;
    public LootSelector lootType;

    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _holdMat;

    [SerializeField] private Transform _lootSpawn;

    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();
    private MeshRenderer _meshRenderer => _meshRenderers[0];

    [SerializeField] private ParticleSystem _correctAnswer = null;
    [SerializeField] private ParticleSystem _TorchOne = null;
    [SerializeField] private ParticleSystem _TorchTwo = null;
    [SerializeField] private ParticleSystem _TorchThree = null;
    [SerializeField] private ParticleSystem _TorchFour = null;

    public UnityEvent OnChallengeCompleteEvent;


    private void Start()
    {
        _correctAnswer.Stop();

        chestLoot.GetComponent<CoinController>().SetLoot(lootType);
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }


    public void OnInteractHoverStarted()
    {
        _meshRenderer.sharedMaterial = _hoverMat;
    }

    public void OnInteractHoverEnded()
    {
        _meshRenderer.sharedMaterial = _defaultMat;
    }


    public void OnInteract(PlayerInteraction invokingPlayerInteraction)
    {
      
        if(interactedObject == puzzleManager.puzzleOjects[puzzleManager._sortedIndex])
        {
            puzzleManager._sortedIndex++;
            _correctAnswer.Play();
           // //Debug.Log("Interacted with correct item");
            if (puzzleManager._sortedIndex == puzzleManager.puzzleOjects.Length)
            {
                PuzzleSolved();
            }
            
        }
        else
        {
           // //Debug.Log("Wrong");
            incorrectInput();
          
        }

    }

    private void incorrectInput()
    {
        puzzleManager._sortedIndex = 0;
        _TorchOne.Stop();
        _TorchTwo.Stop();
        _TorchThree.Stop();
        _TorchFour.Stop();
    }

    private void PuzzleSolved()
    {
        //reward logic
        Instantiate(chestLoot, _lootSpawn.position, _lootSpawn.rotation);
        OnChallengeCompleteEvent?.Invoke();
    }
}
