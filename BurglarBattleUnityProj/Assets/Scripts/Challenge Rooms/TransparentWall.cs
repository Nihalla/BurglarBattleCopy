using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentWall : MonoBehaviour
{
    [SerializeField] private MeshRenderer _myMeshRenderer;
    [SerializeField] private Material _baseWallMaterial;
    [SerializeField] private Material _transparentMaterial;

    [SerializeField] private InteractionButton _button;
    private bool _transparent = false;

    private void Start()
    {
        _button._onInteractEvent.AddListener(FlipMaterial);
    }

    private void FlipMaterial()
    {
        _transparent = !_transparent;
        if (_transparent)
        {
            _myMeshRenderer.sharedMaterial = _transparentMaterial;
        }
        else
        {
            _myMeshRenderer.sharedMaterial = _baseWallMaterial;
        }
    }
}
