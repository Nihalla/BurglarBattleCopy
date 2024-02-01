using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoblinThief : MonoBehaviour
{
    private bool _isHoldingGold = false;
    private NavMeshAgent _agent;

    private Transform _playerChasingTransform;
    public Transform PlayerChasing { set { _playerChasingTransform = value; } }

    private GoblinThiefManager _manager;
    public GoblinThiefManager Manager { set { _manager = value; } }


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (_isHoldingGold)
        {
            _agent.destination = _manager.StartPosition.transform.position;

            if (Vector3.Distance(this.transform.position, _manager.StartPosition.transform.position) < 1)
            {
                _manager.CloseDoor();
                Destroy(gameObject);
            }
            //go back home
        }
        else
        {
            //go towards the player
            _agent.destination = _playerChasingTransform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.transform == _playerChasingTransform)
        {
           // //Debug.Log($"collided with the player");
            //FirstPersonController controller_script = collision.transform.GetComponent<FirstPersonController>();

            _isHoldingGold = true;
        }

        //GameObject other_object = collision.gameObject;

        //if ((1 << other_object.layer) != 0)
        //{
        //    //Debug.Log($"collided with the player");
        //    FirstPersonController controller_script = other_object.GetComponent<FirstPersonController>();

        //    _isHoldingGold = true;
        //}
    }
}
