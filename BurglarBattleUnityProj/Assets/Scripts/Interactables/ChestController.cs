using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public bool Locked = true;
    private Animator _animator;
    public CoinController chestLoot;
    public LootSelector lootType;
    public Transform lootLoc;

    [NonSerialized] public bool isGoblinChest; // HACK(Zack): this works but we shouldn't really have to do it this way
    private CoinController _cachedLootObject;

    public delegate void EmptyEventDel();
    public EmptyEventDel onChestUnlockedEvent;
    public EmptyEventDel onChestLockedEvent;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        chestLoot.SetLoot(lootType);
    }
    
    public void UnlockChest()
    {
        _animator.Play("OpenChest");
        Locked = false;

        onChestUnlockedEvent?.Invoke();

        // HACK(Zack): this is so that we can set value amount for the chest dependant on whether this is a chest
        // controlled by a goblin loot stash or not;
        if (!isGoblinChest) chestLoot.SetLoot(lootType);

        // if the value of the loot is 0 we don't spawn any loot
        if (chestLoot.Value == 0) return;
       ////Debug.Log($"Loot Value: {chestLoot.Value}; Amount: {chestLoot.Loot.lootAmount}");
        _cachedLootObject = Instantiate(chestLoot, transform.position, transform.rotation);
    }

    public void CloseAndLockChest()
    {
        // TODO(Zack): remove the destroying of the object, and return it to an ObjectPool instead.
        if (_cachedLootObject != null) 
        {
            Destroy(_cachedLootObject.gameObject);
        }

        _animator.Play("CloseChest");
        Locked = true;

        onChestLockedEvent?.Invoke();
    }

    public void SetValue(int value)
    {
        chestLoot.Value = value;
    }

    public void IncrementValue(int value)
    {
        chestLoot.Value += value;
    }
}
