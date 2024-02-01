// Joshua Weston

using UnityEngine;
using PlayerControllers;
using Unity.Mathematics;

public class CoinController : MonoBehaviour
{
    // Set by other scrits when loot is spawned
    [SerializeField] private LootSelector _loot;
    private int _value;

    private float _rotationSpeed = 100;
    [SerializeField] private Audio lootPickupSound;

    // NOTE(Zack): we are making a property to be able to clamp the value of the loot to a positive value
    public int Value
    {
        get => _value;
        set => _value = math.max(0, value);
    }

    public LootSelector Loot => _loot;

    private void Start()
    {
        Instantiate(_loot.lootModel, transform);
        Value = _loot.lootAmount;
        transform.localScale.Set(_loot.lootAmount*5, _loot.lootAmount*5, _loot.lootAmount*5);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * (_rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Loot loot))
        {
            /* If value is more than space then don't pickup*/
            if ((loot.GetCurrentLoot() + _value) <= loot.GetMaximumLoot())
            {
                loot.SetCurrentLoot(loot.GetCurrentLoot() + _value);
                AudioManager.PlayScreenSpace(lootPickupSound);
                Destroy(gameObject);
            }
        }
    }

    public void SetLoot(LootSelector loot)
    {
        _loot = loot;
        _value = _loot.lootAmount;
    }
}
