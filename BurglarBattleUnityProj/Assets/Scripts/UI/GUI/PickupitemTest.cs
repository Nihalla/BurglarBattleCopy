using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupitemTest : MonoBehaviour
{
    private Inventory _inventory;
    public GameObject _itemImage;

    private void Start()
    {
        _inventory = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Inventory>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            for (int i = 0; i < _inventory._slots.Length; i++)
            {
                if (_inventory._isfull[i] == false)
                {
                    _inventory._isfull[i] = true;
                    Instantiate(_itemImage, _inventory._slots[i].transform, false);
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}
