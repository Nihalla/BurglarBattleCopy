// Joshua Weston

using UnityEngine;
using System;

public class ToolPickupController : MonoBehaviour, IInteractable
{
    public ToolsSO tool;

    private float _rotationSpeed = 100;
    [SerializeField] private Transform _toolSpawn;
    private MeshRenderer[] _meshRenderers = new MeshRenderer[1];

    void Start()
    {
        //_toolSpawn = GetComponentInChildren<Transform>();
        GameObject toolGO = Instantiate(tool.toolPrefab, _toolSpawn);
        MeshRenderer mr = toolGO.GetComponentInChildren<MeshRenderer>();

        _meshRenderers[0] = mr;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime));
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        // Swap players tool
        ToolController toolController = playerInteraction.GetComponent<ToolController>();
        toolController.DropTool();
        toolController.SwapTool(tool, tool.toolIndex);

        // Destroy gameObject
        Destroy(gameObject);
    }

    public void OnInteractHoverStarted(PlayerInteraction playerInteraction)
    {
        // Show UI for the tool (name and description)
        print(tool.name);
    }

    public void OnInteractHoverEnded(PlayerInteraction playerInteraction)
    {
        // Hide UI for the tool
    }
}
