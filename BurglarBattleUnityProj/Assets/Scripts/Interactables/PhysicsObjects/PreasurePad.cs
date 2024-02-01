// Author: Vlad Trakiyski

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PreasurePad : MonoBehaviour
{
    [SerializeField] private GameObject _testCube;

    private Color _padOldColour = Color.magenta;

    public void ChangeColour()
    {
        Renderer render = GetComponent<Renderer>();
        _padOldColour = render.material.color;
        render.material.color = Color.cyan;
    }

    public void RevertColour()
    {
        Renderer render = GetComponent<Renderer>();
        render.material.color = _padOldColour;
    }
}
