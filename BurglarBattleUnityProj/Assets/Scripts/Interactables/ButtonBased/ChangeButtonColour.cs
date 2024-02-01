using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeButtonColour : MonoBehaviour
{
    [SerializeField] private ButtonState _initialButtonState = ButtonState.ENABLED;
    [Space]
    [SerializeField] private Material _disabledMaterial;
    [SerializeField] private Material _enabledMaterial;
    [SerializeField] private Material _pressedMaterial;

    private Renderer _renderer;
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        Debug.Assert(_renderer != null, "Could not get renderer. Please assign one in the inspector.", this);

        ChangeColour(_initialButtonState);
    }

    public void ChangeColour(ButtonState buttonState)
    {
        switch (buttonState)
        {
            case ButtonState.DISABLED:
                _renderer.material = _disabledMaterial;
                break;
            case ButtonState.ENABLED:
                _renderer.material = _enabledMaterial;
                break;
            case ButtonState.PRESSED:
                _renderer.material = _pressedMaterial;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
