// Joshua Weston

using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public ToolsSO activeTool;
    public int activeToolIndex;
    public GameObject[] tools;
    [Space]
    public float nearbyRange;
    public GameObject toolHolder;
    public Camera cam;
    public LayerMask groundLayerMask;
    public LayerMask ignoreLayerMask;

    public GameObject toolPickup;

    private int _usesLeft = 0;
    private InputActionsInputs _inputs;

    [SerializeField] private CurrentToolUI _toolUI;

    private void Start()
    {
        _inputs = GetComponent<InputActionsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_inputs.useTool && activeTool && _usesLeft > 0)
        {
            // Get Player look point
            RaycastHit hit;
            bool hasHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f, ~ignoreLayerMask) && hit.collider.gameObject.layer == 3;

            Collider[] colliders = Physics.OverlapSphere(transform.position, nearbyRange);

            // gets a list of all nearby objects for use in tools
            List<GameObject> nearbyObjects = new List<GameObject>();
            foreach (Collider collider in colliders)
            {
                nearbyObjects.Add(collider.gameObject);
            }

            // Checks to see if the tool can be used
            if (tools[activeToolIndex].GetComponent<ITool>().CanBeUsed(nearbyObjects, hasHit))
            {
                _usesLeft--;
            }

            // Call use function on tool object
            tools[activeToolIndex].GetComponent<ITool>().Use(nearbyObjects, gameObject, tools[activeToolIndex], hit, hasHit);
        }

        // removes tools if there are no charges left
        if (_usesLeft <= 0)
        {
            _toolUI.EmptyToolUI();
            activeTool = null;
            tools[activeToolIndex].SetActive(false);
        }

       
    }

    public void SwapTool(ToolsSO newTool, int index)
    {
        _usesLeft = newTool.toolUses;
        activeTool = newTool;

        tools[activeToolIndex].SetActive(false);
        tools[index].SetActive(true);
        activeToolIndex = index;

        _toolUI.UpdateToolUI(activeTool.toolDescription, activeTool.toolSprite);
    }

    public void DropTool()
    {
        if(activeTool)
        {
            GameObject toolController = Instantiate(toolPickup, transform);
            toolController.transform.parent = null;
            toolController.GetComponent<ToolPickupController>().tool = activeTool;
            _toolUI.EmptyToolUI();
            activeTool = null;
            _usesLeft = 0;
        }
    }
}
