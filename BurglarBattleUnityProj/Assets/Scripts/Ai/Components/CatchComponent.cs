using UnityEngine;
using PlayerControllers;
using UnityEditor;
using System;
using System.Collections;

public class CatchComponent : AiComponent
{
    /// <summary>
    /// This AI component determines what happens with the player if the guard catches them
    /// </summary>
    public CatchComponent() : base(AiComponentType.CATCH) { }

    [SerializeField] private GuardCatchType _outcomeCatchType;
    [SerializeField] private float _stunningTime = 1;
    [SerializeField] private float _eyeCloseTime = 1;

    private delegate IEnumerator PlayerCatchWaitDel(FirstPersonController player, GuardLevel level);
    private PlayerCatchWaitDel _playerCatchWaitFunc;
    private WaitForSeconds _waitTime;

    private bool _canCatch = true;
    
    public enum GuardCatchType 
    {
        SEND_TO_START,
        STUN,
        SEND_TO_PRISON,
        DRAGGER
    }

    private void Awake()
    {
        _playerCatchWaitFunc = PlayerCaught;
        _waitTime = new WaitForSeconds(_eyeCloseTime);
    }

    /// <summary>
    /// Sets the catch type of the guard.
    /// </summary>
    /// <param name="catchType"></param>
    public void SetGuardCatchType(GuardCatchType catchType)
    {
        _outcomeCatchType = catchType;
    }

    /// <summary>
    /// Returns the catch type of the guard
    /// </summary>
    /// <returns>GuardCatchType</returns>
    public GuardCatchType GetGuardCatchType()
    {
        return _outcomeCatchType;
    }

    /// <summary>
    /// Sets the duration of the stun in seconds.
    /// </summary>
    /// <param name="value"></param>
    public void SetStunTime(float value)
    {
        _stunningTime = value;
    }

    /// <summary>
    /// Gets the duration of the stun in seconds.
    /// </summary>
    /// <returns></returns>
    public float GetStunTime()
    {
        return _stunningTime;
    }

    /// <summary>
    /// Applies the catch effect on the player avatar based on the set catch type of the guard,
    /// takes in the FirstPersonController component from the player as a parameter.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="removeLoot"></param>
    /// <param name="removeTool"></param>
    /// <param name="removePickup"></param>
    //public void CoughtPlayer(FirstPersonController player, bool removeLoot = true, bool removeTool = true, bool removePickup = true)
    //{
    //    // REVIEW(WSWhitehouse): Should probably not use excessive boolean parameters in order like this (especially
    //    // when some are defaulted). It makes it super difficult to read what is going on in function calls and is
    //    // much easier to make mistakes when passing in values as it's not very easy to distinguish the parameters.
    //    // Take a look into using BitFlags (a special type of magic enum)!

    //    if (GuardManager.OnShieldHit(player.gameObject) == 1)
    //    {
    //        return;
    //    }

    //    if (removeLoot)
    //    {
    //        player.transform.GetComponent<PlayerControllers.Loot>().DropLoot();
    //    }

    //    if (removeTool)
    //    {
    //        player.transform.GetComponent<ToolController>().DropTool();
    //    }

    //    if (removePickup)
    //    {
    //        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
    //        interaction.DisableInteraction();

    //        // TODO(WSWhitehouse): Probably want to reenable interactions at a suitable point rather than waiting a 
    //        // few frames. But this should solve the issue for now...
    //        interaction.EnableInteractionNextFrame(2);
    //    }


    //    switch (_outcomeCatchType)
    //    {
    //        case GuardCatchType.SEND_TO_START:

    //            player.SetPlayerPosition(GuardManager.GetPlayerRespawnPoint(player.GetTeam()));

    //            break;
    //        case GuardCatchType.STUN:

    //            player.StunPlayerForTimer(_stunningTime);

    //            break;
    //        case GuardCatchType.SEND_TO_PRISON:

    //            player.SetPlayerPosition(GuardManager.GetPrisonPoint(player.GetTeam()));
    //            GlobalEvents.OnPlayerCaught(player);

    //            break;
    //        case GuardCatchType.DRAGGER:
    //            //similar to stun but it draggs the player with it 
    //            break;
    //        default:
    //            break;
    //    }
    //}


    [Flags]
    public enum CaughtPlayerOptions
    {
        None = 0,
        RemoveLoot = 1 << 0,
        RemoveTool = 1 << 1,
        RemovePickup = 1 << 2,
        All = RemoveLoot | RemoveTool | RemovePickup
    }


    public void CaughtPlayer(FirstPersonController player, GuardLevel level,CaughtPlayerOptions options = CaughtPlayerOptions.All)
    {
        if (!_canCatch)
        {
            return;
        }

        _canCatch = false;
        
        if (GuardManager.OnShieldHit(player.gameObject) == 1)
        {
            return;
        }

        if ((options & CaughtPlayerOptions.RemoveLoot) == CaughtPlayerOptions.RemoveLoot)
        {
            player.transform.GetComponent<PlayerControllers.Loot>().DropLoot(1);
        }

        if ((options & CaughtPlayerOptions.RemoveTool) == CaughtPlayerOptions.RemoveTool)
        {
            player.transform.GetComponent<ToolController>().DropTool();
        }

        if ((options & CaughtPlayerOptions.RemovePickup) == CaughtPlayerOptions.RemovePickup)
        {
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            interaction.DisableInteraction();
            
            // NOTE(WSWhitehouse): Multiplying the _eyeCloseTime by 2 as it's used as the open time too... This will need to be updated if that changes.
            interaction.EnableInteractionAfterDuration(_eyeCloseTime * 2.0f);
        }

        PlayerCatchUI.Catch(player.playerID, _eyeCloseTime);
        player.SetCaught(true);
        StartCoroutine(_playerCatchWaitFunc(player, level));
    }

    private IEnumerator PlayerCaught(FirstPersonController player, GuardLevel level)
    {
        yield return _waitTime;

        _canCatch = true;
        player.SetCaught(false);

        switch (_outcomeCatchType)
        {
            case GuardCatchType.SEND_TO_START:

                player.SetPlayerPosition(GuardManager.GetPlayerRespawnPoint(player.GetTeam()));
                ResetAggroHud(player.gameObject);
                break;
            case GuardCatchType.STUN:

                player.StunPlayerForTimer(_stunningTime);

                break;
            case GuardCatchType.SEND_TO_PRISON:

                player.SetPlayerPosition(GuardManager.GetPrisonPoint(player.GetTeam(), level));
                GlobalEvents.OnPlayerCaught(player);
                ResetAggroHud(player.gameObject);

                break;
            case GuardCatchType.DRAGGER:
                //similar to stun but it draggs the player with it 
                break;
            default:
                break;
        }
    }

    private static void ResetAggroHud(GameObject player)
    {
        player.GetComponentInChildren<AggroHUD>().ResetHud();
    }
}
