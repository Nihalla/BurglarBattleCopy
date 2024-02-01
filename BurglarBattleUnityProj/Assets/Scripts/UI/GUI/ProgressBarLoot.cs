using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarLoot : MonoBehaviour
{
    [SerializeField] private Loot _playerLoot;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;

    private void Start()
    {
        _playerLoot = GetComponentInParent<Loot>();
    }

    private void Update()
    {
        _textMeshProUGUI.text = _playerLoot.GetCurrentLoot().ToString();
    }
}
