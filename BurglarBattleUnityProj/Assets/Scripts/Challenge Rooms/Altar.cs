//Ali

using PlayerControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class Altar : MonoBehaviour, IInteractable
{

    [SerializeField] private MeshRenderer[] _meshRenderers = Array.Empty<MeshRenderer>();

    //private int playerGold;
    private int altarCost = 5;
    [SerializeField]public TextMeshProUGUI altarText;

    [SerializeField] public ToolsSO[] tools;
    private int _lootIndex = 0;
    public GameObject altarLoot;
    [SerializeField] private Transform _lootSpawn;

    void Start()
    {
        altarLoot.GetComponent<ToolPickupController>().tool = (tools[Random.Range(_lootIndex, tools.Length)]);
        altarText.SetText($"Altar Cost: {altarCost}");
;    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction invokingPlayerInteraction)
    {
        ref int playerGold = ref invokingPlayerInteraction.GetComponent<Loot>().currentLoot;

        if (playerGold >= altarCost)
        {
            playerGold -= altarCost;
            GiveReward();
        }
    }

    public void GiveReward()
    {
        altarCost += 10;
        altarText.SetText($"Altar Cost: {altarCost}");
        Instantiate(altarLoot, _lootSpawn.position, _lootSpawn.rotation);
    }

}

