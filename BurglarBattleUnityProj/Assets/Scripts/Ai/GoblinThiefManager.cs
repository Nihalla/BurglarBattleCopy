using PlayerControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinThiefManager : MonoBehaviour
{
    public bool open = false;
    public bool close = false;

    [SerializeField] GameObject _goblinThiefObj;
    [SerializeField] GameObject _doorPivot;
    [SerializeField] GameObject _startPosition;
    public GameObject StartPosition { get { return _startPosition; } }

    public float _duration = 1f;   //duration of animation
    private float _elapsedTime = 0f;
    private bool _isRotating = false;   // is the door currently rotating
    private bool _isOpen = false;   // is the door open

    private bool _outHunting = false;  // the goblin is out of the nest


    public float _attackChance = 0.15f;    //chance of goblin going for the player
    private float timer = 0f;  //hunt decision timer
    private float interval = 0.5f; // every "interval" the golbin asks it sellf if it should go hunting 


    [SerializeField] private LayerMask _playerLayer;
    private List<FirstPersonController> _players = new List<FirstPersonController>();

    private void Update()
    {

        //to delete
        if (open)
        {
            open = false;
            OpenDoor();
        }

        //to delete
        if (close)
        {
            close = false;
            CloseDoor();
        }

        ////Debug.Log(_players.Count);

        if (_players.Count > 0)
        {
            //if there is a player in the list

            timer += Time.deltaTime;

            if (!_isRotating || !_isOpen)
            {
                if (timer >= interval)
                {
                    if (Random.value <= _attackChance)
                    {
                        ////Debug.Log($"going for it");
                        //Open the door
                        OpenDoor();
                    }
                    else
                    {
                        ////Debug.Log($"not going for it");
                    }

                    timer = 0f;
                }
            }
        }

        if (_isRotating)
        {
            _elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(_elapsedTime / _duration);
            float angle = Mathf.Lerp(_isOpen ? 0 : 90, _isOpen ? 90 : 0, t);

            _doorPivot.transform.rotation = Quaternion.Euler(0, angle, 0);

            if (t >= 1f)
            {
                _isRotating = false;
                _elapsedTime = 0f;

                if (_isOpen)
                {
                    ////Debug.Log($"This is called when it finished opening");
                    // once the door is opened
                    _outHunting = true;
                    var goblin = Instantiate(_goblinThiefObj, _startPosition.transform.position, Quaternion.identity);
                    var goblinComp = goblin.GetComponent<GoblinThief>();

                    goblinComp.Manager = this;
                    goblinComp.PlayerChasing = _players.Count == 1 ? _players[0].transform : _players[Random.Range(0, _players.Count)].transform;

                }
                else
                {
                   // //Debug.Log($"This is called when the door closes");
                    _outHunting = false;
                }
            }
        }


    }


    private void OpenDoor()
    {
        if (!_isRotating && !_isOpen)
        {
            _isRotating = true;
            _isOpen = true;
        }
    }
    public void CloseDoor()
    {
        if (!_isRotating && _isOpen)
        {
            _isRotating = true;
            _isOpen = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        GameObject other_object = other.gameObject;

        if ((_playerLayer & (1 << other_object.layer)) != 0)
        {
            FirstPersonController controller_script = other_object.GetComponent<FirstPersonController>();

            if (controller_script != null)
            {
                _players.Add(controller_script);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject other_object = other.gameObject;

        if ((_playerLayer & (1 << other_object.layer)) != 0)
        {
            _players.Remove(other_object.GetComponent<FirstPersonController>());
        }
    }

}
