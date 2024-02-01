using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControllers;
using UnityEngine;

public static class GlobalEvents
{

    public static event Action<FirstPersonController> OnPlayerCaughtEvent;
    public static event Action TeamGoldUpdate;
    public static event Action OpenedVaultTimer;

    public static void OnPlayerCaught(FirstPersonController player)
    {
        OnPlayerCaughtEvent?.Invoke(player);
    }
    
    public static event Action VaultUnlock;
    public static event Action<int> VaultPuzzles;

    public static void OnVaultUnlock()
    {
        VaultUnlock?.Invoke();
    }

    public static void OnVaultPuzzles(int obj)
    { 
        VaultPuzzles?.Invoke(obj);
    }

    public static void OnTeamGoldUpdate()
    {
        TeamGoldUpdate?.Invoke();
    }

    public static void InitiateVaultTimer()
    {
        OpenedVaultTimer?.Invoke();
    }

    // REVIEW(WSWhitehouse): These actions are rather complicated and there are no comments to what each parameter is. Prefer to use a delegate instead.
    public static event Action<int, Transform> PlayerPuzzleInteract;
    public static event Action<int, Action<int>> PlayerPuzzleExit;

    /// <summary>
    /// Event that invokes a camera transition when the player starts an interaction with a puzzle/object.
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="zoomAnchor"></param>
    public static void OnPlayerPuzzleInteract(int playerID, Transform zoomAnchor)
    {
        PlayerPuzzleInteract?.Invoke(playerID, zoomAnchor);
    }

    /// <summary>
    /// Event that invokes a camera transition when the player exits a puzzle/object. As a callback, the function that
    /// reenables the player controls needs to be passed in, the callback is invoked at the end of the lerp is finished.
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="callBackAction"></param>
    public static void OnPlayerPuzzleExit(int playerID, Action<int> callBackAction)
    {
        PlayerPuzzleExit?.Invoke(playerID, callBackAction);
    }

    public static event Action<int> PlayerJoinedTheGame;

    /// <summary>
    /// Event that invokes when a new player joins whilst the game is in progress.
    /// </summary>
    /// <param name="controllerID"></param>
    public static void OnPlayerJoinedTheGame(int controllerID)
    {
        PlayerJoinedTheGame?.Invoke(controllerID);
    }
}
