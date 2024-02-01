using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealthIndicator : MonoBehaviour
{
    [SerializeField] private Sprite[] stealthImages;
    private Image indicator;
    public StealthStatus currentStealthStatus;
    public enum StealthStatus {Hidden, Caution, Visible, Sighted, Off}

    private void Start()
    {
        indicator = GetComponent<Image>();
        currentStealthStatus = StealthStatus.Off;
    }
    public void setStealthStatus(StealthStatus newStatus)
    {
        currentStealthStatus = newStatus;
    }
    private void ChangeStealth()
    {
        if(currentStealthStatus == StealthStatus.Hidden)
        {
            indicator.enabled = true;
            indicator.overrideSprite = stealthImages[0];
        }
        if(currentStealthStatus == StealthStatus.Caution)
        {
            indicator.enabled = true;
            indicator.overrideSprite = stealthImages[1];
        }
        if (currentStealthStatus == StealthStatus.Visible)
        {
            indicator.enabled = true;
            indicator.overrideSprite = stealthImages[2];
        }
        if (currentStealthStatus == StealthStatus.Sighted)
        {
            indicator.enabled = true;
            indicator.overrideSprite = stealthImages[3];
        }
        if (currentStealthStatus == StealthStatus.Off)
        {
            indicator.enabled = false;
        }
    }
    private void Update()
    {
        ChangeStealth();
    }
}
