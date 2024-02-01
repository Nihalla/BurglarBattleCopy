// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts
using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLootBagDrop : MonoBehaviour
{

    private int _playerId;
    private int _lootValue;

    private bool _isOriginalPlayerAllowed = false;
    [SerializeField] float _originalPlayerTimerCheck = 2f;

    private delegate IEnumerator EmptyDel();
    private EmptyDel StartCountDownFunc;

    private void Start()
    {
        // NOTE(Zack): pre-allocation delegate function
        StartCountDownFunc = StartCountDown;
        StartCoroutine(StartCountDownFunc());
    }

    public void SetPlayerAndValue(int playerIndex, int looValue)
    {
        _playerId = playerIndex;
        _lootValue = looValue;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Loot>())
        {
            ////Debug.Log($"am i getting touched");

            Loot loot = other.GetComponent<Loot>();

            if (loot.PlayerController.playerID == _playerId && !_isOriginalPlayerAllowed)
            {
                ////Debug.Log($" the own player trie to get this bag");
                return;
            }

            ////Debug.Log($"is this getting called");
            
            //REVIEW(Felix): please see my review on line 111 of Loot.cs
            /* If value is more than space then don't pickup*/
            if (loot.currentLoot + _lootValue < loot.GetMaximumLoot())
            {
                loot.currentLoot += _lootValue;
                /*Destroy gameObject*/
                Destroy(gameObject);
            }
        }
    }
    
    private IEnumerator StartCountDown()
    {
        _isOriginalPlayerAllowed = false;

        // NOTE(Zack): we're doing the wait loop ourselves to stop any potential,
        // allocations from WaitForSeconds
        float timer = float.Epsilon;
        while (timer < _originalPlayerTimerCheck)
        {
            timer += Time.deltaTime;
            yield return null; // wait for update
        }

        _isOriginalPlayerAllowed = true;
    }
}
