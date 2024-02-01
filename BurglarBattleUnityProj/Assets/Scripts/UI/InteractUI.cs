using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UI;

// REVIEW(WSWhitehouse): Please follow the coding style in this script (specifically the serialized fields).
public class InteractUI : MonoBehaviour
{
    private const float MIN_CAM_DIST = 8.0f;
    
    [SerializeField] private bool HasInteract = false;
    //[SerializeField] private bool HasBack = false;
    [SerializeField] private string InteractableName = "";

    [SerializeField] private GameObject InteractableObjectName;
    [SerializeField] private GameObject InteractableObjectAccept;
    //[SerializeField] private GameObject InteractableObjectBack;
    //[SerializeField] private GameObject InteractableObjectbackground;
    [SerializeField] private GameObject InteractableObject;
    [Space]
    [SerializeField] private Shader _hoverShader;
    
    private int _hoverShaderHash = 0;
    private IInteractable _interactable = null;
    private List<Material> _hoverMaterials = new List<Material>(2);

    private void Start()
    {
        Debug.Assert(_hoverShader != null, "Interact UI: Hover Shader is null! Please assign one.", this);
        
        _hoverShaderHash = _hoverShader.name.GetHashCode();
        
        InteractableObjectName.GetComponent<TMP_Text>().text = InteractableName;
        //InteractableObjectbackground.SetActive(false);
        InteractableObjectAccept.SetActive(false);
        //InteractableObjectBack.SetActive(false);
        InteractableObjectName.SetActive(false);
        
        _interactable = InteractableObject.GetComponent<IInteractable>();
        Debug.Assert(
            _interactable != null, 
            $"Interact UI: {nameof(InteractableObject)} does not contain an IInteractable script! Please assign an IInteractable object.", 
            this
        );
    }

    private void LateUpdate()
    {
        bool foundCam = false;
        Camera nearestCamera = null;
        
        float distance = _interactable.GetInteractionDistance() + float.Epsilon;
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.enabled)
            {
                float camDist = Vector3.Distance(transform.position, cam.transform.position);
                if (camDist < distance)
                {
                    nearestCamera = cam;
                    foundCam      = true;
                    distance      = camDist;
                }
            }
        }
        
        if (!foundCam)
        {
            InteractableObjectName.SetActive(false);
            InteractableObjectAccept.SetActive(false);
            return;
        }
        
        bool ishighlight = false; // set to false by default

        // NOTE(WSWhitehouse): Going through the interaction mesh renderers to find if any are hovering, only need to
        // go through one of the renderers as if one contains the glow material they all do.
        Span<MeshRenderer> meshRenderers = _interactable.GetInteractionMeshRenderers();
        if (meshRenderers.Length > 0)
        {
            _hoverMaterials.Clear();
            meshRenderers[0].GetMaterials(_hoverMaterials);
            
            // NOTE(WSWhitehouse): Going through the materials in reverse as the glow mat is
            // likely to be at the end of the list, so this should be faster...
            for (int i = _hoverMaterials.Count - 1; i >= 0; i--)
            {
                Material material = _hoverMaterials[i];
                Shader shader = material.shader;
                int hash = shader.name.GetHashCode();

                if (_hoverShaderHash == hash)
                {
                    ishighlight = true;
                    break;
                }
            }
        }

        if (ishighlight)
        {
            //if (HasBack) InteractableObjectBack.SetActive(true);
            InteractableObjectName.SetActive(true);
            //InteractableObjectbackground.SetActive(true);
            InteractableObjectAccept.SetActive(true);
        }
        else
        {
            //InteractableObjectbackground.SetActive(false);
            InteractableObjectName.SetActive(false);
            InteractableObjectAccept.SetActive(false);
        }
        
        transform.LookAt(transform.position + nearestCamera.transform.rotation * Vector3.forward, nearestCamera.transform.rotation * Vector3.up);
    }
}
