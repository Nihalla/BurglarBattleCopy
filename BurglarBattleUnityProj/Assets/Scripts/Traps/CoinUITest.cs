using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinUITest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _coins;
    public static int _currentCoins;

    private void Awake()
    {
        _currentCoins = 1000;
    }

    private void Update()
    {
        _coins.text = ("Coins: " + _currentCoins);
    }
}
