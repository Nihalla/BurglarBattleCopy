// Author: Zack Collins

using System;
using UnityEngine;
using Unity.Mathematics;

public class GoblinLootStash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _depositPos;
    [SerializeField] private ChestController _chest;

    public float3 Position => _depositPos.position;

    private void Awake()
    {
        // NOTE(Zack): this could potentially be error prone, as it'll mean that players could technically lose their gold,
        // if they do not collect it from the chest but for now it is a good enough compromise
        _chest.onChestLockedEvent += ResetGold;
        _chest.isGoblinChest = true; // HACK(Zack): allows for no spawning of loot when the goblin, has not added any loot
    }

    private void OnDestroy()
    {
        _chest.onChestLockedEvent -= ResetGold;
    }

    public void AddGold(int amount) 
    {
        _chest.IncrementValue(amount);
    }

    public void ResetGold()
    {
        _chest.SetValue(0);
    }
}
