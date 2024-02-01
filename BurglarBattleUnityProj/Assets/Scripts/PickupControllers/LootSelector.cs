// Joshua Weston

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Loot")]
public class LootSelector : ScriptableObject
{
    public GameObject lootModel;

    public int lootAmount;
}
