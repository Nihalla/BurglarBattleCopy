// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerControllers;

public class DroppedLoot : MonoBehaviour
{
    public LootSelector loot;
    [SerializeField] private LayerMask groundLayer;
    public float sphereRadius = 1;
    public int _value;
    private float _rotationSpeed = 100;

    void Start()
    {
        transform.localScale.Set(loot.lootAmount * 5, loot.lootAmount * 5, loot.lootAmount * 5);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Loot>())
        {
            Loot loot = other.GetComponent<Loot>();

            /* If value is more than space then don't pickup*/
            if (loot.currentLoot + _value < loot.GetMaximumLoot())
            {
                loot.currentLoot += _value;
                /*Destroy gameObject*/
                Destroy(gameObject);
            }
        }
    }

    public void SetDroppedLootValue(int droppedAmount)
    {
        _value = droppedAmount;
    }
}
