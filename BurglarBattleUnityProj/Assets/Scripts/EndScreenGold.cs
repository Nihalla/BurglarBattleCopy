using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndScreenGold : MonoBehaviour
{
    public TextMeshProUGUI team1Text;
    public TextMeshProUGUI team2Text;

    // REVIEW(Zack): do we need to be doing this every frame?
    // or can it be instead changed to work from an event so that only
    // when the amount of gold has changed we need to update the value.
    // or can it also just be done on Awake() if the value never changes?
    private void Awake()
    {
        team1Text.text = GoldTransferToEnd.team1Gold.ToString();
        team2Text.text = GoldTransferToEnd.team2Gold.ToString();
    }
}
