using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PlayerControllers;
using TMPro;

public class GUIUpdater : MonoBehaviour
{
    [SerializeField] private RectTransform item1;
    [SerializeField] private TextMeshProUGUI _item1Text;
    [SerializeField] private RectTransform item2;
    [SerializeField] private TextMeshProUGUI _item2Text;

    private InputActionsInputs _inputs;
    private InputAction _dpadInput;
    private DeviceData _deviceData;
    private PlayerControllers.PauseMenu _pauseMenu;
    private PlayerControllers.FirstPersonController _player;

    private void OnDpad()
    {
        if(_inputs.useTool)
        {
            SelectItem2();
        }
        if(_inputs.useTool2)
        {
            SelectItem1();
        }
    }

    private void Start()
    {
        _player = GetComponentInParent<PlayerControllers.FirstPersonController>();
        _pauseMenu = FindObjectOfType<PlayerControllers.PauseMenu>();
        _inputs = GetComponentInParent<PlayerControllers.InputActionsInputs>();
    }

    void Update()
    {
        if (_pauseMenu != null)
        {
            if (PauseMenu._MainGameIsPaused)
            {
                OnDpad();
            }
        }
    }

    public void SelectItem1()
    {
        item1.sizeDelta = new Vector2(150, 150);
        _item1Text.fontSize = 24;
        item2.sizeDelta = new Vector2(75, 75);
        _item2Text.fontSize = 12;

        item1.GetComponent<ButtonInfo>().Highlight();
        item2.GetComponent<ButtonInfo>().Unhighlight();
    }

    public void SelectItem2()
    {
        item2.sizeDelta = new Vector2(150, 150);
        _item2Text.fontSize = 24;
        item1.sizeDelta = new Vector2(75, 75);
        _item1Text.fontSize = 12;

        item2.GetComponent<ButtonInfo>().Highlight();
        item1.GetComponent<ButtonInfo>().Unhighlight();
    }
}
