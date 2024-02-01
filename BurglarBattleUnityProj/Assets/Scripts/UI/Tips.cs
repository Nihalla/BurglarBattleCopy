using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tips : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    private void Awake()
    {
        _text.text = Strings.tips[Random.Range(0, Strings.tips.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
