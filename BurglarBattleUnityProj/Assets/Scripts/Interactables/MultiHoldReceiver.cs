// Author: Sebastian Adamatzky

using UnityEngine;
using UnityEngine.Events;

public class MultiHoldReceiver : MonoBehaviour
{
    [Tooltip("The buttons that need to be activated to trigger the event")]
    [SerializeField] private MultiHoldButton[] _multiHoldButtons;

    public UnityEvent onReceiverActivatedEvent;

    private int _maxButtonsToActivate;
    private int _currentActivatedButtons;

    private void Awake()
    {
        if (onReceiverActivatedEvent == null)
        {
            onReceiverActivatedEvent = new UnityEvent();
        }
        
        _maxButtonsToActivate = _multiHoldButtons.Length;

        for (int i = 0; i < _maxButtonsToActivate; i++)
        {
            _multiHoldButtons[i].onButtonStateChangeEvent += AdjustButtonActivatedCount;
        }
    }

    private void AdjustButtonActivatedCount(bool buttonOn)
    {
        if (buttonOn)
        {
            _currentActivatedButtons++;
            
            if (_currentActivatedButtons >= _maxButtonsToActivate)
            {
                onReceiverActivatedEvent?.Invoke();
            }
        }
        else
        {
            _currentActivatedButtons--;
        }
    }
}
