using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    [SerializeField] private Color _tintColor;
    [SerializeField] private Color _transparentColor;
    [SerializeField] private RectTransform _transform;
    [SerializeField] private Image _image;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetRotation(float value)
    {
        _transform.localEulerAngles = new Vector3(0.0f,0.0f, -value);
    }

    public void ToggleIndicator(bool value)
    {
        _image.enabled = value;
    }
}
