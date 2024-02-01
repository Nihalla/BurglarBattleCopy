using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionScript : MonoBehaviour
{
    [SerializeField] private string selectable = "Selectable";
    private string _tresure = "Treasure";

    [SerializeField] private Button ButtonName;
    [SerializeField] private Button ButtonDescription;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI description;
    public Camera camera;


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 5;
        Gizmos.DrawRay(transform.position, direction);
    }

    private Transform _selection;
    void Update()
    {

        if (_selection != null)
        {
            var selectionrender = _selection.GetComponent<Renderer>();
            if (selectionrender != null)
            {
                //var outline = _selection.GetComponent<Outline>();
                //outline.OutlineWidth = 0;
            }
        }

        var ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        _selection = null;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform;
            text.text = "";
            description.text = "";
            ButtonName.image.color = new Color(ButtonName.colors.normalColor.r, ButtonName.colors.normalColor.g, ButtonName.colors.normalColor.b, 0f);
            ButtonDescription.image.color = new Color(ButtonDescription.colors.normalColor.r, ButtonDescription.colors.normalColor.g, ButtonDescription.colors.normalColor.b, 0f);

            if (selection.GetComponent<ObjectInfomation>() != null)
            {
                if (selection.GetComponent<ObjectInfomation>().Name != null)
                {
                    ButtonName.image.color = new Color(ButtonName.colors.normalColor.r, ButtonName.colors.normalColor.g, ButtonName.colors.normalColor.b, 1f);
                    var name = selection.GetComponent<ObjectInfomation>().Name;
                    text.text = name;
                    _selection = selection;
                    if (selection.GetComponent<ObjectInfomation>().Description != null)
                    {
                        ButtonDescription.image.color = new Color(ButtonDescription.colors.normalColor.r, ButtonDescription.colors.normalColor.g, ButtonDescription.colors.normalColor.b, 1f);
                        var Description = selection.GetComponent<ObjectInfomation>().Description;
                        description.text = Description;
                    }
                }
            }

            else if (selection.CompareTag(selectable) || selection.CompareTag(_tresure))
            {
                ButtonName.image.color = new Color(ButtonName.colors.normalColor.r, ButtonName.colors.normalColor.g, ButtonName.colors.normalColor.b, 1f);
                var name = selection.gameObject.name;
                text.text = name;
                _selection = selection;
            }
        }

        if (_selection != null)
        {
            var selectionrender = _selection.GetComponent<Renderer>();
            if (selectionrender != null)
            {
                //var outline = _selection.GetComponent<Outline>();
                //outline.OutlineWidth = 10;
            }
        }
    }
}
