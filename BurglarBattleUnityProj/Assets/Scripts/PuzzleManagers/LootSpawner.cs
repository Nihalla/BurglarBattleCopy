using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _loot;
    [SerializeField] private LootSelector _lootType;
    [SerializeField] private Transform _lootLocation;

    // Start is called before the first frame update
    void Awake()
    {
        if (_loot.GetComponent<CoinController>())
        {
            _loot.GetComponent<CoinController>().SetLoot(_lootType);
        }
    }

    public void SpwanLoot()
    {
        Instantiate(_loot, _lootLocation.transform.position, _lootLocation.transform.rotation);
    }
}
