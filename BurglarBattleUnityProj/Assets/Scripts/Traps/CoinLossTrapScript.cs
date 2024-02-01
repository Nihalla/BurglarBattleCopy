//Author: Tane Cotterell-East (Roonstar96)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class CoinLossTrapScript : MonoBehaviour
{
    [SerializeField] private int _interval;
    [SerializeField] private int _coinLoss;
    //[SerializeField] add coin script here if needed, otherwise add below  

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(CoinLossFunction());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator CoinLossFunction()
    {
        for (int i = 1; i > 0; i++)
        {
            yield return new WaitForSeconds(_interval);
            CoinUITest._currentCoins -= _coinLoss; //Change when a proper coin/money manager is implamented
        }
        yield return null;
    }
}
