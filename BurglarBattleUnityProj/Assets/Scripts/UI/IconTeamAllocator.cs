using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconTeamAllocator : MonoBehaviour
{
    [SerializeField] private ColorBlock _team1ColorBlock;
    [SerializeField] private ColorBlock _team2ColorBlock;
    [SerializeField] private Button[] _buttons;
    [SerializeField] private bool _changingButton = true;
    [SerializeField] private GameObject _changingImage;
    private bool _teamChecked;

    private void Update()
    {
        if (!_teamChecked)
        {
            var teamAllocation = GetComponentInParent<PlayerProfile>().GetTeam();

            if (teamAllocation == PlayerControllers.FirstPersonController.PlayerTeam.TEAM_ONE)
            {
                if (_changingButton)
                {
                    foreach (var button in _buttons)
                    {
                        button.GetComponent<ButtonInfo>().UpdateColours(_team1ColorBlock);
                        button.colors = _team1ColorBlock;
                    }
                }
                else
                {
                    if (_changingImage == null)
                    {
                        return;
                    }

                    _changingImage.GetComponent<Image>().color = _team1ColorBlock.normalColor;
                }
            }
            else if (teamAllocation == PlayerControllers.FirstPersonController.PlayerTeam.TEAM_TWO)
            {
                if (_changingButton)
                {
                    foreach (var button in _buttons)
                    {
                        button.GetComponent<ButtonInfo>().UpdateColours(_team2ColorBlock);
                        button.colors = _team2ColorBlock;
                    }
                }
                else
                {
                    if (_changingImage == null)
                    {
                        return;
                    }

                    _changingImage.GetComponent<Image>().color = _team2ColorBlock.normalColor;
                }
            }

            _buttons[0].GetComponent<ButtonInfo>().Highlight();
            _teamChecked = true;
        }
        else
        {
            Destroy(this);
        }
    }
}

