using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpinWheel : MonoBehaviour
{
    [Header("GameObject References")]
    [SerializeField] private GameObject _pointer;
    [SerializeField] private GameObject _wheel;
    [Space]
    [Header("Wheel Spinning Settings")]
    [SerializeField] private float _spinSpeedMin = 100f;
    [SerializeField] private float _spinSpeedMax = 150f;

    [Space]
    [Header("Rewards")]
    //[SerializeField] private GameObject _chest;
    [SerializeField] private GameObject _goldLoot;
    [SerializeField] private GameObject _masterKey;
    [SerializeField] private GameObject _guardShield;

    [Space] [Header("Reward Settings")] 
    [SerializeField] private Transform _rewardSpawnLocation1;
    [SerializeField] private Transform _rewardSpawnLocation2;
    [SerializeField] private float _chestUnlockDelay = 0.5f;
    
    [Space]
    [Header("Button Settings")]
    [SerializeField] private GameObject _button;
    [SerializeField] private Material _buttonOffMaterial;

    private GameObject _currentReward1;
    private GameObject _currentReward2;
    

    private float _subtractionSpeedMin = 0.1f;
     private float _subtractionSpeedMax = 0.5f;

    private float _spinSpeed;
    private float _subtractionSpeed;
    
    private bool _isSpinning;

    private float _wheelRotationZ;
    
    private Coroutine _spinCoroutine;

    private ChestController _chestController;

        // <180 && >135 = 1x Chest
        //
        // <135 && >90 = 2x Chest
        //
        // <90 && >45 = Guard Shield
        //
        // <45 && >0 = Master Key
        //
        // <0 && >-45 = 1x Chest
        //
        // <-45 && >-90 = 2x Chest
        //
        // <-90 && >-135 = Guard Shield
        //
        // <-135 && >-180 = masterkey


    // >0 && <45 = 2x Chest
    // >45 && <90 = 1x Chest
    // >90 && <135 = Master Key
    // >135 && <180 = Guard Shield
    // >180 && <225 = 2x Chest
    // >225 && <270 = 1x Chest
    // >270 && <315 = Master Key
    // >315 && <360 = Guard Shield

    public void StartSpin()
    {
        if (_isSpinning)
        {
            return;
        }
        //Note(Sebadam2010): Changing button colour to red as it is a single use button, if that changes, this will need to be changed.
        _button.GetComponent<MeshRenderer>().material = _buttonOffMaterial;
        
        _spinSpeed = Random.Range(_spinSpeedMin, _spinSpeedMax);
        _subtractionSpeed = Random.Range(_subtractionSpeedMin, _subtractionSpeedMax);
        
        if (_spinCoroutine != null)
        {
            StopCoroutine(_spinCoroutine);
        }
        _spinCoroutine = StartCoroutine(SpinWheelCoroutine());
    }
    
    private IEnumerator SpinWheelCoroutine()
    {
        float time = 0;
        
        _isSpinning = true;
        
        while (_spinSpeed > 0)
        {
            _spinSpeed -= _subtractionSpeed;

            _wheel.transform.Rotate(0,0 , _spinSpeed * Time.deltaTime, Space.World);
            
            // if (_wheel.transform.rotation.eulerAngles.x > 359.99999f)
            // {
            //     
            //     _wheel.transform.rotation = Quaternion.Euler(0, 0, 0);
            // }
            
            time += Time.deltaTime;
            yield return null;
        }

        //_wheelRotationZ = _wheel.transform.rotation.normalized.eulerAngles.x;
        _wheelRotationZ = _wheel.transform.eulerAngles.z;

        CheckPrize();
        _spinSpeed = 0;
        _isSpinning = false;
    }
    
    private void CheckPrize()
    {
        //TODO(Sebadam2010): Make the instantations of rewards into a function to avoid code duplication.

        if (_wheelRotationZ > 0 && _wheelRotationZ < 45)
        {
            //Debug.Log("Guard Shield");
            _currentReward1 = Instantiate(_guardShield, _rewardSpawnLocation1.position, Quaternion.identity);
        }
        else if (_wheelRotationZ > 45 && _wheelRotationZ < 90)
        {
            //Debug.Log("Master Key");
            _currentReward1 = Instantiate(_masterKey, _rewardSpawnLocation1.position, Quaternion.identity);
        }
        else if (_wheelRotationZ > 90 && _wheelRotationZ < 135)
        {
            //Note(Sebadam2010): Switched from chest to gold as there is no room to spawn a chest in Room13 as otherwise it will spawn inside the player.
            
            //Debug.Log("1x Gold loot");
            _currentReward1 = Instantiate(_goldLoot, _rewardSpawnLocation1.position, Quaternion.identity);
            _currentReward1.GetComponent<CoinController>().Value = 5;
            
            //SpawnChests(1);
        }
        else if (_wheelRotationZ > 135 && _wheelRotationZ < 180)
        {
            //Note(Sebadam2010): Switched from chest to gold as there is no room to spawn a chest in Room13 as otherwise it will spawn inside the player.

            //Debug.Log("2x Gold loot");
            _currentReward1 = Instantiate(_goldLoot, _rewardSpawnLocation1.position, Quaternion.identity);
            _currentReward1.GetComponent<CoinController>().Value = 5;
            
            _currentReward2 = Instantiate(_goldLoot, _rewardSpawnLocation2.position, Quaternion.identity);
            _currentReward2.GetComponent<CoinController>().Value = 5;
            
            //SpawnChests(2);
        }
        else if (_wheelRotationZ > 180 && _wheelRotationZ < 225)
        {
            //Debug.Log("Guard Shield");
            _currentReward1 = Instantiate(_guardShield, _rewardSpawnLocation1.position, Quaternion.identity);
        }
        else if (_wheelRotationZ > 225 && _wheelRotationZ < 270)
        {
            //Debug.Log("Master Key");
            _currentReward1 = Instantiate(_masterKey, _rewardSpawnLocation1.position, Quaternion.identity);
        }
        else if (_wheelRotationZ > 270 && _wheelRotationZ < 315)
        {
            //Note(Sebadam2010): Switched from chest to gold as there is no room to spawn a chest in Room13 as otherwise it will spawn inside the player.
            
            //Debug.Log("1x Gold loot");
            _currentReward1 = Instantiate(_goldLoot, _rewardSpawnLocation1.position, Quaternion.identity);
            _currentReward1.GetComponent<CoinController>().Value = 5;
            
                //SpawnChests(1);

        }
        else if (_wheelRotationZ > 315 && _wheelRotationZ < 360)
        {
            //Note(Sebadam2010): Switched from chest to gold as there is no room to spawn a chest in Room13 as otherwise it will spawn inside the player.
            
            //Debug.Log("2x Gold loot");
            _currentReward1 = Instantiate(_goldLoot, _rewardSpawnLocation1.position, Quaternion.identity);
            _currentReward1.GetComponent<CoinController>().Value = 5;
            
            _currentReward2 = Instantiate(_goldLoot, _rewardSpawnLocation2.position, Quaternion.identity);
            _currentReward2.GetComponent<CoinController>().Value = 5;
            
            //SpawnChests(2);
        }
    }

    private void SpawnChests(int amountToSpawn)
    {
        if (amountToSpawn == 1)
        {
            //_currentReward1 = Instantiate(_chest, _rewardSpawnLocation1.position, _rewardSpawnLocation1.rotation);

            //OpenChest(_currentReward1.GetComponent<ChestController>());
        }
        else if (amountToSpawn == 2)
        {
           // _currentReward1 = Instantiate(_chest, _rewardSpawnLocation1.position, _rewardSpawnLocation1.rotation);
           // _currentReward2 = Instantiate(_chest, _rewardSpawnLocation2.position, _rewardSpawnLocation2.rotation);

           // OpenChest(_currentReward1.GetComponent<ChestController>());
           // OpenChest(_currentReward2.GetComponent<ChestController>());
        }
    }
    
    private void OpenChest(ChestController _chestController)
    {
        StartCoroutine(OpeningChestDelay(_chestController, _chestUnlockDelay));
    }
    
    private IEnumerator OpeningChestDelay(ChestController _chestController, float delay)
    {
        float _timer = 0f;

        while (_timer < delay)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        
        _chestController.UnlockChest();
    }

    public void RemovePrizes()
    {
        if (_currentReward1 != null)
        {
            Destroy(_currentReward1);
        }
        if (_currentReward2 != null)
        {
            Destroy(_currentReward2);
        }
    }
}
