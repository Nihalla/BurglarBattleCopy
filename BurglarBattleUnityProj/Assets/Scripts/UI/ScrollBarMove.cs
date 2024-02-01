using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarMove : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 0.1f;
    private Scrollbar _scrollbar;
    private bool _scrollingDown = true;

    // Start is called before the first frame update
    void Start()
    {
        _scrollbar = GetComponent<Scrollbar>();
        _scrollbar.value = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeInHierarchy == false)
        {
            return;
        }

        if (_scrollbar.value > 0f && _scrollingDown)
        {
            _scrollbar.value -= _scrollSpeed;
        }
        else if (_scrollbar.value <= 0f)
        {
            _scrollingDown = false;
        }

        if (_scrollbar.value < 1f && !_scrollingDown)
        {
            _scrollbar.value += _scrollSpeed;
        }
        else if (_scrollbar.value >= 1f)
        {
            _scrollingDown = true;
        }
    }
}
