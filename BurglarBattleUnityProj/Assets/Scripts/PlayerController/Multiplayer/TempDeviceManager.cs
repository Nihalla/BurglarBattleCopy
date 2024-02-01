using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TempDeviceManager : MonoBehaviour
{
    public int number_of_player = 1;
    public float response_timer = 90.0f;

    private void Awake()
    {
        InputDevices.StartSearchForDevices();
    }

    private void FixedUpdate()
    {
        if(response_timer > 0)
        {
            response_timer -= Time.deltaTime;
        }
        else
        {
            InputDevices.StopSearchForDevices();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
