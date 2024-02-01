using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;

public class ButtonInfo : MonoBehaviour
{
    
    [HideInInspector] public ColorBlock defaultColorBlock;
    [HideInInspector] public ColorBlock highlightColorBlock;
    private Button _myButton;
    private bool setup = false;
    private bool _isNull;
    
    // Start is called before the first frame update
    private void Awake()
    {
        setup = false;
        
        if (!setup)
        {
            _myButton = GetComponent<Button>();
            if (_myButton == null) return;
            
            defaultColorBlock = _myButton.colors;
            highlightColorBlock = defaultColorBlock;

            highlightColorBlock.normalColor = defaultColorBlock.highlightedColor;
            highlightColorBlock.highlightedColor = defaultColorBlock.normalColor;
            setup = true;
        }
       
    }

    private void Start()
    {
        if (_myButton == null)
        {
            setup = false;
        }
        if (!setup)
        {
            _myButton = GetComponent<Button>();
            if (_myButton == null) return;
            
            defaultColorBlock = _myButton.colors;
            highlightColorBlock = defaultColorBlock;

            highlightColorBlock.normalColor = defaultColorBlock.highlightedColor;
            highlightColorBlock.highlightedColor = defaultColorBlock.normalColor;
            setup = true;
        }
    }

    public void Highlight()
    {
        if (setup)
        {
            _myButton.colors = highlightColorBlock;
        }
    }

    public void Unhighlight()
    {
        if (setup)
        {
            _myButton.colors = defaultColorBlock;
        }
        
    }

    public void UpdateColours(ColorBlock colorBlock)
    {
        defaultColorBlock = colorBlock;
        highlightColorBlock = defaultColorBlock;

        highlightColorBlock.normalColor = defaultColorBlock.highlightedColor;
        highlightColorBlock.highlightedColor = defaultColorBlock.normalColor;
    }
}
