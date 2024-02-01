// Author: Zack Collins

using System;
using UnityEngine;

// TODO(Zack): add more idol types depending on what the idol puzzle requires
/// <summary>
/// Describes what type of Idol an object is, used by <see cref="IdolPedestel"/> and <see cref="IdolType"/> for if the object matches to a pedestal
/// </summary>
[Serializable]
public enum IdolTypeFlag
{
    NONE = -1,
    DAGGER,
    CHALICE,
    SKULL,
    AXE,
    VAULT
}

/// <summary>
/// Place on object so that <see cref="IdolPedestal"/> can check if the object is the correct idol to be placed upon it
/// </summary>
[RequireComponent(typeof(PickUpInteractable))]
public class IdolType : MonoBehaviour
{
    public IdolTypeFlag type = IdolTypeFlag.NONE;

    // we get a reference to the last player that held onto this object
    [NonSerialized] public PlayerProfile profile;
}
