using System;
using System.Collections.Generic;
using UnityEngine;
using PlayerControllers;

public class Potion : MonoBehaviour
{
    [SerializeField] private static float s_effectRadius = 6;
    [SerializeField] private LayerMask _playerLayer;

    [SerializeField] private GameObject _particles;
    private MeshRenderer _mr;
    

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        _mr.enabled = false;

        _particles.SetActive(false);
    }

    public void SetPotionEffects(float speedChange, float duration, bool inflictsStun, bool inflictsConfusion, bool inflictsInvisibility)
    {
       ////Debug.Log("Affecting player speed by " + speedChange + ". Potion Duration: " + duration + " seconds. Inflicts stun:" + inflictsStun +
           // ". Inflicts confusion: " + inflictsConfusion + ". Inflicts invisibility: " + inflictsInvisibility);

        _mr.enabled = true; 
        _particles.SetActive(true);

        List<FirstPersonController> nearbyPlayers = GetNearbyPlayers();

        if(nearbyPlayers.Count > 0)
        {
            for (int i = 0; i < nearbyPlayers.Count; ++i)
            {
                nearbyPlayers[i].SetMoveSpeedMultiplierForTimer(speedChange, duration);

                if (inflictsStun)
                {
                    nearbyPlayers[i].StunPlayerForTimer(duration / 10);
                }

                if (inflictsConfusion)
                {
                    nearbyPlayers[i].ConfusePlayerForTimer(duration / 10);
                }

                if (inflictsInvisibility)
                {
                    nearbyPlayers[i].SetInvisibleForTimer(duration);
                }
            }
        }

        else
        {
            Debug.LogError("No players nearby.");
        }
    }

    private List<FirstPersonController> GetNearbyPlayers()
    {
        List<FirstPersonController> playerList = new List<FirstPersonController>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, s_effectRadius, _playerLayer);

        for (int i = 0; i < colliders.Length; ++i)
        {
            playerList?.Add(colliders[i].GetComponentInParent<FirstPersonController>());
            ////Debug.Log(playerList[i]);
        }

        return playerList;
    }
}