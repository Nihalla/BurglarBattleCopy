using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private Button _button;
    private static MenuInputController _menuInputControllers;

    private void Awake()
    {
        if (UIStateController.menuInputControllers == null)
        {
            _menuInputControllers = new MenuInputController();
        }
        else
        {
            UIStateController.menuInputControllers.EnableMenuControls();
        }

        if (UIStateController.menuInputControllers != null)
            UIStateController.menuInputControllers.PlayerSelectEvent.AddListener(Return);
    }

    void Start()
    {
        _button = FindObjectOfType<Button>();
        _button.GetComponent<ButtonInfo>().Highlight();
    }

    public async void Return()
    {
        _button.GetComponent<ButtonInfo>().Unhighlight();

        FadeTransition.instance.FadeIn();
//        UIStateController.menuInputControllers.DisableMenuControls();

        await Task.Delay(1000);
        GoldTransferToEnd.team1Gold = 0;
        GoldTransferToEnd.team2Gold = 0;
        SceneManager.LoadScene("Main Menu Scene");
    }
}
