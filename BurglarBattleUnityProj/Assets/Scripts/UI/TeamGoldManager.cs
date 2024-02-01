using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamGoldManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI team1Gold;
    [SerializeField] private TextMeshProUGUI team2Gold;

    private CoinDepositController[] coinDepositControllers;

    private void Awake()
    {
        GlobalEvents.TeamGoldUpdate += UpdateTeamGold;
        coinDepositControllers = FindObjectsOfType<CoinDepositController>();
    }
    void UpdateTeamGold()
    {
        for (int i = 0; i < coinDepositControllers.Length; i++)
        {
            if (coinDepositControllers[i].GetPlayerTeam() == PlayerControllers.FirstPersonController.PlayerTeam.TEAM_ONE)
            {
                team1Gold.text = coinDepositControllers[i].totalLoot.ToString("F0");
            }
            if (coinDepositControllers[i].GetPlayerTeam() == PlayerControllers.FirstPersonController.PlayerTeam.TEAM_TWO)
            {
                team2Gold.text = coinDepositControllers[i].totalLoot.ToString("F0");
            }
        }
    }

    private void OnDestroy()
    {
        GlobalEvents.TeamGoldUpdate -= UpdateTeamGold;
    }
}
