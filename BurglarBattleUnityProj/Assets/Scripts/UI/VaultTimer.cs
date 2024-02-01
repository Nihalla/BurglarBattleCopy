using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VaultTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _minutesText;
    [SerializeField] private TextMeshProUGUI _secondsText;

    public float timeRemaining = 300;
    public bool timerIsRunning = false;
    public bool _vaultTimer = false;

    private int _minutes;
    private int _seconds;

    private void Awake()
    {
        GlobalEvents.OpenedVaultTimer += VaultOpened;
    }
    async void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;

                    if(timeRemaining > 0 && _vaultTimer)
                    {
                    timeRemaining -= Time.deltaTime * 2.5f;
                    }
                }
            else
            { 
                timeRemaining = 0;
                timerIsRunning = false;

                //FadeTransition.instance.FadeIn(); // Fade in the loading screen

                await Task.Delay(1000);
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("EndScreen");
            }
        }

        DisplayTime(timeRemaining);
    }

    private void VaultOpened()
    {
        timeRemaining = 45;
    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = (int)(timeToDisplay / 60);
        int seconds = (int)(timeToDisplay % 60);
    
        // NOTE(Zack): we do these checks so that we don't do unnecessary string allocations 
        // (that we cannot control) from TextMeshPro elements
        if (minutes != _minutes)
        {
            _minutes = minutes;
            _minutesText.text = Strings.numbers[minutes];
        }

        // NOTE(Zack): we do these checks so that we don't do unnecessary string allocations 
        // (that we cannot control) from TextMeshPro elements
        if (seconds != _seconds)
        {
            _seconds = seconds;
            _secondsText.text = Strings.numbers[seconds];
        }
    }

    private void OnDestroy()
    {
        GlobalEvents.OpenedVaultTimer -= VaultOpened;
    }
}
