using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAgent : GuardBase
{
    public GoblinAgent() : base(GuardType.GOBLIN) {}

    public delegate void GoblinStunDel(float seconds);
    public GoblinStunDel onGoblinStunned;

    public override void StunGuard(float seconds)
    {
        onGoblinStunned?.Invoke(seconds);
    }

    protected override IEnumerator GuardStun(float seconds)
    {
        // NOTE(Zack): we should never get here, this has to be implemented because,
        // the base class has an abstract member...
        Debug.LogError("Calling wrong stun function");
        yield break;
    }
}
