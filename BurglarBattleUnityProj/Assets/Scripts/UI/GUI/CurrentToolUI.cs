using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CurrentToolUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _toolInfo;
    public Image _toolSprite;
    [SerializeField] Sprite _emptyImage;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdateToolUI(string toolDesc, Sprite toolImage)
    {
        _toolInfo.text = toolDesc;
        _toolSprite.sprite = toolImage;
    }

    public void EmptyToolUI()
    {
        _toolInfo.text = "";
        _toolSprite.sprite = _emptyImage;
    }

}
