//Author: Norbert Kupeczki - 19040948

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PuzzleDoorHintDecal : MonoBehaviour
{
    private DecalProjector _projector;
    private Material _matInstance;

    private void Awake()
    {
        _projector = GetComponent<DecalProjector>();
        _matInstance = Instantiate(_projector.material);
        _projector.material = _matInstance;
    }

    public void SetDecalColour(Color colour)
    {
        _projector.material.SetColor("_DecalColour", colour);
    }

    public void DisableProjector()
    {
        _projector.enabled = false;
    }
}
