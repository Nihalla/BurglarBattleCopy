using System;
using UnityEngine;

public class PlayerCameraLayer : MonoBehaviour
{
     [Header("Components")]
     [SerializeField] private PlayerProfile _playerProfile;
     [SerializeField] private Camera _camera;
     [SerializeField] private GameObject[] _playerMeshObjects = Array.Empty<GameObject>();
     
     [Header("Layers")]
     [SerializeField] [Layer] private int[] _playerLayers = Array.Empty<int>();

     private void Awake()
     {
          Debug.Assert(_playerProfile     != null, "Player Profile is null! Please assign one.");
          Debug.Assert(_camera            != null, "Camera is null! Please assign one.");
          Debug.Assert(_playerMeshObjects != null, "Player Mesh is null! Please assign one.");
     }

     private void Start()
     {
          int layer = _playerLayers[_playerProfile.GetPlayerID()];
          
          // Place the player mesh and all its children on the layer
          for (int i = 0; i < _playerMeshObjects.Length; i++)
          {
               GameObject playerMesh = _playerMeshObjects[i];
               playerMesh.layer = layer;
               
               for (int childIndex = 0; childIndex < playerMesh.transform.childCount; childIndex++)
               {
                    Transform child = playerMesh.transform.GetChild(childIndex);
                    child.gameObject.layer = layer;
               }
          }

          // Remove the player layer from the cameras culling mask
          _camera.cullingMask &= ~(1 << layer); 
     }
}
