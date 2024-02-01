using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SpinDiscGame : MonoBehaviour
{

    [SerializeField] private List<int> _startPositions;
    [SerializeField] private List<int> _rotationAmount;
    [SerializeField] private List<GameObject> _spinObjects;
    [SerializeField] private List<InteractionButton> _buttons;
    [SerializeField] private Material _liveWireMaterial;
    
    [SerializeField] private Material _offWireMaterial;

    [SerializeField] private List<MeshRenderer> _finalWireMeshRenderers;
    [SerializeField] private List<MeshRenderer> _wires1;
    [SerializeField] private List<MeshRenderer> _wires2;
    [SerializeField] private List<MeshRenderer> _wires3;
    [SerializeField] private List<MeshRenderer> _wires4;
    [SerializeField] private int _puzzleIndex;
    [SerializeField] private float _spinDuration;
    [SerializeField] private AudioSource3D _grindingStone;

    private int _buttonIndexHolder;

    private delegate IEnumerator RotationLerpDelegate(float duration, int objectIndex, Quaternion startRot, float3 endEuler);

    private RotationLerpDelegate RotateSpinObject;
    private Coroutine _rotateCoroutine;
    
    private void Start()
    {
        RotateSpinObject = ObjectRotation;
        for (int i = 0; i < _startPositions.Count; i++)
        {
            _spinObjects[i].transform.Rotate(new Vector3(0,_startPositions[i],0));
        }

     
        _buttons[0]._onInteractEvent.AddListener(FirstButton);
        _buttons[1]._onInteractEvent.AddListener(SecondButton);
        _buttons[2]._onInteractEvent.AddListener(ThirdButton);
        _buttons[3]._onInteractEvent.AddListener(FourthButton);

 
    }

    private void FirstButton()
    {
        RotateObjects(0);
    }
    
    private void SecondButton()
    {
        RotateObjects(1);
    }
    
    private void ThirdButton()
    {
        RotateObjects(2);
    }
    
    private void FourthButton()
    {
        RotateObjects(3);
    }

    private void CheckCompletion()
    {
        int completed = 0;
        for (int i = 0; i < _spinObjects.Count; i++)
        {
            if (_spinObjects[i].transform.localRotation.y <= 0.1 && _spinObjects[i].transform.localRotation.y >= -0.1 )
            {
               ChangeWire(i,_liveWireMaterial);
                completed++;
            }
            else
            {
                ChangeWire(i,_offWireMaterial);
            }
        }

        if (completed == _spinObjects.Count)
        {
            SetFinalWireToLive();
            
            _buttons[0]._onInteractEvent.RemoveListener(FirstButton);
            _buttons[1]._onInteractEvent.RemoveListener(SecondButton);
            _buttons[2]._onInteractEvent.RemoveListener(ThirdButton);
            _buttons[3]._onInteractEvent.RemoveListener(FourthButton);
            GlobalEvents.OnVaultPuzzles(_puzzleIndex);
            //Debug.Log("Completed");
        }
    }
    private void RotateObjects(int buttonIndex)
    {
        if (_rotateCoroutine == null)
        {
            for (int i = 0; i <= buttonIndex; i++)
            {
                _rotateCoroutine = StartCoroutine(RotateSpinObject(_spinDuration,i,_spinObjects[i].transform.localRotation,new float3(0,_rotationAmount[i],0)));
            }
        }

    }

    private IEnumerator ObjectRotation(float duration, int objectIndex,Quaternion startRot, float3 endEuler)
    {
        float _t = 0f;
        Quaternion start = startRot;
        Quaternion end = startRot * Quaternion.Euler(endEuler);
        float timer = float.Epsilon;
        _grindingStone.Play();
        while (timer < duration)
        { 
            _t = timer / duration;
            _spinObjects[objectIndex].transform.localRotation = Quaternion.Lerp(start, end, _t);
            timer += Time.deltaTime;
            yield return null;
        }
        _grindingStone.Stop();

        _rotateCoroutine = null;
        _spinObjects[objectIndex].transform.localRotation = end;
        CheckCompletion();
            yield break;

    }


    private void ChangeWire(int wire, Material material)
    {
        if (wire == 0)
        {
            for (int i = 0; i < _wires1.Count; i++)
            {
                _wires1[i].material = material;
            }
        } 
        if (wire == 1)
        {
            for (int i = 0; i < _wires2.Count; i++)
            {
                _wires2[i].material = material;
            }
        } 
        if (wire == 2)
        {
            for (int i = 0; i < _wires3.Count; i++)
            {
                _wires3[i].material = material;
            }
        } 
        if (wire == 3)
        {
            for (int i = 0; i < _wires4.Count; i++)
            {
                _wires4[i].material = material;
            }
        }
    }

    
    private void SetFinalWireToLive()
    {
        for (int i = 0; i < _finalWireMeshRenderers.Count; i++)
        {
            _finalWireMeshRenderers[i].material = _liveWireMaterial;

        }
    }
}
